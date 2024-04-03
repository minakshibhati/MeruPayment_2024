using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeruPaymentBO
{
    public class CardBO
    {
        public string PGCustomerId { get; set; }
        public string PGCardTokenId { get; set; }
        //credit, debit, unknown
        public string CardType { get; set; }

        public string FullName { get; set; }

        public string Last4 { get; set; }

        public string Network { get; set; }

        public string Issuer { get; set; }

        public bool IsInternational { get; set; }

        public bool IsEMI { get; set; }

        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }

        public DateTime ExpityDateTime { get; set; }
        public PaymentGatway PaymentGateway { get; set; }

        public string DeviceId { get; set; }

        public DateTime RecordCreatedDateTime { get; set; }
    }
}
