using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PayTMCheckout
    {
        public string MID { get; set; }
        public string WEBSITE { get; set; }
        public string ORDER_ID { get; set; }
        public string CUST_ID { get; set; }
        public string MOBILE_NO { get; set; }
        public string EMAIL { get; set; }
        public string INDUSTRY_TYPE_ID { get; set; }
        public string CHANNEL_ID { get; set; }
        public string TXN_AMOUNT { get; set; }  
        public string CALLBACK_URL { get; set; }
        public string CHECKSUMHASH { get; set; }
        public string TRANURL { get; set; }
        public string PAYMENT_MODE_ONLY { get; set; }
        public string AUTH_MODE { get; set; }
        public string PAYMENT_TYPE_ID { get; set; }
        public string MERC_UNQ_REF { get; set; }
    }
}
