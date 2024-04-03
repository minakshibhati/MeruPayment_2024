using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class CardDeleteBO
    {
        public string AppSource { get; set; }
        public string AppSecret { get; set; }
        public string Checksum { get; set; }
        public string AuthToken { get; set; }
        public string Contact { get; set; }
        public int CardId { get; set; }
        public PaymentGatway PaymentGateway { get; set; }
    }
}
