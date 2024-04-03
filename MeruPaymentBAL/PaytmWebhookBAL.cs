using MeruPaymentBO;
using MeruPaymentBO.Paytm;
using MeruPaymentCore;
using MeruPaymentDAL;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using MeruPaymentBO.Razoypay;
using Newtonsoft.Json.Linq;
using MeruCommonLibrary;
using System.Text.RegularExpressions;


namespace MeruPaymentBAL
{
    public class PaytmWebhookBAL : IDisposable
    {
        #region Private Fields and Properties
        private string[] resourcesToProcess;
        private LogHelper logger;
        #endregion

        #region Constructors
        public PaytmWebhookBAL()
        {
            resourcesToProcess = ConfigurationManager.AppSettings["ResourcesToProcess"].ToString().Split(',');
            logger = new LogHelper("PaytmWebhookBAL");
        }
        #endregion

        #region Public Properties and Methods
        public void UpdatePaytmPaymentStatus(PaymentSuccess paymentSuccess)
        {
            logger.MethodName = "UpdatePaytmPaymentStatus(PaymentSuccess paymentSuccess)";
            PaymentDAL objPatjymentDAL = new PaymentDAL();
            //PaymentBO dbPaymentDetails = paymentDataManager.GetMeruPaymentDetail(paymentSuccess.OrderId);

            PaymentBO dbPaymentDetails = null;
            Regex regex = new Regex("^[0-9]+$");
            if (regex.IsMatch(paymentSuccess.OrderId))
            {
                dbPaymentDetails = objPatjymentDAL.GetMeruPaymentDetailOld(Convert.ToInt32(paymentSuccess.OrderId));
            }
            else
            {
                dbPaymentDetails = objPatjymentDAL.GetMeruPaymentDetail(paymentSuccess.OrderId);
            }

            if (Array.IndexOf(resourcesToProcess, dbPaymentDetails.RequestSource) < 0)
            {
                return;
            }

            if (dbPaymentDetails.PaymentStatus == PaymentStatus.PaymentSuccess)
            {
                logger.WriteInfo("Payment Status is already " + dbPaymentDetails.PaymentStatus.ToString() + " for " + dbPaymentDetails.PaymentTransactionId);
                return;
            }

            //string dbValue = string.Empty;
            //if (dbPaymentDetails.RequestReferenceVal.Length > 0)
            //{
            //    Dictionary<string, string> dbObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbPaymentDetails.RequestReferenceVal);
            //    foreach (KeyValuePair<string, string> item in dbObj.ToList<KeyValuePair<string, string>>())
            //    {
            //        dbValue += item.Value + "_";
            //    }
            //    dbValue = dbValue.TrimEnd('_');
            //}

            if (dbPaymentDetails.Amount == Convert.ToDecimal(paymentSuccess.TransactionAmount) * 100
                //&& dbPaymentDetails.Mobile == paymentSuccess.CustomerId || dbValue == paymentSuccess.CustomerId)
                && dbPaymentDetails.PaymentTransactionId == paymentSuccess.OrderId)
            {
                Paytm paytmManager = new Paytm();
                PayTMTransactionBO response = paytmManager.TransactionStatusRequest(dbPaymentDetails.PaymentTransactionId);
                if (response.Status == "TXN_SUCCESS" && response.ResponseCode == "01")
                {
                    using (UpdatePaymentDetailsWebhookDAL updater = new UpdatePaymentDetailsWebhookDAL())
                    {
                        if (regex.IsMatch(paymentSuccess.OrderId))
                        {
                            JObject objResponse = new JObject(
                                new JProperty("RESPCODE", paymentSuccess.ResponseCode),
                                new JProperty("RESPMSG", paymentSuccess.ResponseMessage),
                                new JProperty("Issuer", "PayTM")
                            );

                            RazorpayPaymentBO razorpayPaymentBO = new RazorpayPaymentBO
                            {
                                PaymentId = paymentSuccess.TransactionId,
                                MeruPaymentId = paymentSuccess.OrderId,
                                PaymentMethodDetail = JsonConvert.SerializeObject(objResponse, Formatting.None)
                            };

                            updater.UpdatePaymentDetailsOld(razorpayPaymentBO);
                        }
                        else
                        {
                            updater.UpdatePaymentDetails(response, "Webhook");
                        }

                        dbPaymentDetails.PaymentMethod = PaymentMethod.wallet;
                        dbPaymentDetails.PaymentSource = PaymentGatway.PayTM;
                        dbPaymentDetails.PaymentReferenceData2 = paymentSuccess.TransactionId;

                        PushToQueueAndExecuteProcedure(dbPaymentDetails.RequestSource, dbPaymentDetails);
                    }
                }
            }

        }
        #endregion

        #region Private Methods
        private void PushToQueueAndExecuteProcedure(string requestResouce, PaymentBO dbPaymentDetails)
        {
            logger.MethodName = "PushToQueueAndExecuteProcedure(string requestResouce, PaymentBO dbPaymentDetails)";
            PaymentRequestSystemMasterBO dbSourceDetails = new PaymentRequestSystemMasterBO();
            pragatiQueue queueObject = new pragatiQueue();
            try
            {
                RazorCheckoutResponseBAL objRazorCheckoutResponseBAL = new RazorCheckoutResponseBAL();
                dbSourceDetails = objRazorCheckoutResponseBAL.GetSourceDetail(requestResouce);
                dynamic RequestReferenceVal = JObject.Parse(dbPaymentDetails.RequestReferenceVal);
                queueObject.MeruPaymentId = dbPaymentDetails.PaymentTransactionId;
                queueObject.CarNo = RequestReferenceVal.CarNo;
                queueObject.SPId = RequestReferenceVal.SPId;
                queueObject.Amount = dbPaymentDetails.Amount;
                queueObject.PaymentMethod = dbPaymentDetails.PaymentMethod.ToString();
                queueObject.PaymentSource = dbPaymentDetails.PaymentSource.ToString();
                queueObject.PaymentId = dbPaymentDetails.PaymentReferenceData2;
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, "Exception Occured While Binding Data to PragatiQueue Model.s");
            }

            #region Add Using SP
            if (!string.IsNullOrWhiteSpace(dbSourceDetails.SPName))
            {
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
                try
                {
                    CommonMethods commonMethods = new CommonMethods();
                    commonMethods.PushToQueue(dbSourceDetails.QueueName, Newtonsoft.Json.JsonConvert.SerializeObject(queueObject,Formatting.None));
                }
                catch (Exception ex)
                {
                    logger.WriteError(ex, "Exception Occured during Send data to Queue");
                }
            }
            #endregion
        }
        #endregion

        #region Disposing Method
        public void Dispose() { }
        #endregion

    }
}
