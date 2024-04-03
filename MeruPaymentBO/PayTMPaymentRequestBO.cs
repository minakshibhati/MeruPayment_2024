using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PayTMPaymentRequestBO
    {
        public string ChannelId { get; set; }

        public string Website { get; set; }

        private string _EmailId = "";
        public string EmailId
        {
            get
            {
                if (_EmailId == null)
                {
                    _EmailId = "";
                }

                return _EmailId;
            }
            set { _EmailId = value; }
        }

        public string MobileNo { get; set; }

        public string CustomerId { get; set; }

        public string OrderId { get; set; }

        //public string TransAmount { get; set; }

        private string _TransAmount;

        public string TransAmount
        {
            get { return _TransAmount; }
            set
            {
                _TransAmount = (Convert.ToDecimal(value) / 100).ToString();
            }
        }
    }
}
