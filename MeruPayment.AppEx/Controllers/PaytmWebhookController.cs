using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO.Paytm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using MeruPaymentBAL;

namespace MeruPayment.AppEx.Controllers
{
    public class PaytmWebhookController : ApiController
    {

        #region Private Fields and Properties
        private PaytmWebhookBAL paytmWebhookBAL;
        private LogHelper logger;
        private StringBuilder logData = new StringBuilder();
        private string documentContents;
        #endregion

        #region Constructors
        public PaytmWebhookController()
        {
            logger = new LogHelper("PaytmWebhookController");
        }
        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// This is Paytm S2S API
        /// </summary>
        [HttpPost]
        [Route("V1/Paytm/Webhook")]
        public void Post()
        {
            using (paytmWebhookBAL = new PaytmWebhookBAL())
            {
                try
                {
                    using (Stream receiveStream = HttpContext.Current.Request.InputStream)
                    {
                        using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            documentContents = readStream.ReadToEnd();
                            documentContents = HttpUtility.UrlDecode(documentContents);
                        }
                    }
                    logger.WriteInfo("Recieved Stream Parameters : " + documentContents);
                }
                catch (Exception ex)
                {
                    logger.WriteError(ex, "Error Occured while fetching input stream");
                }

                PaymentSuccess paymentSuccess = new PaymentSuccess();
                try
                {
                    paymentSuccess.Currency = HttpContext.Current.Request.Params["CURRENCY"];
                    paymentSuccess.GatewayName = HttpContext.Current.Request.Params["GATEWAYNAME"];
                    paymentSuccess.ResponseMessage = HttpContext.Current.Request.Params["RESPMSG"];
                    paymentSuccess.BankName = HttpContext.Current.Request.Params["BANKNAME"];
                    paymentSuccess.PaymentMode = HttpContext.Current.Request.Params["PAYMENTMODE"];
                    paymentSuccess.CustomerId = HttpContext.Current.Request.Params["CUSTID"];
                    paymentSuccess.MerchantId = HttpContext.Current.Request.Params["MID"];
                    paymentSuccess.MerchantUniqueValue = HttpContext.Current.Request.Params["MERC_UNQ_REF"];
                    paymentSuccess.ResponseCode = HttpContext.Current.Request.Params["RESPCODE"];
                    paymentSuccess.TransactionId = HttpContext.Current.Request.Params["TXNID"];
                    paymentSuccess.TransactionAmount = HttpContext.Current.Request.Params["TXNAMOUNT"];
                    paymentSuccess.OrderId = HttpContext.Current.Request.Params["ORDERID"];
                    paymentSuccess.Status = HttpContext.Current.Request.Params["STATUS"];
                    paymentSuccess.BankTransactionId = HttpContext.Current.Request.Params["BANKTXNID"];
                    paymentSuccess.TransactionDateTime = HttpContext.Current.Request.Params["TXNDATETIME"];
                    paymentSuccess.TransactionDate = HttpContext.Current.Request.Params["TXNDATE"];
                    //logger.WriteInfo("paytm Webhook Request Data : " + Newtonsoft.Json.JsonConvert.SerializeObject(paymentSuccess));
                }
                catch (Exception ex)
                {
                    logger.WriteError(ex, "Error Occured while binding form data to  model");
                }
                paytmWebhookBAL.UpdatePaytmPaymentStatus(paymentSuccess);
            }
        }
        #endregion

    }
}
