using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace MeruPaymentBO
{
    public class CardAutoChargeRequestDTO
    {
        [Required(ErrorMessage="{0} is required.")]
        public string apprequestid { get; set; }
        
        [Range(100, 1000000, ErrorMessage = "Value for {0} should be between {1} and {2}.")]
        public long amount { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        public string email { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        public string cardId { get; set; }
        
        public string desc { get; set; }

        public string paymenttype { get; set; }
    }
}
