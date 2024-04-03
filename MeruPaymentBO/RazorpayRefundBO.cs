using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class RazorpayRefundBO
    {
        public string RefundId { get; set; }
        public long RefundAmount { get; set; }
        public string PaymentId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
