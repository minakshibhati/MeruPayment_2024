using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentReconcileService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            #if(!DEBUG)
            {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new MeruPaymentReconcileService() 
            };
            ServiceBase.Run(ServicesToRun);
            }
#else
            {
                MeruPaymentReconcileService obj = new MeruPaymentReconcileService();
                obj.OnDebug();
            }
#endif
        }
    }
}
