using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public enum PaymentStatus
    {
        [Description("Entry created in database")]
        PaymentCreated = 0, //TODO: Change it to 7 

        [Description("Entry created at payment gateway")]
        PaymentInitiated = 1,

        [DescriptionAttribute("Payment Failed at payment gateway")]
        PaymentFailed = 2,

        [DescriptionAttribute("Payment Authorised at payment gateway")]
        PaymentAuthorised = 9,

        [DescriptionAttribute("Payment Success at payment gateway and Meru")]
        PaymentSuccess = 3,

        [DescriptionAttribute("Payment Cancelled")]
        PaymentCancelled = 4,

        [DescriptionAttribute("Payment Refunded")]
        PaymentRefunded = 5,

        [DescriptionAttribute("Payment Pending")]
        PaymentPending = 6,

        [DescriptionAttribute("Payment Unknown")]
        PaymentUnkown = 8,

        [DescriptionAttribute("Payment Success Via Payment link")]
        PaymentSuccessViaLink = 10
    }
}
