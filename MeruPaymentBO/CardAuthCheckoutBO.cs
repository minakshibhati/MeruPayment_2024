using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class CardAuthCheckoutBO
    {
        public string PaymentId { get; set; }
        public string Desc { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string PGOrderId { get; set; }
        public string PGCustomerId { get; set; }
        public string AppRequestId { get; set; }

        public string AuthToken { get; set; }
        public string Checksum { get; set; }
        public string AppSource { get; set; }
        public string AppSecret { get; set; }
        public string AppReturnURL { get; set; }
        public string AppColorCode { get; set; }

        public int Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string OrderType { get; set; }

        public string DeviceId { get; set; }

        //private string _RawRequest;
        //public string RawRequest
        //{
        //    get
        //    {
        //        if (_RawRequest != null && _RawRequest.Length > 0)
        //        {
        //            _RawRequest = _RawRequest.Substring(0, _RawRequest.LastIndexOf(("&Checksum")));
        //        }
        //        return _RawRequest;
        //    }
        //    set { _RawRequest = value; }
        //}
        public string RawRequest { get; set; }


        public bool EnableAuthToken { get; set; }
    }
}
