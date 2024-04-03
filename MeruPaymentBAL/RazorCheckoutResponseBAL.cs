using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruCommonLibrary;
using NLog;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeruPaymentBAL
{
    public class RazorCheckoutResponseBAL
    {
        private static Logger objLogger;
        private PaymentDAL objPaymentDAL = null;
        private PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        private PaymentBO objPaymentBO = null;
        PaymentHistoryDAL objPaymentHistoryDAL = null;

        public RazorCheckoutResponseBAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            objPaymentDAL = new PaymentDAL();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
            objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
            objPaymentHistoryDAL = new PaymentHistoryDAL();
        }

        public PaymentRequestSystemMasterBO GetSourceDetail(string SourceSystemCode)
        {
            try
            {
                if (SourceSystemCode == null || SourceSystemCode.Length == 0)
                {
                    objLogger.Warn("Request data for rayzorpay checkout is null");
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

        public PaymentBO GetMeruPaymentDetail(string MeruPaymentId)
        {
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

        public bool ValidateResponseSignature(string PaymentSignature, string PaymentData, string Secret)
        {
            CommonMethods objCommonMethods = new CommonMethods();
            return objCommonMethods.ValidateData_HMACSHAH256(PaymentSignature, PaymentData, Secret);
        }

        public void UpdatePaymentCancelledStatus(string mpid)
        {
            objPaymentDAL.TransactionCancelled(mpid, PaymentGatway.Razorpay);
            objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentCancelled, "TRANS");
        }

        public void UpdatePaymentFailedStatus(string mpid, string ErrorCode, string ErrorDescription)
        {
            JObject objOthers = new JObject(
                        new JProperty("Error Code", ErrorCode),
                        new JProperty("Error Description", ErrorDescription)
                        );

            objPaymentDAL.TransactionFailed(mpid, objOthers.ToString(Formatting.None), PaymentGatway.Razorpay);
            objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentFailed, "TRANS");
        }

        public bool UpdatePaymentSuccessStatus(string mpid, string OrderId, string PaymentId)
        {
            bool returnValue = false;
            try
            {
                JObject objRef = new JObject { 
                                new JProperty("PGOrderId",OrderId),
                                new JProperty("PGPaymentId",PaymentId)
                            };
                returnValue = objPaymentDAL.TransactionSuccess(mpid, OrderId, PaymentId, objRef.ToString(Formatting.None), PaymentGatway.Razorpay);
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
            objPaymentDAL.TransactionPending(mpid, PaymentId, PaymentGatway.Razorpay);
            objPaymentHistoryDAL.AddStatusChange(mpid, PaymentStatus.PaymentPending, "TRANS");
        }

        public bool PushToQueue(string QueueName, PaymentBO objPaymentBO)
        {
            CommonMethods objCommonMethods = new CommonMethods();

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

        public bool UpdatePostPaymentDetail(string mpid, string RazorPaymentId)
        {
            CommonMethods objCommonMethods = new CommonMethods();
            return objCommonMethods.UpdateRazorPostPaymentDetail(mpid, RazorPaymentId);
            //bool returnStatus = false;
            //try
            //{
            //    Razorpay objRazorpay = new Razorpay();
            //    RazorpayPaymentBO objRazorPaymentBO = objRazorpay.GetPaymentDetail(RazorPaymentId);
            //    JObject objDetail = new JObject();
            //    if (objRazorPaymentBO.PaymentMethod == PaymentMethod.card)
            //    {
            //        RazorpayCardBO objRazorpayCardBO = objRazorpay.GetCardDetail(objRazorPaymentBO.PaymentMethodDetail);
                   
            //        objDetail.Add(new JProperty("Name",objRazorpayCardBO.FullName));
            //        objDetail.Add(new JProperty("Last4",objRazorpayCardBO.Last4));
            //        objDetail.Add(new JProperty("Issuer",objRazorpayCardBO.Issuer));
            //        objDetail.Add(new JProperty("International",objRazorpayCardBO.IsInternational));
            //        objDetail.Add(new JProperty("Emi",objRazorpayCardBO.IsEMI));
            //    }
            //    objDetail.Add(new JProperty("Issuer", objRazorPaymentBO.PaymentMethodDetail));
             
            //    returnStatus = objPaymentDAL.UpdatePostTransactionSuccess(mpid, objDetail.ToString(Formatting.None), PaymentGatway.Razorpay);
            //}
            //catch (Exception ex)
            //{
            //    objLogger.Error(ex);
            //}
            //return returnStatus;
        }

        public bool ValidateTransactionResponse(string RazorPaymentId, string RazorOrderId, long Amount)
        {
            bool returnValue = false;
            try
            {
                Razorpay objRazorpay = new Razorpay();
                RazorpayPaymentBO objRazorPaymentBO = objRazorpay.GetPaymentDetail(RazorPaymentId);
                if (objRazorPaymentBO.Amount != Amount.ToString() || objRazorPaymentBO.OrderId != RazorOrderId)
                {
                    return false;
                }

                if (objRazorPaymentBO.PaymentStatus == PaymentStatus.PaymentInitiated)
                {
                    return objRazorpay.CapturePayment(RazorPaymentId, Amount.ToString());
                }

                returnValue = true;
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
        }


        public Tuple<string, string, Boolean> UpdatePaymentResponseDetail_DA(SuccessCallBackResponse resp)
        //public Tuple<string, string, Boolean> UpdatePaymentResponseDetail_DA(string razorpay_signature, string Order_Id, string razorpay_payment_id, string razorpay_order_id, string status_code,dynamic resp)
        {
            bool returnValue = false;
            string txnamount  = "", status = "", resCode = "", resMsg = "";
            
            try
            {

                status = resp.status_code;
                PaymentStatus paymentStatus = PaymentStatus.PaymentUnkown;

                if (status == "200" )
                {
                    paymentStatus = PaymentStatus.PaymentSuccess;
                }
                else 
                {
                    paymentStatus = PaymentStatus.PaymentFailed;
                }

                CheckFormRequestBAL objCheckFormRequestBAL = new CheckFormRequestBAL();
                PaymentMethod paymethod = objCheckFormRequestBAL.GetPaymentMethodEnum(resp.PaymentMethod);
                var txndet = objPaymentDAL.TransactionResponseAPI_DA_Update(resp.Order_Id, resp.razorpay_payment_id, resp.razorpay_order_id, resp.razorpay_signature,"", JsonConvert.SerializeObject(resp).ToString(), PaymentGatway.Razorpay, paymentStatus, paymethod,resp.SPId,resp.CarNo,resp.PurchaseDescription,resp.email);
                

                if (txndet != null)
                {
                    returnValue = true;
                    objPaymentHistoryDAL.AddStatusChange(txndet.Payment_Transaction_ID, paymentStatus, "TRANS");
                    return new Tuple<string, string, Boolean>(
                    "200",
                   (txndet.Payment_Amount_Paise * 0.01).ToString()
                    , true);



                }

                else
                {
                    return new Tuple<string, string, Boolean>(
                "400",
               null
                , false);


                }

            }
            catch (Exception ex)
            {
                objLogger.Error(ex);

                return new Tuple<string, string, Boolean>(
            "400",
           ex.Message
            , false);
            }
            
        }


    }
}
