using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class GetCustomer
    {
        private LogHelper _logHelper;
        private CustomerDAL _customerDAL = null;
        private PaymentDAL _paymentDAL = null;
        
        public GetCustomer()
        {
            _logHelper = new LogHelper("GetOrder()");
            _customerDAL = new CustomerDAL();
            _paymentDAL = new PaymentDAL();
        }

        public Tuple<string, string, CustomerBO> ByMobileNo(string mobileNo, PaymentGatway paymentGateway)
        {
            _logHelper.MethodName = "ByMobileNo(string mobileNo, PaymentGatway paymentGateway)";
            try
            {
                return _customerDAL.GetCustomerDetailByMobileNo(mobileNo, paymentGateway);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting customer detail by mobile number.");
                return new Tuple<string, string, CustomerBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CustomerBO> ByPaymentId(string paymentId)
        {
            _logHelper.MethodName = "ByPaymentId(string paymentId)";
            try
            {
                PaymentBO paymentBO = _paymentDAL.GetMeruPaymentDetail(paymentId);

                return _customerDAL.GetCustomerDetailByMobileNo(paymentBO.Mobile, paymentBO.PaymentSource);

            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting customer detail by payment id.");
                return new Tuple<string, string, CustomerBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, List<CustomerBO>> ByCustomerStatus(CustomerStatus customerStatus) 
        {
            _logHelper.MethodName = "ByCustomerStatus(CustomerStatus customerStatus)";
            try
            {
                return _customerDAL.GetCustomerDetailByCustomerStatus(customerStatus);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting customer detail.");
                return new Tuple<string, string, List<CustomerBO>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
