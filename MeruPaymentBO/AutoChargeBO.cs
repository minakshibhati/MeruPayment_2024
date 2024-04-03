using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class AutoChargeBO
    {
        public string CardId { get; set; }
        public Int64 Amount { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string ReferenceId { get; set; }
        public string PurchaseDescription { get; set; }
    }
}
