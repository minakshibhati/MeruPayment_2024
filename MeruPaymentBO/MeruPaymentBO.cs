using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PaymentBO
    {
        public string PaymentTransactionId { get; set; }
        public Int64 MeruPaymentId { get; set; }
        public string RequestSource { get; set; }
        public string RequestUniqueId { get; set; }
        public Int64 Amount { get; set; }
        public string PurchaseDesc { get; set; }
        public PaymentGatway PaymentSource { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        
        public PaymentStatus PaymentStatus { get; set; }
        //public string PaymentDetail { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public string RequestReferenceVal { get; set; }

        public string PaymentReferenceValue { get; set; }

        public string PaymentReferenceData1 { get; set; }

        public string PaymentReferenceData2 { get; set; }

        public string PaymentReferenceData3 { get; set; }

        public Int64 RefundAmount { get; set; }

        public string PaymentType { get; set; }
        public string DeviceId { get; set; }

        public int PaymentMethodRefId { get; set; }

        public int? JobId { get; set; }

        public int ProviderId { get; set; }

        public string OrderId { get; set; }

        public int? TripId { get; set; }

        public int? Id { get; set; }
    }
}
