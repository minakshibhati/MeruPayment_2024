using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentDAL.EntityModel;
using NLog;

namespace MeruPaymentDAL.DAL
{
    public class PaymentLinkDAL : IDisposable
    {
        #region Private Fields
        private static Logger objLogger;
        #endregion

        public PaymentLinkDAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
        }

        public void SavePaymentLink(PaymentLinkBO objPaymentLinkBO)
        {
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    db.tbl_Payment_Link.Add(new tbl_Payment_Link
                    {
                        Contact = objPaymentLinkBO.Contact,
                        CreatedOn = DateTime.Now,
                        Description = objPaymentLinkBO.Description,
                        Email = objPaymentLinkBO.Email,
                        LastUpdatedOn = DateTime.Now,
                        Payment_Amount_Paise = objPaymentLinkBO.Payment_Amount_Paise,
                        Payment_Method = objPaymentLinkBO.Payment_Method,
                        Payment_Source = objPaymentLinkBO.Payment_Source,
                        Payment_Transaction_ID = objPaymentLinkBO.Payment_Transaction_ID,
                        PG_InvoiceId = objPaymentLinkBO.PG_InvoiceId,
                        PG_OrderId = objPaymentLinkBO.PG_OrderId,
                        PG_PaymentId = objPaymentLinkBO.PG_PaymentId,
                        PG_ReceiptNo = objPaymentLinkBO.PG_ReceiptNo,
                        PG_Ref_Data = objPaymentLinkBO.PG_Ref_Data,
                        Request_RefId = objPaymentLinkBO.Request_RefId,
                        Status = objPaymentLinkBO.Status,
                        Url = objPaymentLinkBO.Url
                    });
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        objLogger.Info("payment link created successfully for trip Id: " + objPaymentLinkBO.Request_RefId + " contact: " + objPaymentLinkBO.Contact);
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Error when saving payment link to DB");
            }
        }

        public void UpdatePGRefDetail()
        {
            //TODO:
        }

        #region Public Properries and Methods

        public PaymentLinkBO GetPaymentLinkDetails(string requestRefId)
        {
            PaymentLinkBO objpaymentLink = null;
            try
            {
                objpaymentLink = GetPaymentLinkDetails().Where(a => a.Request_RefId == requestRefId).SingleOrDefault();

                //using (CDSBusinessEntities entities = new CDSBusinessEntities())
                //{
                //    tbl_Payment_Link dbPaymentDetails = entities.tbl_Payment_Link.SingleOrDefault(x => x.Request_RefId == requestRefId);
                //    if (dbPaymentDetails == null)
                //    {
                //        return null;
                //    }
                //    objpaymentLink = new PaymentLinkBO
                //    {
                //        Payment_Transaction_ID = dbPaymentDetails.Payment_Transaction_ID,
                //        Request_RefId = dbPaymentDetails.Request_RefId,
                //        Payment_Amount_Paise = dbPaymentDetails.Payment_Amount_Paise,
                //        PG_OrderId = dbPaymentDetails.PG_OrderId,
                //        PG_PaymentId = dbPaymentDetails.PG_PaymentId,
                //        PG_InvoiceId = dbPaymentDetails.PG_InvoiceId,
                //        PG_ReceiptNo = dbPaymentDetails.PG_ReceiptNo,
                //        PG_Ref_Data = dbPaymentDetails.PG_Ref_Data,
                //        Payment_Source = dbPaymentDetails.Payment_Source,
                //        Payment_Method = dbPaymentDetails.Payment_Method,
                //        Url = dbPaymentDetails.Url,
                //        Description = dbPaymentDetails.Description,
                //        Contact = dbPaymentDetails.Contact,
                //        Email = dbPaymentDetails.Email,
                //        Status = dbPaymentDetails.Status,
                //        CreatedOn = dbPaymentDetails.CreatedOn,
                //        LastUpdatedOn = dbPaymentDetails.LastUpdatedOn,
                //    };
                //}
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured while processing Payment Link model");
            }
            return objpaymentLink;
        }

        public List<PaymentLinkBO> GetPaymentLinkDetails()
        {
            List<PaymentLinkBO> objpaymentLink = new List<PaymentLinkBO>();
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    objpaymentLink = entities.tbl_Payment_Link.Select(dbPaymentDetails =>
                    new PaymentLinkBO
                    {
                        Payment_Transaction_ID = dbPaymentDetails.Payment_Transaction_ID,
                        Request_RefId = dbPaymentDetails.Request_RefId,
                        Payment_Amount_Paise = dbPaymentDetails.Payment_Amount_Paise,
                        PG_OrderId = dbPaymentDetails.PG_OrderId,
                        PG_PaymentId = dbPaymentDetails.PG_PaymentId,
                        PG_InvoiceId = dbPaymentDetails.PG_InvoiceId,
                        PG_ReceiptNo = dbPaymentDetails.PG_ReceiptNo,
                        PG_Ref_Data = dbPaymentDetails.PG_Ref_Data,
                        Payment_Source = dbPaymentDetails.Payment_Source,
                        Payment_Method = dbPaymentDetails.Payment_Method,
                        Url = dbPaymentDetails.Url,
                        Description = dbPaymentDetails.Description,
                        Contact = dbPaymentDetails.Contact,
                        Email = dbPaymentDetails.Email,
                        Status = dbPaymentDetails.Status,
                        CreatedOn = dbPaymentDetails.CreatedOn,
                        LastUpdatedOn = dbPaymentDetails.LastUpdatedOn,
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured while processing Payment Link model");
            }
            return objpaymentLink;
        }

        public void UpdatePaymentlinkDetails(RazorpayPaymentBO paymentLinkDetails, string updatedBy)
        {
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    var dbPaymentLinkDetails = entities.tbl_Payment_Link.FirstOrDefault(x => x.Payment_Transaction_ID == paymentLinkDetails.MeruPaymentId);
                    if (dbPaymentLinkDetails == null)
                    {
                        objLogger.Info("No record found in data base for Payment Transaction Id :" + paymentLinkDetails.MeruPaymentId);
                        return;
                    }

                    dbPaymentLinkDetails.Status = (int)PaymentStatus.PaymentSuccess;
                    dbPaymentLinkDetails.LastUpdatedOn = DateTime.Now;
                    dbPaymentLinkDetails.PG_OrderId = paymentLinkDetails.OrderId;
                    dbPaymentLinkDetails.PG_PaymentId = paymentLinkDetails.PaymentId;
                    dbPaymentLinkDetails.PG_Ref_Data = paymentLinkDetails.PaymentMethodDetail;
                    dbPaymentLinkDetails.Payment_Method = (int)paymentLinkDetails.PaymentMethod;
                    dbPaymentLinkDetails.Payment_Source = (int)PaymentGatway.Razorpay;

                    if (entities.Entry(dbPaymentLinkDetails).State == System.Data.Entity.EntityState.Modified)
                    {
                        entities.SaveChanges();
                        objLogger.Info("Payment link details upadted successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured while updating payment link details");
            }
        }       

        //public List<usp_Wallet_GetPartialAmountTransactionDetails_Result> GetPendingPayments()
        //{
        //    List<usp_Wallet_GetPartialAmountTransactionDetails_Result> pendingPayments = null;
        //    try
        //    {
        //        using (CDSBusinessEntities entities = new CDSBusinessEntities())
        //        {
        //            pendingPayments = entities.usp_Wallet_GetPartialAmountTransactionDetails().ToList();

        //            #region Get Pending Payments from tbl_Payment_Link
        //            //pendingPayments = entities.tbl_Payment_Link.Select(payment => new PaymentLinkBO
        //            //            {
        //            //                Payment_Transaction_ID = payment.Payment_Transaction_ID,
        //            //                Request_RefId = payment.Request_RefId,
        //            //                Payment_Amount_Paise = payment.Payment_Amount_Paise,
        //            //                PG_OrderId = payment.PG_OrderId,
        //            //                PG_PaymentId = payment.PG_PaymentId,
        //            //                PG_InvoiceId = payment.PG_InvoiceId,
        //            //                PG_ReceiptNo = payment.PG_ReceiptNo,
        //            //                PG_Ref_Data = payment.PG_Ref_Data,
        //            //                Payment_Source = payment.Payment_Source,
        //            //                Payment_Method = payment.Payment_Method,
        //            //                Url = payment.Url,
        //            //                Description = payment.Description,
        //            //                Contact = payment.Contact,
        //            //                Email = payment.Email,
        //            //                Status = payment.Status,
        //            //                CreatedOn = payment.CreatedOn,
        //            //                LastUpdatedOn = payment.LastUpdatedOn
        //            //            }).ToList(); 
        //            #endregion
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.Error(ex);
        //    }
        //    return pendingPayments;
        //}
        #endregion

        #region Disposing Methods
        public void Dispose() { }
        #endregion
    }
}
