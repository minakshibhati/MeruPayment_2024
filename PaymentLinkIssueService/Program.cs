using MeruPaymentBAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PaymentLinkIssueService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

//#if !RELEASE

//            PaymentLinkIssue paymentLinkIssue = new PaymentLinkIssue();
//            paymentLinkIssue.ProcessLink();
//#endif

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PaymentLinkIssueService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
