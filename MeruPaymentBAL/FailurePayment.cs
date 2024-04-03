using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class FailurePayment
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private PaymentDAL paymentDAL = null;
        private PaymentHistoryDAL paymentHistoryDAL = null;

        public FailurePayment()
        {
            _logHelper = new LogHelper("FailurePayment()");
            paymentDAL = new PaymentDAL();
            paymentHistoryDAL = new PaymentHistoryDAL(); 
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(string paymentId, string errorCode, string errorDesc)
        {
            _logHelper.MethodName = "ProcessRequest(string paymentId)";
            try
            {
                JObject objOthers = new JObject(
                      new JProperty("Error Code", errorCode),
                      new JProperty("Error Description", errorDesc)
                      );

                returnValue = paymentDAL.FailurePayment(paymentId, objOthers.ToString(Formatting.None));
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }

                paymentHistoryDAL.AddStatusChange(paymentId, PaymentStatus.PaymentFailed, returnValue.Item3["PaymentType"]);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    null);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while marking failure payment status.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
