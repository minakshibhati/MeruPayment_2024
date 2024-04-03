using System;
using System.Linq;
using MeruPaymentBO.Razoypay;
using MeruPaymentDAL.DAL;
using MeruPaymentBO;
using System.Configuration;
using MeruPaymentCore;
using System.Collections.Generic;
using Newtonsoft.Json;
using MeruCommonLibrary;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using MeruPaymentDAL;

namespace MeruPaymentBAL
{
    public class RazorpayWebhookBAL : IDisposable
    {

        #region Fileds and Properties
        private LogHelper logger;
        private string[] resourcesToProcess;
        private PaymentDAL paymentDataManager;
        private PaymentBO paymentDetails;
        private PaymentLinkBO dbPaymentLinkDetails;
        private Razorpay razorManager;
        #endregion

        public RazorpayWebhookBAL()
        {
            resourcesToProcess = ConfigurationManager.AppSettings["ResourcesToProcess"].ToString().Split(',');
            logger = new LogHelper("RazorpayWebhookBAL");
        }

        #region Public Fields and Methods

        public void ProcessPaymentAuthorized(PaymentAuthorized paymentAuthorized)
        {
            try
            {
                logger.MethodName = "UpdatePaymentAuthorized(PaymentAuthorized paymentAuthorized)";
                dynamic Notes = null;
                Notes = paymentAuthorized.PayLoad.Payment.Entity.Notes;
                if (Notes == null || Notes.Count == 0)
                {
                    logger.WriteWarn("Request does not contanin any Notes");
                    return;
                }
                string PaymentTransactionId = paymentAuthorized.PayLoad.Payment.Entity.Notes["Meru_PaymentId"];
                PaymentDAL objPatjymentDAL = new PaymentDAL();
                PaymentBO dbPaymentDetails = null;
                Regex regex = new Regex("^[0-9]+$");
                if (regex.IsMatch(PaymentTransactionId))
                {
                    dbPaymentDetails = objPatjymentDAL.GetMeruPaymentDetailOld(Convert.ToInt32(PaymentTransactionId));
                }
                else
                {
                    dbPaymentDetails = objPatjymentDAL.GetMeruPaymentDetail(PaymentTransactionId);
                }
                if (Array.IndexOf(resourcesToProcess, dbPaymentDetails.RequestSource) < 0)
                {
                    logger.WriteInfo("No need to process data for Resource : " + dbPaymentDetails.RequestSource);
                    return;
                }
                if (dbPaymentDetails.PaymentStatus == PaymentStatus.PaymentSuccess || dbPaymentDetails.PaymentStatus == PaymentStatus.PaymentSuccessViaLink)
                {
                    logger.WriteInfo("Payment Status is already " + dbPaymentDetails.PaymentStatus.ToString() + " for " + PaymentTransactionId);
                    return;
                }

                string mob = dbPaymentDetails.Mobile;
                if (mob == null)
                {
                    logger.WriteInfo("moble number is null "+ " for " + PaymentTransactionId);

                    mob = "";
                }

                if (dbPaymentDetails.Amount == paymentAuthorized.PayLoad.Payment.Entity.Amount
                    && dbPaymentDetails.PaymentReferenceData1 == paymentAuthorized.PayLoad.Payment.Entity.OrderId
                    && (paymentAuthorized.PayLoad.Payment.Entity.Contact.Contains(mob) 
                    ))
                {
                    razorManager = new Razorpay();
                    var Response = razorManager.GetPaymentDetailByOrderId(dbPaymentDetails.PaymentReferenceData1);
                    if (Response == null)
                    {
                        logger.WriteWarn("Unable to find order details in Razorpay for order Id: " + dbPaymentDetails.PaymentReferenceData1);
                        return;
                    }

                    if (Response.PaymentStatus == PaymentStatus.PaymentAuthorised)
                    {
                        if (razorManager.CapturePayment(paymentAuthorized.PayLoad.Payment.Entity.Id, dbPaymentDetails.Amount.ToString()))
                        {
                            Response = razorManager.GetPaymentDetailByOrderId(dbPaymentDetails.PaymentReferenceData1);
                        }
                    }

                    if (Response.PaymentStatus != dbPaymentDetails.PaymentStatus)
                    {
                        Response.MeruPaymentId = dbPaymentDetails.PaymentTransactionId;
                        using (UpdatePaymentDetailsWebhookDAL updater = new UpdatePaymentDetailsWebhookDAL())
                        {
                            logger.WriteInfo("UpdatePaymentDetails for " + PaymentTransactionId);

                            if (regex.IsMatch(PaymentTransactionId))
                            {
                                logger.WriteInfo("UpdatePaymentDetails for " + JsonConvert.SerializeObject(Response));
                                updater.UpdatePaymentDetailsOld(Response);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(paymentAuthorized.PayLoad.Payment.Entity.InvoiceId)))
                                {
                                    Response.PaymentStatus = PaymentStatus.PaymentSuccessViaLink;
                                }
                                updater.UpdatePaymentDetails(Response, "Webhook authorized");
                                logger.WriteInfo("UpdatePaymentDetails Webhook authorized " + JsonConvert.SerializeObject(Response));
                            }
                            dbPaymentDetails.PaymentReferenceData2 = Response.PaymentId;
                            dbPaymentDetails.PaymentSource = PaymentGatway.Razorpay;
                        }
                    }

                    if (Response.PaymentStatus == PaymentStatus.PaymentSuccess)
                    {
                        PushToQueueAndExecuteProcedure(dbPaymentDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, "Exception Occured while manipulating Razor Payment");
            }
        }

