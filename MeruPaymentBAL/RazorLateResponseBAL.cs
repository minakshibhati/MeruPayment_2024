using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using MeruPaymentDAL.DAL;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeruPaymentBAL
{
    public class RazorLateResponseBAL
    {
        private static Logger objLogger = null;
        PaymentDAL objPaymentDAL = null;
        PaymentBO objPaymentBO = null;
        PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        PaymentHistoryDAL objPaymentHistoryDAL = null;

        public RazorLateResponseBAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            objPaymentDAL = new PaymentDAL();
            objPaymentBO = new PaymentBO();
            objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
            objPaymentHistoryDAL = new PaymentHistoryDAL();
        }

        public bool ValidateResponseSignature(string PaymentSignature, string PaymentData, string Secret)
        {
            CommonMethods objCommonMethods = new CommonMethods();
            return objCommonMethods.ValidateData_HMACSHAH256(PaymentSignature, PaymentData, Secret);
        }

        public bool ProcessPaymentAuthorized(RazorpayPaymentBO objRazorpayPaymentBO, ref bool IsCaptureProcessed)
        {
            bool returnValue = false;
            try
            {
                objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(objRazorpayPaymentBO.MeruPaymentId);
                if (objPaymentBO == null)
                {
                    objLogger.Info(string.Format("No record found for meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                    return returnValue;
                }

                if (objPaymentBO.Amount.ToString() != objRazorpayPaymentBO.Amount)
                {
                    objLogger.Info(string.Format("Amount mismatch for meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                    return returnValue;
                }

                if (objPaymentBO.PaymentReferenceData1 != objRazorpayPaymentBO.OrderId)
                {
                    objLogger.Info(string.Format("Order Id mismatch for meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                    return returnValue;
                }
                returnValue = objPaymentBO.PaymentStatus == PaymentStatus.PaymentSuccess;
                if (!returnValue)
                {
                    Razorpay objRazorpay = new Razorpay();
                    IsCaptureProcessed = objRazorpay.CapturePayment(objPaymentBO.PaymentReferenceData2, objPaymentBO.Amount.ToString());
                    if (!IsCaptureProcessed)
                    {
                        objLogger.Info(string.Format("Unable to capture payment for Meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                        return returnValue;
                    }
                    JObject objPaymentRefDetail = new JObject
                    {
                        new JProperty("PGOrderId",objRazorpayPaymentBO.OrderId),
                        new JProperty("PGPaymentId",objRazorpayPaymentBO.PaymentId)
                    };

                    if (!objPaymentDAL.TransactionSuccess(objRazorpayPaymentBO.MeruPaymentId, objRazorpayPaymentBO.OrderId, objRazorpayPaymentBO.PaymentId, objPaymentRefDetail.ToString(Formatting.None), PaymentGatway.Razorpay))
                    {
                        objLogger.Info(string.Format("Unable to update webhook response to db for Meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                        return returnValue;
                    }
                    objPaymentHistoryDAL.AddStatusChange(objRazorpayPaymentBO.MeruPaymentId, PaymentStatus.PaymentSuccess, "Webhook");

                    if (!UpdatePostPaymentDetail(objRazorpayPaymentBO.MeruPaymentId, objRazorpayPaymentBO.PaymentId))
                    {
                        objLogger.Info(string.Format("Unable to update post payment detail for Meru payment id {0}", objRazorpayPaymentBO.MeruPaymentId));
                        return returnValue;
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
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

            //        objDetail.Add(new JProperty("Name", objRazorpayCardBO.FullName));
            //        objDetail.Add(new JProperty("Last4", objRazorpayCardBO.Last4));
            //        objDetail.Add(new JProperty("Issuer", objRazorpayCardBO.Issuer));
            //        objDetail.Add(new JProperty("International", objRazorpayCardBO.IsInternational));
            //        objDetail.Add(new JProperty("Emi", objRazorpayCardBO.IsEMI));
            //    }
            //    objDetail.Add(new JProperty("Issuer", objRazorPaymentBO.PaymentMethodDetail));

            //    returnStatus = objPaymentDAL.UpdatePostTransactionSuccess(mpid, objDetail.ToString(Formatting.None), PaymentGatway.Razorpay);
            //}
            //catch (Exception ex)
            //{
            //    objLogger.Error(ex);
            //}
            //returnStatus;
        }

        public bool PushToQueue(string MeruPaymentId)
        {
            objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);
            if (objPaymentBO == null)
            {
                objLogger.Info(string.Format("No record found for meru payment id {0}", MeruPaymentId));
                return false;
            }

            objPaymentRequestSystemMasterBO = objPaymentRequestSystemDAL.GetDetailBySystemCode(objPaymentBO.RequestSource);
            if (objPaymentRequestSystemMasterBO == null)
            {
                objLogger.Info(string.Format("Unable to get source detail for {0}", objPaymentBO.RequestSource));
                return false;
            }
            if (objPaymentRequestSystemMasterBO.QueueName != null && objPaymentRequestSystemMasterBO.QueueName.Length > 0)
            {
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

                CommonMethods objCommonMethods = new CommonMethods();
                return objCommonMethods.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, objQ.ToString(Formatting.None));
            }
            return true;
        }

        public PaymentRequestSystemMasterBO GetSourceDetailByPaymentId(string MeruPaymentId)
        {
            try
            {
                objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);
                if (objPaymentBO == null)
                {
                    objLogger.Info(string.Format("No record found for meru payment id {0}", MeruPaymentId));
                    return null;
                }
                objPaymentRequestSystemMasterBO = objPaymentRequestSystemDAL.GetDetailBySystemCode(objPaymentBO.RequestSource);
                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.Info(string.Format("Unable to get source detail for {0}", objPaymentBO.RequestSource));
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPaymentRequestSystemMasterBO;
        }
    }
}
