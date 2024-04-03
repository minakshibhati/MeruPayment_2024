using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json;
using MeruPaymentBO;
using System.Configuration;

namespace MeruPaymentBAL
{
    public class PayTMCheckoutResponseBAL
    {
        private static Logger objLogger;
        Paytm objPaytm;
        PaymentDAL objPaymentDAL = null;
        PaymentHistoryDAL objPaymentHistoryDAL = null;
        private PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        public PayTMCheckoutResponseBAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            objPaytm = new Paytm();
            objPaymentDAL = new PaymentDAL();
            objPaymentHistoryDAL = new PaymentHistoryDAL();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public bool VerifyCheckSum(Dictionary<string, string> parameters, string paytmChecksum)
        {
            //string checksum = "";
            //checksum = objPaytm.VerifyChecksum(paytmChecksum, parameters);


            return objPaytm.VerifyChecksum(paytmChecksum, parameters);// (checksum == paytmChecksum);
        }

        private dynamic TransStatusRequest(string MerchantId, string PayTmOrderId)
        {
            //Dictionary<String, String> parameters = new Dictionary<String, String>();
            //parameters.Add("MID", MerchantId);
            //parameters.Add("ORDERID", PayTmOrderId);
            dynamic objTranStatusResponse = null;
            //try
            //{
            //    parameters.Add("CHECKSUMHASH", objPaytm.ge paytm.CheckSum.generateCheckSum(MerchantKey, parameters));
            //    String postData = "JsonData=" + new JavaScriptSerializer().Serialize(parameters);

            //    HttpWebRequest connection = (HttpWebRequest)WebRequest.Create(TransStatusURL);
            //    connection.Headers.Add("ContentType", "application/json");
            //    connection.Method = "POST";
            //    using (StreamWriter requestWriter = new StreamWriter(connection.GetRequestStream()))
            //    {
            //        requestWriter.Write(postData);
            //    }
            //    string responseData = string.Empty;
            //    using (StreamReader responseReader = new StreamReader(connection.GetResponse().GetResponseStream()))
            //    {
            //        responseData = responseReader.ReadToEnd();
            //        objLogger.Info("Response received from paytm transaction status api. Response: " + responseData);
            //        objTranStatusResponse = JsonConvert.DeserializeObject(responseData);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    objLogger.Error(ex);
            //}
            return objTranStatusResponse;
        }

        public void UpdatePaymentFailedStatus(string mpid, string ResponseCode, string ResponseMessage)
        {
            JObject objOthers = new JObject(
                        new JProperty("Response Code", ResponseCode),
                        new JProperty("Response Message", ResponseMessage)
                        );

            objPaymentDAL.TransactionFailed(mpid, objOthers.ToString(Formatting.None), PaymentGatway.PayTM);
            objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentFailed, "TRANS");
        }

        public bool UpdatePaymentSuccessStatus(string mpid, string OrderId, string TransactionId, string BankTransactionId)
        {
            bool returnValue = false;
            try
            {
                JObject objRef = new JObject { 
                                new JProperty("RazorOrderId",OrderId),
                                new JProperty("PaytmTransactiontId",TransactionId),
                                  new JProperty("BankTransactiontId",BankTransactionId)
                            };
                returnValue = objPaymentDAL.TransactionSuccess(mpid, OrderId, TransactionId, objRef.ToString(Formatting.None), PaymentGatway.PayTM);
                objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentSuccess, "TRANS");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
        }

