using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class OrderBO
    {
        public string AppSource { get; set; }
        public long Amount { get; set; }
        public string Desc { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }

        public string AppRequestId { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string OrderType { get; set; }

        public string PaymentId { get; set; }
        public string PGOrderId { get; set; }

        public string DeviceId { get; set; }

        public int PaymentMethodRefId { get; set; }

        public string AppRequestRefVal { get; set; }
    }
}
