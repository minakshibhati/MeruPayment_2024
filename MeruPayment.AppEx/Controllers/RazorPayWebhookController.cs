using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using MeruCommonLibrary;
using Newtonsoft.Json;
using System.IO;
using MeruPaymentBO.Razoypay;
using MeruPaymentBAL;

namespace MeruPayment.AppEx.Controllers
{
    public class RazorPayWebhookController : ApiController
    {
        #region Provate Fields and Properties
        private RazorpayWebhookBAL razorManager;
        private StringBuilder logData;
        private string headerName;
        private LogHelper logger;
        #endregion

        #region Constructors
        public RazorPayWebhookController()
        {
            headerName = "X-Razorpay-Signature";//ConfigurationManager.AppSettings["WebhookHeaderName"].ToString(); // Eg : X-Razorpay-Signature
            logData = new StringBuilder();
            logger = new LogHelper("RazorPayWebhookController");
        }
        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// This is Razorpay S2S API
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        [Route("V1/RazorPayment/Webhook")]
        public void Post([FromBody]JObject value)
        {
            try
            {
                //HttpRequestHeaders requestHeaders = Request.Headers;
                //if (!requestHeaders.Contains(headerName))
                //{
                //    logger.WriteWarn("Header key " + headerName + " is missing");
                //    return;
                //}
                //var requestSignature = requestHeaders.GetValues(headerName).FirstOrDefault();
                //logData.Append("Received Razorpay signature from webhook. Signature: " + requestSignature);
                ////logger.WriteInfo("Razorpay Webhook request " + value.ToString(Formatting.None) + " " + logData.ToString());

                //string documentData = string.Empty;
                //using (Stream recievedStream = HttpContext.Current.Request.InputStream)
                //{
                //    using (StreamReader streamReader = new StreamReader(recievedStream, Encoding.UTF8))
                //    {
                //        documentData = streamReader.ReadToEnd();
                //    }
                //}
                //logger.WriteInfo("Razorpay Webhook Request Input Stream : " + documentData + " " + logData.ToString());
                //string secretKey = ConfigurationManager.AppSettings["Razor_Webhook_Secret"].ToString();
                //CommonMethods commonMethods = new CommonMethods();

                ////if (!ValidateSignature(requestSignature, documentData))
                //if (!commonMethods.ValidateData_HMACSHAH256(requestSignature, documentData, secretKey))
                //{
                //    return;
                //}

                //logger.WriteInfo("Value of property event from the request is : " + value["event"].ToString());

                string payloadEvent = Convert.ToString(value["event"]);

                switch (payloadEvent)
                {
                    case "payment.authorized":
                        PaymentAuthorized paymentAuthorized = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentAuthorized>(value.ToString());
                        if (paymentAuthorized.PayLoad.Payment.Entity == null)
                        {
                            logger.WriteInfo("Payload not found for payment.authorized " + logData.ToString());
                        }
                        using (razorManager = new RazorpayWebhookBAL())
                        {
                            razorManager.ProcessPaymentAuthorized(paymentAuthorized);
                        }
                        break;
                    case "payment.captured":
                        PaymentCaptured paymentCaptured = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentCaptured>(value.ToString());
                        break;
                    case "payment.failed":
                        PaymentFailed paymentFailed = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentFailed>(value.ToString());
                        break;
                    case "order.paid":
                        OrderPaid orderPaid = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderPaid>(value.ToString());
                        break;
                    case "invoice.paid":
                        InvoicePaid invoicePaid = Newtonsoft.Json.JsonConvert.DeserializeObject<InvoicePaid>(value.ToString());
                        if (invoicePaid.PayLoad.Invoice.Entity == null)
                        {
                            logger.WriteWarn("Payload not found for invoice.paid in the request");
                            return;
                        }
                        using (razorManager = new RazorpayWebhookBAL())
                        {
                            razorManager.ProcessInvoicePaid(invoicePaid);
                        }
                        break;
                    case "invoice.expired":
                        InvoiceExpired invoiceExpired = Newtonsoft.Json.JsonConvert.DeserializeObject<InvoiceExpired>(value.ToString());
                        break;
                    case "subscription.charged":
                        SucbscriptionCharged sucbscriptionCharged = Newtonsoft.Json.JsonConvert.DeserializeObject<SucbscriptionCharged>(value.ToString());
                        break;
                    case "payment.dispute.created":
                        PaymentDisputeCreated paymentDisputeCreated = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentDisputeCreated>(value.ToString());
                        break;
                    default:
                        logger.WriteInfo("No such event is found from the object Data : " + value.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.WriteError(ex, "Error in Razorpay webhook");
            }
        }
        #endregion

        #region Private Methods
        //private bool ValidateSignature(string signature, string data)
        //{
        //    bool isValidated = false;
        //    try
        //    {
        //        string secretKey = ConfigurationManager.AppSettings["Razor_Webhook_Secret"].ToString();
        //        using (HMACSHA256Hash hmac = new HMACSHA256Hash(secretKey))
        //        {
        //            byte[] encryptedbytes = hmac.Encrypt(data);

        //            StringBuilder encryptedString = new StringBuilder();
        //            foreach (byte _byte in encryptedbytes)
        //                encryptedString.AppendFormat("{0:x2}", _byte);

        //            if (string.Equals(signature, encryptedString.ToString()))
        //                isValidated = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.WriteError(ex, "Error in ValidateResponseSignature in razorpay webhook. " + logData.ToString());
        //    }
        //    return isValidated;
        //}

        //private void UpdateAuthorizedPayment(PaymentAuthorized paymentAuthorized)
        //{
        //    using (razorManager = new RazorpayWebhookBAL())
        //    {
        //        razorManager.UpdatePaymentAuthorized(paymentAuthorized);
        //    }
        //}
        #endregion

    }
}
