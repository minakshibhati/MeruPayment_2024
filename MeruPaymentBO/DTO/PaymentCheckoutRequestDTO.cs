using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PaymentCheckoutRequestDTO
    {
        //[DataMember]
        public string Name { get; set; }
        public string Email { get; set; }
        public string ReferenceId { get; set; }
        public string Authorization { get; set; }
        [Required]
        public string Checksum { get; set; }

        public string Contact { get; set; }
        public string Source { get; set; }
        //public string DeviceId { get; set; }

        public string Amount { get; set; }
        public string PurchaseDescription { get; set; }
        //public string PaymentMethod { get; set; }

        private string _PaymentMethod;
        public string PaymentMethod
        {
            get { return (_PaymentMethod == null) ? "" : _PaymentMethod; }
            set { _PaymentMethod = value; }
        }



        public string RawRequest { get; set; }
    }
}
