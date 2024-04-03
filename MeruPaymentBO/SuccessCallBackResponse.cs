using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class SuccessCallBackResponse
    {
        public string Order_Id { get; set; }
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }

        public string status_code { get; set; }

        public string appsource { get; set; }
        public string CheckSum { get; set; }

        public string mobilenum { get; set; }

        public string PaymentId { get; set; }

        public string CarNo { get; set; }

        public string SPId { get; set; }

        public string PaymentMethod { get; set; }

        public string PurchaseDescription { get; set; }

        public string email { get; set; }

    }
}
