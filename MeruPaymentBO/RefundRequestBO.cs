using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class RefundRequestBO
    {
        public string Action { get; set; }
        public string MId { get; set; }
        public Int64 Amount { get; set; }
        public string Note { get; set; }
    }
}
