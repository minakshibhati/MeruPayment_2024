using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public enum OrderStatus
    {
        [Description("Entry created in razorpay database")]
        OrderCreated = 1,
        [Description("Waiting for response from bank")]
        OrderAttempted = 2,
        [Description("Got response from bank")]
        OrderPaid = 3
    }

    public class RazorpayOrderBO
    {
        public string OrderId { get; set; }
        public long OrderAmount { get; set; }
        public string ReceiptId { get; set; }
        public OrderStatus Status { get; set; }
        public long Attempts { get; set; }
    }
}