        public void UpdatePaymentPendingStatus(string mpid, string PaymentId)
        {
            objPaymentDAL.TransactionPending(mpid, PaymentId, PaymentGatway.PayTM);
            objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentPending, "TRANS");
        }

        public bool PushToQueue(string QueueName, PaymentBO objPaymentBO)
        {
            CommonMethods objCommonMethods = new CommonMethods();

            JObject objQ = new JObject(
                new JProperty("MeruPaymentId", objPaymentBO.MeruPaymentId),
                new JProperty("Amount", objPaymentBO.Amount),
                new JProperty("PaymentMethod", objPaymentBO.PaymentMethod.ToString()),
                new JProperty("PaymentSource", objPaymentBO.PaymentSource.ToString()),
                new JProperty("PaymentId", objPaymentBO.PaymentReferenceData2)
                );
            Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(objPaymentBO.RequestReferenceVal);
            foreach (KeyValuePair<string, string> item in obj.ToList<KeyValuePair<string, string>>())
            {
                objQ.Add(item.Key, item.Value);
            }

            return objCommonMethods.PushToQueue(QueueName, objQ.ToString(Formatting.None));
        }

        public bool PushToQueue(string QueueName, string MeruPaymentId)
        {
            CommonMethods objCommonMethods = new CommonMethods();

            PaymentBO objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);

            JObject objQ = new JObject(
                new JProperty("MeruPaymentId", objPaymentBO.PaymentTransactionId),
                new JProperty("Amount", objPaymentBO.Amount),
                new JProperty("PaymentMethod", objPaymentBO.PaymentMethod.ToString()),
                new JProperty("PaymentSource", objPaymentBO.PaymentSource.ToString()),
                new JProperty("PaymentId", objPaymentBO.PaymentReferenceData2)
                );
            Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(objPaymentBO.RequestReferenceVal);
            foreach (KeyValuePair<string, string> item in obj.ToList<KeyValuePair<string, string>>())
            {
                objQ.Add(item.Key, item.Value);
            }

            return objCommonMethods.PushToQueue(QueueName, objQ.ToString(Formatting.None));
        }

        public PaymentBO GetMeruPaymentDetail(string MeruPaymentId)
        {
            PaymentBO objPaymentBO = null;
            try
            {
                objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPaymentBO;
        }

        public PaymentRequestSystemMasterBO GetSourceDetail(string SourceSystemCode)
        {
            try
            {
                if (SourceSystemCode == null || SourceSystemCode.Length == 0)
                {
                    objLogger.Warn("Request data for paytm checkout is null");
                    return null;
                }

                objPaymentRequestSystemMasterBO = objPaymentRequestSystemDAL.GetDetailBySystemCode(SourceSystemCode);
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return objPaymentRequestSystemMasterBO;
        }

        public bool UpdatePaymentResponseDetail(Dictionary<string, string> parameters, string OrderId, string MeruPaymentId)
        {
            bool returnValue = false;
            string txnId = "", status = "", resCode = "", resMsg = "";
            try
            {
                JObject objQ = new JObject(
                                 new JProperty("RazorOrderId", OrderId)
                             );

                if (parameters.ContainsKey("TXNID"))
                {
                    txnId = parameters["TXNID"];
                    objQ.Add(new JProperty("PayTMTransId", txnId));
                }

                JObject objQ1 = new JObject();

                if (parameters.ContainsKey("MERC_UNQ_REF"))
                {
                    objQ1.Add(new JProperty("CustomerId", parameters["MERC_UNQ_REF"]));
                }
                if (parameters.ContainsKey("BANKTXNID"))
                {
                    objQ1.Add(new JProperty("BankTransId", parameters["BANKTXNID"]));
                }
                if (parameters.ContainsKey("TXNAMOUNT"))
                {
                    objQ1.Add(new JProperty("Amount", parameters["TXNAMOUNT"]));
                }
                if (parameters.ContainsKey("STATUS"))
                {
                    status = parameters["STATUS"];
                    objQ1.Add(new JProperty("STATUS", status));
                }
                if (parameters.ContainsKey("RESPCODE"))
                {
                    resCode = parameters["RESPCODE"];
                    objQ1.Add(new JProperty("RESPCODE", resCode));
                }
                if (parameters.ContainsKey("RESPMSG"))
                {
                    resMsg = parameters["RESPMSG"];
                    objQ1.Add(new JProperty("RESPMSG", resMsg));
                }
                if (parameters.ContainsKey("GATEWAYNAME"))
                {
                    objQ1.Add(new JProperty("GatewayName", parameters["GATEWAYNAME"]));
                }
                if (parameters.ContainsKey("BANKNAME"))
                {
                    objQ1.Add(new JProperty("BankName", parameters["BANKNAME"]));
                }
                if (parameters.ContainsKey("PAYMENTMODE"))
                {
                    objQ1.Add(new JProperty("PaymentMode", parameters["PAYMENTMODE"]));
                }


                //paymentObject.PaymentReferenceValue = objQ.ToString(Formatting.None);
                //paymentObject.PaymentReferenceData3 = objQ1.ToString(Formatting.None);

                JObject objOthers = new JObject(
                    new JProperty("Response Code", resCode),
                    new JProperty("Response Message", resMsg)
                );

                string payRes = objQ1.ToString(Formatting.None);

                payRes += objOthers.ToString(Formatting.None);


                PaymentStatus paymentStatus = PaymentStatus.PaymentUnkown;

                if (status == "TXN_SUCCESS" && resCode == "01")
                {
                    paymentStatus = PaymentStatus.PaymentSuccess;
                }
                else if (status == "TXN_FAILURE")
                {
                    paymentStatus = PaymentStatus.PaymentFailed;
                }
                else if (status == "PENDING")
                {
                    paymentStatus = PaymentStatus.PaymentPending;
                }

                returnValue = objPaymentDAL.TransactionResponseUpdate(MeruPaymentId, txnId, objQ.ToString(Formatting.None), payRes, PaymentGatway.PayTM, paymentStatus, PaymentMethod.wallet);
                objPaymentHistoryDAL.AddStatusChange(MeruPaymentId, paymentStatus, "TRANS");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
        }


    }
}
