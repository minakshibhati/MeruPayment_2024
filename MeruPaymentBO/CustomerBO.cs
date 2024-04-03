using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public enum CustomerStatus
    {
        [DescriptionAttribute("Active Customer")]
        Active = 1,
        [DescriptionAttribute("Blocked Customer")]
        Block = 2
    }

    public class CustomerBO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public PaymentGatway PaymentGateway { get; set; }
        public string PGCustomerId { get; set; }
        public CustomerStatus CustomerStatus { get; set; }
    }
}
