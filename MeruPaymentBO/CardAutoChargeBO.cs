using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class CardAutoChargeBO
    {
        public string AppRequestId { get; set; }
        public string PGOrderId { get; set; }
        public string PGCustomerId { get; set; }
        public string PGTokenId { get; set; }

        public long Amount { get; set; }
        public string Email { get; set; }
        public int CardId { get; set; }
        public string AppSource { get; set; }
        public string AppSecret { get; set; }
        public string PaymentId { get; set; }
        public string Desc { get; set; }
        public string FullName { get; set; }
        public string Contact { get; set; }

        public string AuthToken { get; set; }
        public string Checksum { get; set; }

        public string AppReturnURL { get; set; }

        public PaymentGatway PaymentGateway { get; set; }

        public string PaymentType { get; set; }

        public string RawRequest { get; set; }
    }
}
