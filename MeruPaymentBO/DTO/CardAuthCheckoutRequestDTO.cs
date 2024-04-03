using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    //[DataContract]
    public class CardAuthCheckoutRequestDTO
    {
        //[DataMember]
        public string fullname { get; set; }
        public string email { get; set; }
        public string apprequestid { get; set; }
        public string Authorization { get; set; }
        [Required]
        public string Checksum { get; set; }
        public string Mobile { get; set; }
        public string AppSource { get; set; }
        public string DeviceId { get; set; }
    }
}
