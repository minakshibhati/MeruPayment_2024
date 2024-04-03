using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class RazorpayPaymentBO
    {
        public string Amount { get; set; }

        /// <summary>
        /// card,netbanking, wallet or emi
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        public string PaymentMethodDetail { get; set; }

        public string PaymentId { get; set; }

        public string OrderId { get; set; }

        public string MeruPaymentId { get; set; }
        /// <summary>
        /// created, authorized, captured, refunded, failed
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; }

        public string ErrorDescription { get; set; }

        public string ErrorCode { get; set; }

        public string AmountRefunded { get; set; }

        /// <summary>
        /// null, partial or full.
        /// </summary>
        public string RefundStatus { get; set; }

        public string TokenId { get; set; }

        public string CustomerId { get; set; }
    }
}
