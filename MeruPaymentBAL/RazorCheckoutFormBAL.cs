using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeruPaymentBAL
{
    public class RazorCheckoutFormBAL
    {
        private PaymentDAL objPaymentDAL = null;
        private PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        private static Logger objLogger;
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        private Razorpay objRazorpay;
        private PaymentHistoryDAL objPaymentHistoryDAL = null;

        public RazorCheckoutFormBAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            objPaymentDAL = new PaymentDAL();
            objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
            objRazorpay = new Razorpay();
            objPaymentHistoryDAL = new PaymentHistoryDAL();
        }

        public PaymentBO GetMeruPaymentDetail(string MeruPaymentId)
        {
            PaymentBO objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);
            return objPaymentBO;
        }

        public string InitializePaymentWithOrderId(PaymentBO objPaymentBO)
        {
            string OrderId = "";
            try
            {
                OrderId = objRazorpay.CreateOrder(objPaymentBO.PaymentTransactionId.ToString(), objPaymentBO.Amount.ToString());
                objPaymentBO.PaymentSource = PaymentGatway.Razorpay;
                objPaymentBO.PaymentReferenceValue = new JObject { new JProperty("RazorOrderId", OrderId) }.ToString(Formatting.None);
                objPaymentBO.PaymentReferenceData1 = OrderId;
                objPaymentDAL.InitializePayment(objPaymentBO);
                objPaymentHistoryDAL.AddStatusChange(objPaymentBO.PaymentTransactionId, PaymentStatus.PaymentInitiated, "TRANS");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return OrderId;
        }

        public string CreatePaymentEntry(PaymentBO objPaymentBO)
        {
            string MeruPaymentId = "";
            try
            {
                objPaymentBO.PaymentSource = PaymentGatway.Razorpay;
                MeruPaymentId = objPaymentDAL.CreatePayment(objPaymentBO);
                objPaymentHistoryDAL.AddStatusChange(MeruPaymentId.ToString(), PaymentStatus.PaymentCreated, "TRANS");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return MeruPaymentId;
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


        public PaymentMethod GetPaymentMethod(string objPaymentMethod)
        {
            CommonMethods objCommonMethods = new CommonMethods();
            return objCommonMethods.GetPaymentMethod(objPaymentMethod);
        }
    }
}
