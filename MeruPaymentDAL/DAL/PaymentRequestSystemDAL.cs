using MeruPaymentBO;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using MeruPaymentDAL.EntityModel;

namespace MeruPaymentDAL.DAL
{
    public class PaymentRequestSystemDAL
    {
        #region RESOURCE_DECLARE

        private static Logger objLogger;
        private CDSBusinessEntities db = null;

        private StringBuilder objLogData = null;

        private bool disposed = false;

        #endregion

        #region RESOURC_MANAGEMENT

        public PaymentRequestSystemDAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            db = new CDSBusinessEntities();
            objLogData = new StringBuilder();
        }

        ~PaymentRequestSystemDAL()
        {
            Dispose(false);
        }

        public void Dispose()
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
                    //Free managed resources
                    //LogManager.Flush();
                    db.Dispose();
                }
                // Free native or unmanaged resources
                disposed = true;
            }
        }
        #endregion

        #region LOGIC

        public PaymentRequestSystemMasterBO GetDetailBySystemCode(string SourceSystemCode)
        {
            PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
            try
            {
                objLogData.Append(string.Format("System Code: {0} ", SourceSystemCode));
                tbl_Payment_Request_System objPaymentRequestSystemMaster = db.tbl_Payment_Request_System.SingleOrDefault<tbl_Payment_Request_System>(d => d.Payment_Request_System == SourceSystemCode && d.Record_Status == "A");
                if (objPaymentRequestSystemMaster != null)
                {
                    objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
                    objPaymentRequestSystemMasterBO.RequestKeyInput = objPaymentRequestSystemMaster.Payment_Request_System_KeyInput;
                    objPaymentRequestSystemMasterBO.SecretCode = objPaymentRequestSystemMaster.Payment_Request_System_Secret;
                    objPaymentRequestSystemMasterBO.ReturnURL = objPaymentRequestSystemMaster.Payment_Request_System_Return_URL;
                    objPaymentRequestSystemMasterBO.ColorCode = objPaymentRequestSystemMaster.ColorCode;
                    objPaymentRequestSystemMasterBO.QueueName = objPaymentRequestSystemMaster.QueueName;
                    objPaymentRequestSystemMasterBO.SPName = objPaymentRequestSystemMaster.SPName;
                    objPaymentRequestSystemMasterBO.RequestSourceName = objPaymentRequestSystemMaster.Payment_Request_System;
                    objPaymentRequestSystemMasterBO.EnableAuthToken = Convert.ToString(objPaymentRequestSystemMaster.EnableAuthToken);//0 and 1
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, string.Format("Refdata: {0}", objLogData.ToString()));
            }
            return objPaymentRequestSystemMasterBO;
        }

        #endregion
    }
}