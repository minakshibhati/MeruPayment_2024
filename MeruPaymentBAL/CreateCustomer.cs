using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using MeruPaymentCore;
//using Razorpay.Api;

namespace MeruPaymentBAL
{
    public class CreateCustomer
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private CustomerDAL _customerDAL;
        private Razorpay _razorpay = null;

        public CreateCustomer()
        {
            _logHelper = new LogHelper("CreateCustomer()");
            _customerDAL = new CustomerDAL();
            _razorpay = new Razorpay();
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(CustomerBO customerBO)
        {
            _logHelper.MethodName = "ProcessRequest(CustomerBO customerBO)";

            try
            {
                returnValue = _customerDAL.CreateCustomer(customerBO);

                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                };
                Dictionary<string, string> returnData = new Dictionary<string, string>();
                returnData.Add("CustomerId", returnValue.Item3["CustomerId"]);

                if (returnValue.Item3["PGCustomerId"] == null || returnValue.Item3["PGCustomerId"].Length == 0)
                {
                    returnValue = _razorpay.CreateCustomer(customerBO.FullName, customerBO.Email, customerBO.Contact);
                    if (returnValue.Item1 != "200")
                    {
                        return returnValue;
                    };

                    returnValue = _customerDAL.UpdatePGCustomerId(customerBO.Contact, customerBO.PaymentGateway, returnValue.Item3["PGCustomerId"]);
                    if (returnValue.Item1 != "200")
                    {
                        return returnValue;
                    };
                }

                returnData.Add("PGCustomerId", returnValue.Item3["PGCustomerId"]);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnData);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while creating customer.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
