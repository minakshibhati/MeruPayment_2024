using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;

namespace MeruPaymentBO
{
   public class Checkout
    {
        public bool ShowPayTM { get; set; }

        //Request Data
        //[Required(AllowEmptyStrings = false)]
        public string SPId { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public string CarNo { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public string Source { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public Int64 Amount { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public string PurchaseDescription { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        //[Required(AllowEmptyStrings = false)]
        public string Contact { get; set; }
        //[Required(AllowEmptyStrings = false)]
        private string _PaymentMethod;
        public string PaymentMethod
        {
            get { return _PaymentMethod; }
            set
            {
                _PaymentMethod = value.ToLower() == "unknown" ? "" : value;
            }
        }


        private string _Email;
        //[Required(AllowEmptyStrings = false)]
        public string Email
        {
            get { return _Email; }
            set
            {
                _Email = value;
                if (string.IsNullOrEmpty(_Email))
                {
                    _Email = ConfigurationManager.AppSettings["Razor_Default_EmailId"];
                }
            }
        }

        //System generated
        public string OrderId { get; set; }
        public string MeruPaymentId { get; set; }
        public string RazorPaymentId { get; set; }

        //Config Values
        public string LogoPath { get { return ConfigurationManager.AppSettings["MerchantLogo"]; } }
        public string HostURl { get { return ConfigurationManager.AppSettings["MeruPayment_Host"]; } }
        public string PayTMURL
        {
            get
            {
                return ConfigurationManager.AppSettings["MeruPayment_Host"] + ConfigurationManager.AppSettings["PayTM_CheckoutURL"];
            }
        }

        public string ColorCode { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string MerchantName { get { return ConfigurationManager.AppSettings["MerchantName"]; } }
        [Required(AllowEmptyStrings = false)]
        public string Razor_Key_Id { get { return ConfigurationManager.AppSettings["Razor_Key_Id"]; } }
        [Required(AllowEmptyStrings = false)]
        public string Razor_Key_Secret { get { return ConfigurationManager.AppSettings["Razor_Key_Secret"]; } }
        [Required(AllowEmptyStrings = false)]
        public string Razor_CallBackURL { get { return ConfigurationManager.AppSettings["Razor_CallBackURL"]; } }

        [Required(AllowEmptyStrings = false)]
        public string RazorPayment_CallBackURL { get { return ConfigurationManager.AppSettings["RazorPayment_CallBackURL"]; } }
    }
}
