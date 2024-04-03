using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeruCommonLibrary;
using NLog;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using System.Text;

namespace MeruPaymentBAL
{
    public class CheckFormRequestBAL
    {
        private static Logger objLogger;
        private StringBuilder LogData = null;

        PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        PaymentDAL objPaymentDAL = null;
        PaymentHistoryDAL objPaymentHistoryDAL = null;

        public CheckFormRequestBAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
            objPaymentDAL = new PaymentDAL();
            LogData = new StringBuilder();
            objPaymentHistoryDAL = new PaymentHistoryDAL();
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

        public string CreatePaymentEntry(PaymentBO objPaymentBO)
        {
            string MeruPaymentId = "";
            try
            {
                if (objPaymentBO.Amount < 100)
                {
                    objLogger.Info(string.Format("Amount cannot be less than 1 rupee. Contact {0}", objPaymentBO.Mobile));
                    return "";
                }

                MeruPaymentId = objPaymentDAL.CreatePayment(objPaymentBO);
                objPaymentHistoryDAL.AddStatusChange(MeruPaymentId, PaymentStatus.PaymentCreated, "TRANS");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return MeruPaymentId;
        }

        public bool ValidateChecksum(string signatureVal, string documentContents, string secretKey)
        {
            bool retValue = false;
            try
            {
                using (HMACSHA256Hash objHMACSHA256Hash = new HMACSHA256Hash(secretKey))
                {
                    retValue = objHMACSHA256Hash.ValidateData(signatureVal, documentContents);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return retValue;
        }

        public PaymentMethod GetPaymentMethodEnum(string objPaymentMethod)
        {
            CommonMethods objCommonMethods = new CommonMethods();
            return objCommonMethods.GetPaymentMethod(objPaymentMethod);
        }
    }
}