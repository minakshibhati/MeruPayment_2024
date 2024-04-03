using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class PaymentLinkIssue
    {

        #region Private Fields
        private int baseProviderId = 1000;
        private string AmountType = "FARE";
        private Int64 millisecondOld = 0;
        private DateTime comparer = DateTime.Now;
        private LogHelper logHelper = new LogHelper("PaymentLinkIssue");
        #endregion


        public PaymentLinkIssue()
        {
            millisecondOld = Convert.ToInt64(ConfigurationManager.AppSettings["millisecondOld"]);
            comparer = DateTime.Now.AddMilliseconds(-millisecondOld);
        }
        //public void Process()
        //{
        //    logHelper.MethodName = "Process";

        //    ProcessLink();
        //    // GET ALL CARD FAILURE PAYMENTS
        //    // FILTER 24HOURS
        //    // GENERATE PAYMENT LINK
        //    // INSERT PAYMENT LINK DETAILS 
        //}

        public void ProcessLink()
        {
            logHelper.MethodName = "ProcessLink()";
            List<PaymentBO> failedPayments = new List<PaymentBO>();
            List<PaymentLinkBO> dbPaymentLink = new List<PaymentLinkBO>();
            try
            {
                using (PaymentDAL objPaymentDAL = new PaymentDAL())
                {
                    //failedPayments = new List<PaymentBO> { new PaymentBO { JobId = 49945475, Mobile = "9448941414", Amount = Convert.ToInt64(1016.62 * 100), ProviderId = 4500, CreatedOn = Convert.ToDateTime("05-12-2019  21:19:15"), OrderId = "49945475", Email = null, PaymentType = "FARE", TripId = 49987441, Id = 13412925 } };
                    failedPayments = objPaymentDAL.GetAllFailedPayments();
                    if (failedPayments != null)
                    {
                        failedPayments = failedPayments.Where(x =>
                        x.CreatedOn <= comparer
                        && x.ProviderId > baseProviderId
                        && x.PaymentType == AmountType).ToList();
                    }
                }
                using (PaymentLinkDAL objPaymentLinkDAL = new PaymentLinkDAL())
                {
                    dbPaymentLink = objPaymentLinkDAL.GetPaymentLinkDetails();
                }
                logHelper.WriteDebug("Total no of fail payments count is : " + failedPayments.Count);
                foreach (PaymentBO failedPayment in failedPayments)
                {
                    using (PaymentDAL objPaymentDAL = new PaymentDAL())
                    {
                        var dbfailedTransactionDetails = objPaymentDAL.GetPaymentDetailsByTripIdAndPaymentType("FARE", Convert.ToString(failedPayment.TripId));
                        if (dbfailedTransactionDetails != null)
                        {
                            failedPayment.PaymentTransactionId = dbfailedTransactionDetails.PaymentTransactionId;
                        }
                    }

                    if (failedPayment.PaymentTransactionId != null && failedPayment.PaymentTransactionId != string.Empty)
                    {
                        if (dbPaymentLink
                                        .Where(x => x.Payment_Amount_Paise == failedPayment.Amount
                                            && x.Request_RefId == Convert.ToString(failedPayment.TripId)
                                            && x.Payment_Transaction_ID == failedPayment.PaymentTransactionId
                                        ).Count() == 0)
                        {
                            string providerCustomerId;
                            Dictionary<string, string> additionalParameter = new Dictionary<string, string>();
                            additionalParameter.Add("Meru_PaymentId", failedPayment.PaymentTransactionId);
                            using (CustomerDAL objCustomerDAL = new CustomerDAL())
                            {
                                providerCustomerId = objCustomerDAL.GetCustomerId(failedPayment.Mobile, failedPayment.Email);

                                if (!string.IsNullOrWhiteSpace(providerCustomerId))
                                {
                                    Razorpay objRazorpay = new Razorpay();
                                    string linkDescription = string.Empty;
                                    linkDescription = string.Format("Payment for Meru ride on {0} ", failedPayment.CreatedOn.ToString("dd MMM yyyy"));
                                    var Response = objRazorpay.CreatePaymentLink(providerCustomerId, Convert.ToString(failedPayment.TripId), Convert.ToString(failedPayment.Amount), linkDescription, null, additionalParameter);

                                    if (Response.Item2 == "Success")
                                    {
                                        PaymentLinkBO objPaymentLinkDetails = new PaymentLinkBO();
                                        objPaymentLinkDetails = Response.Item3;
                                        //objPaymentLinkDetails.Payment_Method = (int)failedPayment.PaymentMethod;
                                        objPaymentLinkDetails.Payment_Source = (int)PaymentGatway.Razorpay;
                                        objPaymentLinkDetails.Status = (int)PaymentStatus.PaymentInitiated;
                                        objPaymentLinkDetails.Contact = failedPayment.Mobile;
                                        objPaymentLinkDetails.Email = failedPayment.Email;
                                        objPaymentLinkDetails.Description = linkDescription;


                                        using (PaymentLinkDAL objPaymentLinkDAL = new PaymentLinkDAL())
                                        {
                                            objPaymentLinkDAL.SavePaymentLink(objPaymentLinkDetails);
                                        }
                                    }
                                    else
                                    {
                                        logHelper.WriteDebug("Response from Razorpay is not success response is : " + Response.Item2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            logHelper.WriteDebug("Link has already created for Payment Transaction Id : " + failedPayment.PaymentTransactionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "Exception occured while processing payment link");
            }
        }
    }
}
