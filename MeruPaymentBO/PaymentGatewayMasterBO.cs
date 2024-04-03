using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public enum PaymentGatway
    {
        [DescriptionAttribute("Razorpay")]
        Razorpay = 1,
        [DescriptionAttribute("PayTM")]
        PayTM = 2,
        [DescriptionAttribute("Unknown")]
        Unknown = 3
    }

    public class PaymentGatewayMasterBO
    {
    }
}
