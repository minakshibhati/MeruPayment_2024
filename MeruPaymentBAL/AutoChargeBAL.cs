using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class AutoChargeBAL
    {
        private LogHelper _logHelper;

        public AutoChargeBAL()
        {
            _logHelper = new LogHelper("AutoChargeBAL");
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(string paymentId, string appRequestId, string orderId, string pgToken, string pgCustomerId, long amount, string email, string contact, string desc)
        {
            _logHelper.MethodName = "ProcessRequest()";
            try
            {
                bool returnValue = false;
                long returnId = 0;

                PaymentDAL paymentDAL = new PaymentDAL();
                PaymentHistoryDAL paymentHistoryDAL = new PaymentHistoryDAL();

                Razorpay razorpay = new Razorpay();
                Dictionary<string, string> AdditionalParam = new Dictionary<string, string>();
                AdditionalParam.Add("Meru_PaymentId", paymentId);
                AdditionalParam.Add("AppRequestId", appRequestId);

                Tuple<string, string, RazorpayPaymentBO> returnAutoCharge = razorpay.AutoCharge(AdditionalParam, orderId, pgToken, pgCustomerId, amount, email, contact, desc);

                if (returnAutoCharge.Item1 != "200")
                {

                    JObject objAuthCard = JObject.Parse(returnAutoCharge.Item2);

                    string ErrorCode = Convert.ToString(objAuthCard["error"]["code"]);
                    string ErrorDescription = Convert.ToString(objAuthCard["error"]["description"]);

                    string failReason = ErrorCode + " - " + ErrorDescription;
                    JObject objOthers = new JObject (
                        new JProperty("Error Code", ErrorCode),
                        new JProperty("Error Description", ErrorDescription)
                        );

                    paymentDAL.TransactionFailed(paymentId, objOthers.ToString(Formatting.None), PaymentGatway.Razorpay);
                    paymentHistoryDAL.AddStatusChange(paymentId, PaymentStatus.PaymentFailed, "AUTOCHARGE");

                    return new Tuple<string, string, Dictionary<string, string>>(returnAutoCharge.Item1, failReason, null);
                }

                JObject objRef = new JObject { 
                                new JProperty("PGOrderId",returnAutoCharge.Item3.OrderId),
                                new JProperty("PGPaymentId",returnAutoCharge.Item3.PaymentId)
                            };

                returnValue = paymentDAL.TransactionSuccess(paymentId, returnAutoCharge.Item3.OrderId, returnAutoCharge.Item3.PaymentId, objRef.ToString(Formatting.None), PaymentGatway.Razorpay);

                returnId = paymentHistoryDAL.AddStatusChange(paymentId, PaymentStatus.PaymentSuccess, "AUTOCHARGE");

                CommonMethods commonMethods = new CommonMethods();
                if (!commonMethods.UpdateRazorPostPaymentDetail(paymentId, returnAutoCharge.Item3.PaymentId))
                {
                    return new Tuple<string, string, Dictionary<string, string>>("500", "Error while updating additional payment details", null);
                }

                return new Tuple<string, string, Dictionary<string, string>>("200", "Success", null);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing auto charge request.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
