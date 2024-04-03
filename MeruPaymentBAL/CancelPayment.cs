using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class CancelPayment
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private PaymentDAL paymentDAL = null;
        private PaymentHistoryDAL paymentHistoryDAL = null;

        public CancelPayment()
        {
            _logHelper = new LogHelper("CancelPayment()");
            paymentDAL = new PaymentDAL();
            paymentHistoryDAL = new PaymentHistoryDAL(); 
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(string paymentId)
        {
            _logHelper.MethodName = "ProcessRequest(string paymentId)";
            try
            {
                returnValue = paymentDAL.CancelPayment(paymentId);
                
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }

                paymentHistoryDAL.AddStatusChange(paymentId, PaymentStatus.PaymentCancelled, returnValue.Item3["PaymentType"]);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    null);
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
