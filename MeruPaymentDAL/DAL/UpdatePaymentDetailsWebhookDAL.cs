using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using MeruPaymentDAL.EntityModel;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentDAL
{
    public class UpdatePaymentDetailsWebhookDAL : IDisposable
    {
        #region Private Fields and Properties
        private PaymentDAL paymentManager;
        private PaymentHistoryDAL paymentHistoryDAL;
        private static Logger objLogger;
        #endregion

        #region Constructors
        public UpdatePaymentDetailsWebhookDAL()
        {
            paymentManager = new PaymentDAL();
            paymentHistoryDAL = new PaymentHistoryDAL();
            objLogger = LogManager.GetCurrentClassLogger();
        }
        #endregion

        #region Public Properties and Methods
        public void UpdatePaymentDetails(RazorpayPaymentBO razorpayPaymentBO, string updatedBy)
        {
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    var dbPaymentDetails = entities.tbl_PaymentTransaction.SingleOrDefault(x => x.Payment_Transaction_ID == razorpayPaymentBO.MeruPaymentId);
                    if (dbPaymentDetails == null)
                    {
                        return;
                    }
                    dbPaymentDetails.PaymentStatus = (int)razorpayPaymentBO.PaymentStatus;
                    dbPaymentDetails.LastUpdatedOn = DateTime.Now;
                    //dbPaymentDetails.PaymentRefData2 = razorpayPaymentBO.PaymentId;
                    //dbPaymentDetails.PaymentRefData3 = razorpayPaymentBO.PaymentMethodDetail;
                    //dbPaymentDetails.PaymentSource = (int)PaymentGatway.Razorpay;

                    if (entities.Entry(dbPaymentDetails).State == System.Data.Entity.EntityState.Modified)
                        entities.SaveChanges();

                    paymentHistoryDAL.AddStatusChange(dbPaymentDetails.Payment_Transaction_ID, PaymentStatus.PaymentSuccess, updatedBy);
                    objLogger.Info("Payment Details Updated Successfully");
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured while Updating Payment Details");
            }
        }

        public void UpdatePaymentDetailsOld(RazorpayPaymentBO razorpayPaymentBO)
        {
            try
            {
                Int32 payId = Convert.ToInt32(razorpayPaymentBO.MeruPaymentId);
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    var dbPaymentDetails = entities.tblMeruPayments.SingleOrDefault(x => x.PaymentIncrementId == payId);

                    dbPaymentDetails.LastUpdatedOn = DateTime.Now;
                    dbPaymentDetails.PaymentStatus = (int)PaymentStatus.PaymentSuccess;
                    dbPaymentDetails.PaymentRefData2 = razorpayPaymentBO.PaymentId;
                    dbPaymentDetails.PaymentRefData3 = razorpayPaymentBO.PaymentMethodDetail;
                    dbPaymentDetails.PaymentSource = PaymentGatway.Razorpay.ToString();

                    if (entities.Entry(dbPaymentDetails).State == System.Data.Entity.EntityState.Modified)
                        entities.SaveChanges();
                }
                objLogger.Info("Payment Details Updated Successfully");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception occured while updating old Payment Transaction for PaymentIncrementId: " + razorpayPaymentBO.MeruPaymentId);
            }
        }

        public void UpdatePaymentDetails(PayTMTransactionBO payTMTransactionBO, string updatedBy)
        {
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    var dbPaymentDetails = entities.tbl_PaymentTransaction.Single(x => x.Payment_Transaction_ID == payTMTransactionBO.OrderId);
                    if (dbPaymentDetails == null)
                    {
                        return;
                    }

                    dbPaymentDetails.PaymentStatus = (int)PaymentStatus.PaymentSuccess;
                    dbPaymentDetails.LastUpdatedOn = DateTime.Now;
                    dbPaymentDetails.PaymentRefData2 = payTMTransactionBO.TransactionId;

                    JObject objResponse = new JObject(
                            new JProperty("RESPCODE", payTMTransactionBO.ResponseCode),
                            new JProperty("RESPMSG", payTMTransactionBO.ResponseMessage),
                            new JProperty("Issuer", "PayTM")
                        );

                    dbPaymentDetails.PaymentRefData3 = Newtonsoft.Json.JsonConvert.SerializeObject(objResponse, Newtonsoft.Json.Formatting.None);
                    dbPaymentDetails.PaymentSource = (int)PaymentGatway.PayTM;


                    if (entities.Entry(dbPaymentDetails).State == System.Data.Entity.EntityState.Modified)
                        entities.SaveChanges();

                    paymentHistoryDAL.AddStatusChange(dbPaymentDetails.Payment_Transaction_ID, PaymentStatus.PaymentSuccess, updatedBy);

                    objLogger.Info("Payment Details Updated Successfully");
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured while Updating Payment Details");
            }
        }

        public void UpdatePostPaymentDetails(PaymentBO dbPaymentDetails, string Action)
        {
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    string AdditionalDetails = string.Empty;
                    //TODO: Move JOBject to BAL
                    //dynamic RequestReferenceVal = JObject.Parse(dbPaymentDetails.RequestReferenceVal);
                    //AdditionalDetails = RequestReferenceVal.CarNo + "|" + RequestReferenceVal.SPId;
                    entities.USP_UpdatePostPaymentSuccess(dbPaymentDetails.PaymentTransactionId, Convert.ToInt32(dbPaymentDetails.Amount), (int)dbPaymentDetails.PaymentMethod, (int)dbPaymentDetails.PaymentSource, dbPaymentDetails.PaymentReferenceData2, Action, AdditionalDetails);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured in method UpdatePostPaymentDetails(PaymentBO dbPaymentDetails)");
            }
        }
        #endregion

        #region Disposing Methods
        public void Dispose() { }
        #endregion

    }
}
