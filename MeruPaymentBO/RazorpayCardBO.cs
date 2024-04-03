using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class RazorpayCardBO
    {
        //credit, debit, unknown
        public string CardType { get; set; }

        public string FullName { get; set; }

        public string Last4 { get; set; }

        public string Network { get; set; }

        public string Issuer { get; set; }

        public bool IsInternational { get; set; }

        public bool IsEMI { get; set; }

        public string CardId { get; set; }
    }
}
