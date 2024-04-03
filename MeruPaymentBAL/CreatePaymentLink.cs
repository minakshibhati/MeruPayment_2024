using MeruPaymentCore;
using MeruCommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentDAL.DAL;
using MeruPaymentBO;

namespace MeruPaymentBAL
{
    public class CreatePaymentLink
    {
        private LogHelper _logHelper;
        private Razorpay _razorpay = null;
        private PaymentLinkDAL _paymentLinkDAL = null;

        public CreatePaymentLink()
        {
            _logHelper = new LogHelper("CreatePaymentLink()");
            _paymentLinkDAL = new PaymentLinkDAL();
            _razorpay = new Razorpay();
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(PaymentLinkBO paymentLinkBO)
        {
            _logHelper.MethodName = "ProcessRequest(PaymentLinkBO paymentLinkBO)";
            try
            {
                if (paymentLinkBO.Payment_Amount_Paise < 100)
                {
                    _logHelper.WriteInfo(string.Format("Payment link amount cannot be less than 1 rupee. Payment Id {0}", paymentLinkBO.Payment_Transaction_ID));
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    string.Format("Payment link amount cannot be less than 1 rupee."),
                    null);
                }

                // GET Failure transactions
                // Create payment link via razorpay
                // Insert Payment link

                //returnValue = _paymentLinkDAL.CreatePaymentLink(orderBO);

                return new Tuple<string, string, Dictionary<string, string>>(
                   "200",
                   "Success",
                   null);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing payment link.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
