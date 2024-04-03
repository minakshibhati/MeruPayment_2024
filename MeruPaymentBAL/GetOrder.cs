using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class GetOrder
    {
        private LogHelper _logHelper;
        private Tuple<string, string, OrderBO> returnValue = null;
        private OrderDAL _orderDAL = null;

        public GetOrder()
        {
            _logHelper = new LogHelper("GetOrder()");
            _orderDAL = new OrderDAL();
        }

        public Tuple<string, string, OrderBO> ByPaymentId(string PaymentId)
        {
            _logHelper.MethodName = "ByPaymentId(string PaymentId)";
            try
            {
                return _orderDAL.GetOrderDetailByPaymentId(PaymentId);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting order detail.");
                return new Tuple<string, string, OrderBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
