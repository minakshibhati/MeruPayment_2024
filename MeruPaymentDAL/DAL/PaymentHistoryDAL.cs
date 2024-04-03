using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using MeruPaymentDAL.EntityModel;
using MeruCommonLibrary;

namespace MeruPaymentDAL.DAL
{
    public class PaymentHistoryDAL
    {
        private LogHelper _logHelper;
        public PaymentHistoryDAL()
        {
            _logHelper = new LogHelper("PaymentHistoryDAL");
        }

        public long AddStatusChange(string MeruPaymentId, PaymentStatus objPaymentStatus, string UpdatedBy)
        {
            Int64 RequestId = 0;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    tbl_Payment_History objPaymentRequestHistory = new tbl_Payment_History();

                    objPaymentRequestHistory.PaymentTransactionId = MeruPaymentId;
                    objPaymentRequestHistory.Payment_Status_ID = (int)objPaymentStatus;
                    objPaymentRequestHistory.Record_Created_DateTime = DateTime.Now;
                    objPaymentRequestHistory.Updating_Process = UpdatedBy;
                    db.tbl_Payment_History.Add(objPaymentRequestHistory);
                    db.SaveChanges();
                    RequestId = objPaymentRequestHistory.Payment_Request_ID;
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while updating payment status to DB");
            }
            return RequestId;
        }
    }
}