        public void ProcessInvoicePaid(InvoicePaid invoicePaid)
        {
            try
            {
                logger.MethodName = "ProcessInvoicePaid(InvoicePaid invoicePaid)";
                //JObject InputNotes = invoicePaid.PayLoad.Invoice.Entity.Notes;
                //if (InputNotes == null || InputNotes.Count == 0)
                //{
                //    logger.WriteWarn("Request does not contain ant notes");
                //    return;
                //}

                string MeruPaymentId = invoicePaid.PayLoad.Invoice.Entity.Receipt;
                using (PaymentLinkDAL paymentLinkManager = new PaymentLinkDAL())
                {
                    dbPaymentLinkDetails = paymentLinkManager.GetPaymentLinkDetails(MeruPaymentId);
                    paymentDataManager = new PaymentDAL();
                }

                if (dbPaymentLinkDetails == null)
                {
                    logger.WriteWarn("No record found for Meru Payment Id : " + MeruPaymentId);
                    return;
                }
                paymentDetails = paymentDataManager.GetMeruPaymentDetail(dbPaymentLinkDetails.Payment_Transaction_ID);

                if (dbPaymentLinkDetails.Status == (int)PaymentStatus.PaymentSuccess)
                {
                    logger.WriteInfo("Payment Status is already Success for " + MeruPaymentId);
                    return;
                }

                if (Array.IndexOf(resourcesToProcess, paymentDetails.RequestSource) < 0)
                {
                    return;
                }

                if (dbPaymentLinkDetails.Payment_Amount_Paise == invoicePaid.PayLoad.Invoice.Entity.Amount)
                {
                    using (razorManager = new Razorpay())
                    {
                        var Response = razorManager.GetPaymentDetailByOrderId(dbPaymentLinkDetails.PG_OrderId);
                        if (Response == null)
                        {
                            logger.WriteWarn("Unable to find order details in Razorpay for order Id: " + dbPaymentLinkDetails.PG_OrderId);
                            return;
                        }
                        Response.MeruPaymentId = dbPaymentLinkDetails.Payment_Transaction_ID;
                        if (Response.PaymentStatus == PaymentStatus.PaymentSuccess && (int)Response.PaymentStatus != dbPaymentLinkDetails.Status)
                        {
                            //using (UpdatePaymentDetailsWebhookDAL updater = new UpdatePaymentDetailsWebhookDAL())
                            //{
                            //    updater.UpdatePaymentDetails(Response, "Webhook invoice paid");
                            //}
                            using (PaymentLinkDAL paymentLinkManager = new PaymentLinkDAL())
                            {
                                using (UpdatePaymentDetailsWebhookDAL updater = new UpdatePaymentDetailsWebhookDAL())
                                {
                                    Response.PaymentStatus = PaymentStatus.PaymentSuccessViaLink;
                                    updater.UpdatePaymentDetails(Response, "Webhook invoice paid");
                                    paymentDetails.PaymentReferenceData2 = Response.PaymentId;
                                }
                                if (paymentDetails.PaymentMethod == PaymentMethod.card || paymentDetails.PaymentMethod == PaymentMethod.debit || paymentDetails.PaymentMethod == PaymentMethod.credit)
                                {
                                    using (Razorpay objRazorpay = new Razorpay())
                                    {
                                        RazorpayCardBO cardDetails = objRazorpay.GetCardDetail(Response.PaymentMethodDetail);
                                        if (cardDetails != null)
                                        {
                                            if (cardDetails.CardType == "credit")
                                            {
                                                Response.PaymentMethod = PaymentMethod.credit;    
                                            }
                                            if (cardDetails.CardType == "debit")
                                            {
                                                Response.PaymentMethod = PaymentMethod.debit;
                                            }
                                            
                                            Response.PaymentMethodDetail = Newtonsoft.Json.JsonConvert.SerializeObject(cardDetails, Formatting.None);
                                        }
                                    }
                                }
                                paymentLinkManager.UpdatePaymentlinkDetails(Response, "Webhook invoice paid");
                            }
                            paymentDetails.PaymentSource = PaymentGatway.Razorpay;
                            PushToQueueAndExecuteProcedure(paymentDetails);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, "Exception Occured while procesing Invoice Paid event.");
            }
        }

        #endregion

        #region Private Methods

        private void PushToQueueAndExecuteProcedure(PaymentBO dbPaymentDetails)
        {
            PaymentRequestSystemMasterBO dbSourceDetails = new PaymentRequestSystemMasterBO();

            #region Model Processing
            pragatiQueue queueObject = new pragatiQueue();
            try
            {
                logger.MethodName = "PushToQueueAndExecuteProcedure(string requestResouce,PaymentBO dbPaymentDetails)";
                RazorCheckoutResponseBAL objRazorCheckoutResponseBAL = new RazorCheckoutResponseBAL();
                dbSourceDetails = objRazorCheckoutResponseBAL.GetSourceDetail(dbPaymentDetails.RequestSource);
                if (dbPaymentDetails.RequestReferenceVal != null)
                {
                    dynamic RequestReferenceVal = JObject.Parse(dbPaymentDetails.RequestReferenceVal);
                    queueObject.CarNo = RequestReferenceVal.CarNo;
                    queueObject.SPId = RequestReferenceVal.SPId;
                }
                queueObject.MeruPaymentId = dbPaymentDetails.PaymentTransactionId;
                queueObject.Amount = dbPaymentDetails.Amount;
                queueObject.PaymentMethod = dbPaymentDetails.PaymentMethod.ToString();
                queueObject.PaymentSource = dbPaymentDetails.PaymentSource.ToString();
                queueObject.PaymentId = dbPaymentDetails.PaymentReferenceData2;
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, "Exception Occured during binding data to pragatiQueue");
            }
            #endregion

            #region Add Using SP
            if (!string.IsNullOrWhiteSpace(dbSourceDetails.SPName))
            {
                logger.WriteInfo("Add using SP -region");

                try
                {
                    CommonMethods commonMethods = new CommonMethods();
                    commonMethods.ExecuteProcedure(dbPaymentDetails, "CLEAR_DUE");
                }
                catch (Exception ex)
                {
                    logger.WriteError(ex, "Exception occured duruing UpdatePostPaymentDetails Method");
                }
            }
            #endregion

            

            #region Push Into Queue
            if (!string.IsNullOrWhiteSpace(dbSourceDetails.QueueName))
            {
                CommonMethods commonMethods = new CommonMethods();
                logger.WriteInfo("Pushing to queue details " + Newtonsoft.Json.JsonConvert.SerializeObject(queueObject, Formatting.None));

                commonMethods.PushToQueue(dbSourceDetails.QueueName, Newtonsoft.Json.JsonConvert.SerializeObject(queueObject, Formatting.None));
            }
            #endregion
        }

        #endregion

        #region Disposing Method
        public void Dispose() { }
        #endregion
    }
}
