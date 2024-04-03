using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL.DAL;
using NLog;

namespace MeruPaymentBAL
{
    public class PayTMCheckoutFormBAL : IDisposable
    {
        private Paytm objPaytm = null;
        private PaymentDAL objPaymentDAL = null;
        private bool disposed = false;

        private static Logger objLogger;
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        private PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;

        public PayTMCheckoutFormBAL()
        {
            objPaytm = new Paytm();
            objPaymentDAL = new PaymentDAL();
            objLogger = LogManager.GetCurrentClassLogger();
            objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
        }

        ~PayTMCheckoutFormBAL()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //LogManager.Flush();
                    if (objPaytm != null)
                    {
                        //TODO: objPaytm.dispose();
                    }
                    if (objPaymentDAL != null)
                    {
                        objPaymentDAL.Dispose();
                    }
                }
                disposed = true;
            }
        }

        public string GenerateChecksum(PayTMPaymentRequestBO objPayTMPaymentRequestBO)
        {
            return objPaytm.GenerateChecksumForPayment(objPayTMPaymentRequestBO);
        }

        public PaymentBO GetMeruPaymentDetail(string MeruPaymentId)
        {
            PaymentBO objPaymentBO = objPaymentDAL.GetMeruPaymentDetail(MeruPaymentId);
            return objPaymentBO;
        }

        public PaymentRequestSystemMasterBO GetSourceDetail(string SourceSystemCode)
        {
            try
            {
                if (SourceSystemCode == null || SourceSystemCode.Length == 0)
                {
                    objLogger.Warn("Request data for rayzorpay checkout is null");
                    return null;
                }

                objPaymentRequestSystemMasterBO = objPaymentRequestSystemDAL.GetDetailBySystemCode(SourceSystemCode);
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return objPaymentRequestSystemMasterBO;
        }
    }
}
