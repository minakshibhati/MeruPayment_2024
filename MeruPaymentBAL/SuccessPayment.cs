using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class SuccessPayment
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private PaymentDAL paymentDAL = null;
        private PaymentHistoryDAL paymentHistoryDAL = null;

        public SuccessPayment()
        {
            _logHelper = new LogHelper("CreateOrder()");
            paymentDAL = new PaymentDAL();
            paymentHistoryDAL = new PaymentHistoryDAL();
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(string paymentId, string PGPaymentId, PaymentGatway paymentGatway)
        {
            _logHelper.MethodName = "ProcessRequest(string paymentId)";
            try
            {
                returnValue = paymentDAL.SuccessPayment(paymentId, PGPaymentId, paymentGatway);
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }
                if (paymentGatway == PaymentGatway.Razorpay)
                {
                    CommonMethods commonMethods = new CommonMethods();
                    if (!commonMethods.UpdateRazorPostPaymentDetail(paymentId, PGPaymentId))
                    {
                        return new Tuple<string, string, Dictionary<string, string>>("500", "Error while updating additional payment details", returnValue.Item3);
                    }
                }

                paymentHistoryDAL.AddStatusChange(paymentId, PaymentStatus.PaymentSuccess, returnValue.Item3["PaymentType"]);
                

                return returnValue;
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while cancellaing payment.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
