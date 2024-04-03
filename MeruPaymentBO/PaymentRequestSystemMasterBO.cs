using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    //public enum PaymentRequestSystem
    //{
    //    [DescriptionAttribute("com.merucabs.dis")]
    //    com_merucabs_dis = 1,
    //    [DescriptionAttribute("com.meru.merumobile")]
    //    com_meru_merumobile = 2,
    //    [DescriptionAttribute("outstationweb")]
    //    outstation_web = 3
    //}

    public class PaymentRequestSystemMasterBO
    {
        public string SecretCode { get; set; }
        public string RequestKeyInput { get; set; }
        public string ReturnURL { get; set; }
        public string ColorCode { get; set; }
        public string QueueName { get; set; }
        public string RequestSourceName { get; set; }
        public string EnableAuthToken { get; set; }
        public string SPName { get; set; }
    }
}
