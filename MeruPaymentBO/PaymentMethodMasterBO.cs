using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public enum PaymentMethod
    {
        [DescriptionAttribute("Any Card")]
        card = 1,
        [DescriptionAttribute("Debit Card")]
        debit = 2,
        [DescriptionAttribute("Credit Card")]
        credit = 3,
        [DescriptionAttribute("Net Banking")]
        netbanking = 4,
        [DescriptionAttribute("Wallet")]
        wallet = 5,
        [DescriptionAttribute("EMI")]
        emi = 6,
        [DescriptionAttribute("UPI")]
        upi = 7,
        [DescriptionAttribute("Unknown")]
        Unknown = 8
    }

    class PaymentMethodMasterBO
    {
    }
}
