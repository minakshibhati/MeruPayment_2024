using MeruPaymentBAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PaymentInstrumentDisableService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if !RELEASE

            //DisablePaymentInstrument disablePaymentInstrument = new DisablePaymentInstrument();
            //disablePaymentInstrument.Process();
#endif

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new PaymentInstrumentDisableService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
