using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentBO;
using System.Net;
using System.IO;
using NLog;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace MeruPaymentCore
{
    public class Paytm
    {
        private string MerchantId = "";
        private string MerchantKey = "";
        private string Industry = "";
        private string Host = "";
        private string ReturnURL = "";
        private string CallbackURL = "";
        private string TransURL = "";
        private string TransStatusURL = "";
        private static Logger objLogger;
        public Paytm()
        {
            MerchantId = ConfigurationManager.AppSettings["PayTM_MechantId"];
            MerchantKey = ConfigurationManager.AppSettings["PayTM_MerchantKey"];
            Industry = ConfigurationManager.AppSettings["Meru_Industry"];

            Host = ConfigurationManager.AppSettings["MeruPayment_Host"];
            ReturnURL = ConfigurationManager.AppSettings["PayTM_CallBackURL"];
            CallbackURL = Host + ReturnURL;

            TransURL = ConfigurationManager.AppSettings["PayTM_TransactionURL"];
            TransStatusURL = ConfigurationManager.AppSettings["PayTM_TransactionStatusURL"];

            objLogger = LogManager.GetCurrentClassLogger();
        }

        private string GenerateChecksumForTransactionStatus(string OrderId)
        {
            string checksum = "";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("MID", MerchantId);
            parameters.Add("ORDERID", OrderId);
            checksum = GenerateChecksum(parameters); //paytm.CheckSum.generateCheckSum(MerchantKey, parameters);
            return checksum;
        }

        public PayTMTransactionBO TransactionStatusRequest(string OrderId)
        {
            PayTMTransactionBO objPayTMTransactionBO = null;
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("MID", MerchantId);
            parameters.Add("ORDERID", OrderId);
            try
            {
                parameters.Add("CHECKSUMHASH", GenerateChecksumForTransactionStatus(OrderId));
                String postData = "JsonData=" + new JavaScriptSerializer().Serialize(parameters);

                HttpWebRequest connection = (HttpWebRequest)WebRequest.Create(TransStatusURL);
                connection.Headers.Add("ContentType", "application/json");
                connection.Method = "POST";
                using (StreamWriter requestWriter = new StreamWriter(connection.GetRequestStream()))
                {
                    requestWriter.Write(postData);
                }
                string responseData = string.Empty;
                using (StreamReader responseReader = new StreamReader(connection.GetResponse().GetResponseStream()))
                {
                    responseData = responseReader.ReadToEnd();
                        objLogger.Info("Response received from paytm transaction status api. Response: " + responseData);
                    dynamic objTranStatusResponse = JsonConvert.DeserializeObject(responseData);
                    if (objTranStatusResponse != null)
                    {
                        objPayTMTransactionBO = new PayTMTransactionBO
                        {
                            MerchantId = objTranStatusResponse.MID,
                            TransactionId = objTranStatusResponse.TXNID,
                            OrderId = objTranStatusResponse.ORDERID,
                            BankTransactionId = objTranStatusResponse.BANKTXNID,
                            TransactionAmount = objTranStatusResponse.TXNAMOUNT,
                            Status = objTranStatusResponse.STATUS,
                            ResponseCode = objTranStatusResponse.RESPCODE,
                            ResponseMessage = objTranStatusResponse.RESPMSG,
                            TransactionDate = objTranStatusResponse.TXNDATE,
                            GatewayName = objTranStatusResponse.GATEWAYNAME,
                            BankName = objTranStatusResponse.BANKNAME,
                            PaymentMode = objTranStatusResponse.PAYMENTMODE,
                            TransactionType = objTranStatusResponse.TXNTYPE,
                            RefundAmount = objTranStatusResponse.REFUNDAMT,
                            CustomerId = objTranStatusResponse.CUST_ID
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPayTMTransactionBO;
        }

        public string GenerateChecksumForPayment(PayTMPaymentRequestBO objPayTMPaymentBO)
        {
            string checksum = "";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("MID", MerchantId);//
            parameters.Add("CHANNEL_ID", objPayTMPaymentBO.ChannelId);//
            parameters.Add("INDUSTRY_TYPE_ID", Industry);//
            parameters.Add("WEBSITE", objPayTMPaymentBO.Website);//
            parameters.Add("EMAIL", objPayTMPaymentBO.EmailId);//
            parameters.Add("MOBILE_NO", objPayTMPaymentBO.MobileNo);//
            parameters.Add("CUST_ID", objPayTMPaymentBO.CustomerId);//
            parameters.Add("MERC_UNQ_REF", objPayTMPaymentBO.CustomerId);//objPayTMPaymentBO.CustomerId + "_" + objPayTMPaymentBO.MobileNo);
            parameters.Add("ORDER_ID", objPayTMPaymentBO.OrderId);//
            parameters.Add("TXN_AMOUNT", objPayTMPaymentBO.TransAmount);//
            parameters.Add("CALLBACK_URL", CallbackURL);// //This parameter is not mandatory. Use this to pass the callback url dynamically.
            parameters.Add("PAYMENT_MODE_ONLY", "YES");
            parameters.Add("AUTH_MODE", "USRPWD"); //For Credit/Debit card - 3D and For Wallet, Net Banking – USRPWD
            parameters.Add("PAYMENT_TYPE_ID", "PPI"); //CC payment mode – CC | DC payment mode - DC | NB payment mode - NB | Paytm wallet – PPI | EMI - EMI | UPI - UPI

            checksum = GenerateChecksum(parameters);

            //checksum = paytm.CheckSum.generateCheckSum(MerchantKey, parameters);
            return checksum;
        }

        public string GenerateChecksum(Dictionary<string, string> parameters)
        {
            string checksum = "";
            try
            {
                if (parameters != null && parameters.Count > 0)
                {
                    checksum = paytm.CheckSum.generateCheckSum(MerchantKey, parameters);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Error occured while generating checksum for merchantkey:" + MerchantKey + " parameter: " + JsonConvert.SerializeObject(parameters, Formatting.None));
            }
            return checksum;
        }

        public bool VerifyChecksum(string Checksum, Dictionary<string, string> Parameter)
        {
            bool isValidChecksum = false;
            isValidChecksum = paytm.CheckSum.verifyCheckSum(MerchantKey, Parameter, Checksum);
            return isValidChecksum;
        }

        public PayTMRefundBO RefundTransaction(string MeruPaymentId, string PayTMTransId, long RefundAmount)
        {
            PayTMRefundBO objPayTMRefundBO = null;
            String transactionURL = ConfigurationManager.AppSettings["PayTM_RefundURL"];
            String transactionType = "REFUND";
            String refundAmount = (RefundAmount / 100).ToString();
            String refId = Guid.NewGuid().ToString();
            Dictionary<String, String> paytmParams = new Dictionary<String, String>();
            paytmParams.Add("MID", MerchantId);
            paytmParams.Add("ORDERID", MeruPaymentId);
            paytmParams.Add("TXNTYPE", transactionType);
            paytmParams.Add("REFUNDAMOUNT", refundAmount);
            paytmParams.Add("TXNID", PayTMTransId);
            paytmParams.Add("REFID", refId);
            try
            {
                string paytmChecksum = GenerateChecksum(paytmParams); //paytm.CheckSum.generateCheckSumForRefund(MerchantKey, paytmParams);
                paytmParams.Add("CHECKSUM", paytmChecksum);
                String postData = "JsonData=" + new JavaScriptSerializer().Serialize(paytmParams);
                HttpWebRequest connection = (HttpWebRequest)WebRequest.Create(transactionURL);
                connection.Headers.Add("ContentType", "application/json");
                connection.Method = "POST";
                using (StreamWriter requestWriter = new StreamWriter(connection.GetRequestStream()))
                {
                    requestWriter.Write(postData);
                }
                string responseData = string.Empty;
                using (StreamReader responseReader = new StreamReader(connection.GetResponse().GetResponseStream()))
                {
                    responseData = responseReader.ReadToEnd();
                    dynamic objRefundResponse = JsonConvert.DeserializeObject(responseData);
                    if (objRefundResponse != null)
                    {
                        objPayTMRefundBO = new PayTMRefundBO
                        {
                            BankTransactionId = objRefundResponse["BANKTXNID"],
                            CardIssuer = objRefundResponse["CARD_ISSUER"],
                            MerchandId = objRefundResponse["MID"],
                            OrderId = objRefundResponse["ORDERID"],
                            PaymentMode = objRefundResponse["PAYMENTMODE"],
                            RefundAmount = objRefundResponse["REFUNDAMOUNT"],
                            RefundDate = objRefundResponse["REFUNDDATE"],
                            RefundMeruRefId = refId,
                            RefundPaytmRefId = objRefundResponse["REFUNDID"],
                            RefundType = objRefundResponse["REFUNDTYPE"],
                            ResponseCode = objRefundResponse["RESPCODE"],
                            ResponseMessage = objRefundResponse["RESPMSG"],
                            Status = objRefundResponse["STATUS"],
                            TotalRefundAmount = objRefundResponse["TOTALREFUNDAMT"],
                            TransactionAmount = objRefundResponse["TXNAMOUNT"],
                            TransactionDate = objRefundResponse["TXNDATE"],
                            TransactionId = objRefundResponse["TXNID"]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPayTMRefundBO;
        }

    }
}
