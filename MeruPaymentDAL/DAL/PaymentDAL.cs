using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;
using MeruPaymentBO;
using MeruPaymentDAL.EntityModel;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeruPaymentDAL.DAL
{
    public class PaymentDAL : IDisposable
    {
        #region RESOURCE_DECLARE
        //Ref:https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netframework-4.7.2

        private static Logger objLogger;
        private bool disposed = false;

        #endregion

        #region RESOURC_MANAGEMENT

        public PaymentDAL()
        {
            objLogger = LogManager.GetCurrentClassLogger();
        }

        ~PaymentDAL()
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
                    //db.Dispose();
                }
                // Free native or unmanaged resources
                disposed = true;
            }
        }

        #endregion

        #region LOGIC

        public PaymentBO GetMeruPaymentDetail(string MeruPaymentId)
        {
            PaymentBO objPaymentBO = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    
                    tbl_PaymentTransaction objMeruPayments = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault<tbl_PaymentTransaction>(); //Find(MeruPaymentId);

                    objPaymentBO = GetPaymentBO(objMeruPayments);

                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPaymentBO;
        }

        public PaymentBO GetMeruPaymentDetailOld(Int32 MeruPaymentId)
        {
            PaymentBO objPaymentBO = null;
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    tblMeruPayment objMeruPayment = entities.tblMeruPayments.SingleOrDefault(x => x.PaymentIncrementId == MeruPaymentId);
                    objPaymentBO = GetPaymentBO(objMeruPayment);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return objPaymentBO;
        }


        public List<PaymentBO> GetMeruPaymentDetail_ByAppRequestId(string AppRequestId)
        {
            List<PaymentBO> lstPaymentBO = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var payments = db.tbl_PaymentTransaction.Where(a => a.RequestRefId == AppRequestId).ToList(); //Find(MeruPaymentId);
                    lstPaymentBO = GetPaymentBOList(payments);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return lstPaymentBO;
        }

        public List<PaymentBO> GetMeruPaymentDetail_ByPaymentMethodRefId_Contact_Date(int PaymentMethodRefId, string Contact, DateTime FromDate)
        {
            List<PaymentBO> lstPaymentBO = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var payments = db.tbl_PaymentTransaction.Where(a => a.PaymentMethodRefId == PaymentMethodRefId && a.Contact == Contact && a.CreatedOn >= FromDate).ToList(); //Find(MeruPaymentId);
                    lstPaymentBO = GetPaymentBOList(payments);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return lstPaymentBO;
        }

        public List<PaymentBO> GetMeruPaymentDetail_Date(int? PaymentMethodRefId, DateTime FromDate)
        {
            List<PaymentBO> lstPaymentBO = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var payments = db.tbl_PaymentTransaction.Where(a => a.CreatedOn >= FromDate && a.PaymentMethodRefId != PaymentMethodRefId).ToList(); //Find(MeruPaymentId);
                    lstPaymentBO = GetPaymentBOList(payments);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return lstPaymentBO;
        }

        private List<PaymentBO> GetPaymentBOList(List<tbl_PaymentTransaction> payments)
        {
            List<PaymentBO> lstPaymentBO = null;
            if (payments != null)
            {
                lstPaymentBO = new List<PaymentBO>();
                foreach (var item in payments)
                {
                    lstPaymentBO.Add(GetPaymentBO(item));
                }
            }
            return lstPaymentBO;
        }

        private PaymentBO GetPaymentBO(tbl_PaymentTransaction item)
        {
            if (item == null)
            {
                return null;
            }

            return new PaymentBO
            {
                MeruPaymentId = item.PaymentIncrementId,
                PaymentTransactionId = item.Payment_Transaction_ID,
                RequestSource = item.RequestSource,
                RequestUniqueId = item.RequestRefId,
                RequestReferenceVal = item.RequestRefValues,
                Amount = item.Payment_Amount_Paise,
                RefundAmount = item.Refund_Amount_Paise,
                PaymentSource = (PaymentGatway)item.PaymentSource,
                PaymentMethod = (PaymentMethod)item.PaymentMethod,
                PaymentStatus = (PaymentStatus)item.PaymentStatus,
                CreatedOn = item.CreatedOn,
                LastUpdatedOn = item.LastUpdatedOn,
                PaymentReferenceValue = item.PaymentRefValues,
                PaymentReferenceData1 = item.PaymentRefData1,
                PaymentReferenceData2 = item.PaymentRefData2,
                PaymentReferenceData3 = item.PaymentRefData3,
                PurchaseDesc = item.PurchaseDescription,
                FullName = item.FullName,
                Email = item.Email,
                Mobile = item.Contact,
                PaymentType = item.Payment_Type,
                DeviceId = item.DeviceId,
                PaymentMethodRefId = Convert.ToInt32(item.PaymentMethodRefId)
            };
        }

        private PaymentBO GetPaymentBO(tblMeruPayment item)
        {
            if (item == null)
            {
                return null;
            }
            PaymentBO objPaymentBO = new PaymentBO
            {

                MeruPaymentId = item.PaymentIncrementId,
                PaymentTransactionId = Convert.ToString(item.PaymentIncrementId),
                RequestSource = item.RequestSource,
                //RequestUniqueId = item.RequestRefId,
                RequestReferenceVal = item.RequestRefValues,
                Amount = item.PaymentAmount,
                RefundAmount = item.PaymentAmount,
                //PaymentSource = item.RequestSource,
                //PaymentMethod = (PaymentMethod)((int)item.PaymentMethod),
                PaymentStatus = (PaymentStatus)item.PaymentStatus,
                CreatedOn = item.CreatedOn,
                LastUpdatedOn = item.LastUpdatedOn,
                PaymentReferenceValue = item.PaymentRefValues,
                PaymentReferenceData1 = item.PaymentRefData1,
                PaymentReferenceData2 = item.PaymentRefData2,
                PaymentReferenceData3 = item.PaymentRefData3,
                PurchaseDesc = item.PaymentDetail,
                //FullName = item.FullName,
                //Email = item.Email,
                //Mobile = item.Contact,
                //PaymentType = item.Payment_Type,
                //DeviceId = item.DeviceId,
                //PaymentMethodRefId = Convert.ToInt32(item.PaymentMethodRefId)
            };

            #region Payment Source
            if (item.PaymentSource == "PayTM")
                objPaymentBO.PaymentSource = PaymentGatway.PayTM;

            if (item.PaymentSource == "RazorPay")
                objPaymentBO.PaymentSource = PaymentGatway.Razorpay;

            if (objPaymentBO.PaymentSource == null)
                objPaymentBO.PaymentSource = PaymentGatway.Unknown;
            #endregion

            #region Payment Method
            if (item.PaymentMethod == "upi")
                objPaymentBO.PaymentMethod = PaymentMethod.upi;

            if (item.PaymentMethod == "netbanking")
                objPaymentBO.PaymentMethod = PaymentMethod.netbanking;

            if (item.PaymentMethod == "card")
                objPaymentBO.PaymentMethod = PaymentMethod.card;

            if (item.PaymentMethod == "wallet")
                objPaymentBO.PaymentMethod = PaymentMethod.wallet;

            if (item.PaymentMethod == "credit")
                objPaymentBO.PaymentMethod = PaymentMethod.credit;

            if (item.PaymentMethod == "debit")
                objPaymentBO.PaymentMethod = PaymentMethod.debit;

            if (item.PaymentMethod == "emi")
                objPaymentBO.PaymentMethod = PaymentMethod.emi;

            if (item.PaymentMethod == null)
                objPaymentBO.PaymentMethod = PaymentMethod.Unknown;
            #endregion

            return objPaymentBO;
        }

        public string CreatePayment(PaymentBO objPaymentBO)
        {
            string MeruPaymentId = "";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    tbl_PaymentTransaction objMeruPayments = new tbl_PaymentTransaction();

                    objMeruPayments.RequestSource = objPaymentBO.RequestSource;
                    objMeruPayments.RequestRefValues = objPaymentBO.RequestReferenceVal;
                    objMeruPayments.RequestRefId = objPaymentBO.RequestUniqueId;
                    objMeruPayments.Payment_Amount_Paise = objPaymentBO.Amount;
                    objMeruPayments.PaymentMethod = (int)objPaymentBO.PaymentMethod;
                    objMeruPayments.PaymentStatus = (int)PaymentStatus.PaymentCreated;
                    objMeruPayments.PaymentSource = (int)PaymentGatway.Unknown;
                    objMeruPayments.CreatedOn = DateTime.Now;
                    objMeruPayments.LastUpdatedOn = DateTime.Now;
                    objMeruPayments.PurchaseDescription = objPaymentBO.PurchaseDesc;
                    objMeruPayments.FullName = objPaymentBO.FullName;
                    objMeruPayments.Email = objPaymentBO.Email;
                    objMeruPayments.Contact = objPaymentBO.Mobile;
                    objMeruPayments.Payment_Transaction_ID = Guid.NewGuid().ToString();
                    objMeruPayments.Payment_Type = objPaymentBO.PaymentType;
                    //objMeruPayments.Payment_Gateway_Code = "";
                    //objMeruPayments.Payment_Method_Code = "";
                    //objMeruPayments.Payment_Status_Code = "";
                    //objMeruPayments.PG_Transaction_Code = "";

                    db.tbl_PaymentTransaction.Add(objMeruPayments);
                    db.SaveChanges();

                    objMeruPayments.Payment_Transaction_ID = "MP" + DateTime.Now.ToString("dd") +
                        objPaymentBO.RequestUniqueId.Substring(objPaymentBO.RequestUniqueId.Length - 2) +
                        objMeruPayments.PaymentIncrementId.ToString() +
                        new Random().Next(999).ToString("D3");

                    objMeruPayments.LastUpdatedOn = DateTime.Now;
                    db.Entry(objMeruPayments).State = EntityState.Modified;
                    db.SaveChanges();

                    MeruPaymentId = objMeruPayments.Payment_Transaction_ID;// objMeruPayments.MeruPaymentsId;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return MeruPaymentId;
        }

        public bool InitializePayment(PaymentBO objPaymentBO)
        {
            bool returnStatus = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == objPaymentBO.PaymentTransactionId).SingleOrDefault(); //Find(objPaymentBO.MeruPaymentId);
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", objPaymentBO.MeruPaymentId));
                        return returnStatus;
                    }

                    trans.PaymentSource = (int)objPaymentBO.PaymentSource;
                    trans.PaymentRefValues = objPaymentBO.PaymentReferenceValue;
                    trans.PaymentRefData1 = objPaymentBO.PaymentReferenceData1;
                    trans.PaymentStatus = (int)PaymentStatus.PaymentInitiated;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    returnStatus = true;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return returnStatus;
        }

        public bool TransactionCancelled(string MeruPaymentId, PaymentGatway objPaymentGateway)
        {
            bool returnStatus = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
                        return returnStatus;
                    }
                    trans.PaymentSource = (int)objPaymentGateway;
                    trans.PaymentStatus = (int)PaymentStatus.PaymentCancelled;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    returnStatus = true;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return returnStatus;
        }

        public Tuple<string, string, Dictionary<string, string>> CancelPayment(string MeruPaymentId)
        {
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId),
                            null);
                    }
                    trans.PaymentStatus = (int)PaymentStatus.PaymentCancelled;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();

                    Dictionary<string, string> returnData = new Dictionary<string, string>();
                    returnData.Add("PaymentType", trans.Payment_Type);

                    return new Tuple<string, string, Dictionary<string, string>>(
                     "200",
                     "Success",
                     returnData);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public List<PaymentBO> GetAllPaymentsForReconcileOld(int minutes)
        {
            List<PaymentBO> lstPaymentBO = new List<PaymentBO>(); 
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    DateTime obj = DateTime.Now.AddMinutes(-minutes);
                    List<tblMeruPayment> lstMeruPayments = db.tblMeruPayments.Where(
                        a => a.Payment_Reconciled != true && a.PaymentStatus != (int)PaymentStatus.PaymentSuccess && a.CreatedOn < obj && a.PaymentIncrementId > 0).ToList();

                    if (lstMeruPayments != null && lstMeruPayments.Count > 0)
                    {
                        foreach (tblMeruPayment item in lstMeruPayments)
                        {
                            lstPaymentBO.Add(new PaymentBO
                            {
                                MeruPaymentId = item.PaymentIncrementId,
                                RequestSource = item.RequestSource,
                                PaymentTransactionId = "",
                                RequestUniqueId = "",//item.RequestRefValues,
                                Amount = item.PaymentAmount,
                                PaymentReferenceData1 = item.PaymentRefData1,
                                PaymentReferenceData2 = item.PaymentRefData2,
                                RequestReferenceVal = item.RequestRefValues,
                                PaymentSource = GetPaymentSource(item.PaymentSource),
                                PaymentMethod = GetPaymentMethod(item.PaymentMethod),
                                PaymentStatus = (PaymentStatus)item.PaymentStatus,
                                CreatedOn = item.CreatedOn,
                                LastUpdatedOn = item.LastUpdatedOn
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return lstPaymentBO;
        }

        public List<PaymentBO> GetAllPaymentsForReconcileNew(int minutes)
        {
            List<PaymentBO> lstPaymentBO = new List<PaymentBO>();
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    
                    DateTime obj = DateTime.Now.AddMinutes(-minutes);
                    List<tbl_PaymentTransaction> lstMeruPayments = db.tbl_PaymentTransaction.Where(
                       a => a.Payment_Reconciled != true && a.PaymentStatus != (int)PaymentStatus.PaymentSuccess
                           && a.PaymentStatus != (int)PaymentStatus.PaymentSuccessViaLink && a.CreatedOn < obj && a.Payment_Transaction_ID != null).ToList();

                    lstPaymentBO = GetPaymentBOList(lstMeruPayments);
                    if (lstPaymentBO == null)
                    {
                        lstPaymentBO = new List<PaymentBO>();
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return lstPaymentBO;
        }

        private PaymentGatway GetPaymentSource(string Name)
        {
            PaymentGatway objPaymentGateway = PaymentGatway.Unknown;
            if (Name.ToLower() == PaymentGatway.Razorpay.ToString().ToLower())
            {
                objPaymentGateway = PaymentGatway.Razorpay;
            }
            else if (Name.ToLower() == PaymentGatway.PayTM.ToString().ToLower())
            {
                objPaymentGateway = PaymentGatway.PayTM;
            }
            return objPaymentGateway;
        }

        public PaymentMethod GetPaymentMethod(string Name)
        {
            PaymentMethod objPaymentMethod = PaymentMethod.Unknown;
            if (Name.ToLower() == "card")
            {
                objPaymentMethod = PaymentMethod.card;
            }
            else if (Name.ToLower() == "debit")
            {
                objPaymentMethod = PaymentMethod.debit;
            }
            else if (Name.ToLower() == "credit")
            {
                objPaymentMethod = PaymentMethod.credit;
            }
            else if (Name.ToLower() == "netbanking")
            {
                objPaymentMethod = PaymentMethod.netbanking;
            }
            else if (Name.ToLower() == "wallet")
            {
                objPaymentMethod = PaymentMethod.wallet;
            }
            else if (Name.ToLower() == "emi")
            {
                objPaymentMethod = PaymentMethod.emi;
            }
            else if (Name.ToLower() == "upi")
            {
                objPaymentMethod = PaymentMethod.upi;
            }
            return objPaymentMethod;
        }

        #endregion

        public bool TransactionFailed(string MeruPaymentId, string ErrorDetails, PaymentGatway objPaymentGatway)
        {
            bool returnStatus = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans != null)
                    {
                        trans.PaymentSource = (int)objPaymentGatway;
                        trans.PaymentStatus = (int)PaymentStatus.PaymentFailed;
                        trans.PaymentRefData3 = ErrorDetails;
                        trans.LastUpdatedOn = DateTime.Now;
                        db.Entry(trans).State = EntityState.Modified;
                        db.SaveChanges();
                        returnStatus = true;
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return returnStatus;
        }

        public Tuple<string, string, Dictionary<string, string>> FailurePayment(string MeruPaymentId, string ErrorDetails)
        {
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans != null)
                    {
                        trans.PaymentStatus = (int)PaymentStatus.PaymentFailed;
                        trans.PaymentRefData3 = ErrorDetails;
                        trans.LastUpdatedOn = DateTime.Now;
                        db.Entry(trans).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    Dictionary<string, string> returnData = new Dictionary<string, string>();
                    returnData.Add("PaymentType", trans.Payment_Type);

                    return new Tuple<string, string, Dictionary<string, string>>(
                     "200",
                     "Success",
                     returnData);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public bool TransactionResponseUpdate(string MeruPaymentId, string PaymentId, string PaymentRefData, string PaymentResponseDetail, PaymentGatway paymentGateway, PaymentStatus paymentStatus, PaymentMethod paymentMethod)
        {
            bool output = false;

            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
                        return output;
                    }
                    trans.PaymentSource = (int)paymentGateway;
                    trans.PaymentStatus = (int)paymentStatus;

                    trans.PaymentRefValues = PaymentRefData;
                    trans.PaymentRefData2 = PaymentId;
                    trans.PaymentRefData3 = PaymentResponseDetail;

                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    output = true;

                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return output;
        }
        public tbl_PaymentTransaction TransactionResponseAPI_DA_Update(string OrderId, string RazorPaymentID, string RazorPaymentOrderID, string RazorPaymentSignature , string PaymentRefData, string PaymentResponseDetail, PaymentGatway paymentGateway, PaymentStatus paymentStatus, PaymentMethod  paymentMethod,string SPId,string carNo,string purchasedesc,string email)
        {
            string output = "";
            long orderid = Convert.ToInt64(OrderId);
            tbl_PaymentTransaction trans=null ;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    //var trans = db.tbl_PaymentTransaction.Where(a =>  a.RequestRefId == RazorPaymentOrderID).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                     trans = db.tbl_PaymentTransaction.Where(a => a.PaymentIncrementId ==orderid && a.RequestRefId == RazorPaymentOrderID).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", RazorPaymentOrderID));
                        return trans;
                    }
                    trans.PaymentSource = (int)paymentGateway;
                    trans.PaymentStatus = (int)paymentStatus;
                    trans.PaymentMethod = (int)paymentMethod;
                    //trans.RequestRefValues = "{}";
                    trans.RequestRefValues = new JObject { new JProperty("SPId", SPId.ToString()),new JProperty("CarNo", carNo.ToString()) }.ToString(Formatting.None);
                    trans.PaymentRefValues = new JObject { new JProperty("RazorOrderId", RazorPaymentOrderID) }.ToString(Formatting.None);
                    trans.Email = email;
                    trans.PurchaseDescription = purchasedesc;
                    trans.PaymentRefData1 = RazorPaymentOrderID;
                    trans.PaymentRefData2 = RazorPaymentID ;
                    trans.PaymentRefData3 = RazorPaymentSignature;
                    trans.PaymentResponse = PaymentResponseDetail;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    output = (trans.Payment_Amount_Paise*0.01).ToString();
                    //output = trans.Payment_Transaction_ID;

                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return trans;
        }

        public bool TransactionSuccess(string MeruPaymentId, string OrderId, string PaymentId, string PaymentRefValue, PaymentGatway objPaymentGatway)
        {
            bool returnValue = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
                        return returnValue;
                    }
                    trans.PaymentSource = (int)objPaymentGatway;
                    trans.PaymentStatus = (int)PaymentStatus.PaymentSuccess;
                    trans.PaymentRefValues = PaymentRefValue;
                    trans.PaymentRefData2 = PaymentId;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return returnValue;
        }

        //public bool PaymentSuccess(string MeruPaymentId, string OrderId, string PaymentId, string PaymentRefValue, PaymentGatway objPaymentGatway)
        //{
        //    bool returnValue = false;
        //    try
        //    {
        //        using (CDSBusinessEntities db = new CDSBusinessEntities())
        //        {
        //            var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
        //            if (trans == null)
        //            {
        //                objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
        //                return returnValue;
        //            }
        //            trans.PaymentSource = (int)objPaymentGatway;
        //            trans.PaymentStatus = (int)PaymentStatus.PaymentSuccess;
        //            trans.PaymentRefValues = PaymentRefValue;
        //            trans.PaymentRefData2 = PaymentId;
        //            trans.LastUpdatedOn = DateTime.Now;
        //            db.Entry(trans).State = EntityState.Modified;
        //            db.SaveChanges();
        //            returnValue = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.Error(ex);
        //    }

        //    return returnValue;
        //}

        public Tuple<string, string, Dictionary<string, string>> SuccessPayment(string paymentId, string pgPaymentId, PaymentGatway paymentGatway)
        {
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == paymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    JObject objTemp = JObject.Parse(trans.PaymentRefValues);

                    if (!objTemp.ContainsKey("PGPaymentId"))
                    {
                        objTemp.Add(new JProperty("PGPaymentId", pgPaymentId));
                    }
                    else
                    {
                        string temp = Convert.ToString(objTemp["PGPaymentId"]);
                        if (temp != pgPaymentId)
                        {
                            objLogger.Info(string.Format("Different PG Payment ID existing {0} new {1} received for same Payment ID {2}", temp, pgPaymentId, paymentId));
                            return new Tuple<string, string, Dictionary<string, string>>(
                             "500",
                             "failed",
                             null);
                        }
                    }

                    if (trans != null)
                    {
                        trans.PaymentSource = (int)paymentGatway;
                        trans.PaymentStatus = (int)PaymentStatus.PaymentSuccess;
                        trans.PaymentRefValues = objTemp.ToString(Formatting.None);
                        trans.PaymentRefData2 = pgPaymentId;
                        trans.LastUpdatedOn = DateTime.Now;
                        db.Entry(trans).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    Dictionary<string, string> returnData = new Dictionary<string, string>();
                    returnData.Add("PaymentType", trans.Payment_Type);
                    returnData.Add("Amount", trans.Payment_Amount_Paise.ToString());

                    return new Tuple<string, string, Dictionary<string, string>>(
                     "200",
                     "Success",
                     returnData);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        //private bool ValidateData(string MeruPaymentId, )

        public void TransactionPending(string MeruPaymentId, string PaymentId, PaymentGatway objPaymentGatway)
        {
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault(); //Find(Convert.ToInt64(MeruPaymentId));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", MeruPaymentId));
                    }

                    trans.PaymentSource = (int)objPaymentGatway;
                    trans.PaymentStatus = (int)PaymentStatus.PaymentPending;
                    trans.PaymentRefData2 = PaymentId;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
        }

        public bool UpdatePostTransactionSuccess(string mpid, string PaymentDetail, PaymentGatway paymentGatway, PaymentMethod paymentMethod)
        {
            bool returnValue = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var trans = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == mpid).SingleOrDefault(); //Find(Convert.ToInt64(mpid));
                    if (trans == null)
                    {
                        objLogger.Info(string.Format("Unable to find detail for Meru payment Id {0}", mpid));
                        return returnValue;
                    }

                    trans.PaymentSource = (int)paymentGatway;
                    trans.PaymentMethod = (int)paymentMethod;
                    trans.PaymentRefData3 = PaymentDetail;
                    trans.LastUpdatedOn = DateTime.Now;
                    db.Entry(trans).State = EntityState.Modified;
                    db.SaveChanges();
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
        }

        public bool RefundSuccess(string MeruPaymentId, string RefundAmount, string RefundId)
        {
            bool refundUpdated = false;
            try
            {
                //int meruPaymentId = Convert.ToInt32(MeruPaymentId);
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentData = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == MeruPaymentId).SingleOrDefault();

                    if (paymentData != null)
                    {
                        paymentData.Refund_Amount_Paise = Convert.ToInt64(RefundAmount);
                        paymentData.PaymentStatus = (int)PaymentStatus.PaymentRefunded;
                        JObject json = JObject.Parse(paymentData.PaymentRefValues);

                        if (json.ContainsKey("RazorRefundId"))
                        {
                            if (Convert.ToString(json["RazorRefundId"]).Length >= 0)
                            {
                                objLogger.Info(string.Format("Refund Id updated from:{0} to {1} for meru payment id {2}", Convert.ToString(json["RazorRefundId"]), RefundId, MeruPaymentId));
                            }
                            json.Remove("RazorRefundId");
                        }

                        json.Add("RazorRefundId", RefundId);

                        paymentData.PaymentRefValues = json.ToString(Formatting.None); //JsonConvert.SerializeObject(json);

                        paymentData.LastUpdatedOn = DateTime.Now;

                        int i = db.SaveChanges();

                        if (i > 0)
                        {
                            tbl_Payment_History paymentHistory = new tbl_Payment_History();
                            paymentHistory.Payment_Status_Code = "";
                            paymentHistory.Payment_Status_ID = (int)PaymentStatus.PaymentRefunded;
                            paymentHistory.PaymentTransactionId = MeruPaymentId;// meruPaymentId;
                            paymentHistory.Record_Created_DateTime = DateTime.Now;
                            paymentHistory.Updating_Process = "Refund Service";


                            db.tbl_Payment_History.Add(paymentHistory);

                            int j = db.SaveChanges();

                            if (j > 0)
                            {
                                refundUpdated = true;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL class RefundSuccess Exception : " + ex.ToString());
            }
            return refundUpdated;
        }

        public bool PaymentReconcileSuccessOld(long MeruPaymentId)
        {
            bool paymentReconciled = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tblMeruPayments.Where(a => a.PaymentIncrementId == MeruPaymentId).SingleOrDefault();

                    paymentObj.Payment_Reconciled = true;
                    paymentObj.LastUpdatedOn = DateTime.Now;
                    
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        paymentReconciled = true;
                        //objLogger.Info("Reconcilation successful for meru payment id " + MeruPaymentId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL PaymentReconcileSuccess Exception " + ex.ToString());
            }
            return paymentReconciled;
        }

        public bool PaymentReconcileSuccessNew(string PaymentTransactionId)
        {
            bool paymentReconciled = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == PaymentTransactionId).ToList();
                    paymentObj.ForEach(a =>
                    {
                        a.Payment_Reconciled = true;
                        a.LastUpdatedOn = DateTime.Now;
                    });

                    //var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == PaymentTransactionId).SingleOrDefault();

                    //paymentObj.Payment_Reconciled = true;
                    //paymentObj.LastUpdatedOn = DateTime.Now;

                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        paymentReconciled = true;
                        //objLogger.Info("Reconcilation successful for meru payment id " + PaymentTransactionId);
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Payment Transaction Id: " + PaymentTransactionId);
            }
            return paymentReconciled;
        }


        public void updateResponse(string response,string paymenttransactionid)
        {
            
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == paymenttransactionid).ToList();
                    paymentObj.ForEach(a =>
                    {
                        a.PaymentResponse = response;
                        a.LastStatusOn = DateTime.Now;
                       
                    });

                    //var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == PaymentTransactionId).SingleOrDefault();

                    //paymentObj.Payment_Reconciled = true;
                    //paymentObj.LastUpdatedOn = DateTime.Now;

                    int i= db.SaveChanges();
                   
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Payment Transaction Id response: " + paymenttransactionid);
            }
           
        }

        public bool PaymentReconcileStatusUpdateOld(PaymentBO paymentBO)//, PayTMTransactionBO objTranStatusResponse,RazorpayCardBO objRazorpayCardBO)
        {
            bool paymentReconciled = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tblMeruPayments.Where(a => a.PaymentIncrementId == paymentBO.MeruPaymentId).SingleOrDefault();

                    paymentObj.PaymentSource = paymentBO.PaymentSource.ToString(); //PaymentGatway.PayTM.ToString();
                    paymentObj.PaymentRefData2 = paymentBO.PaymentReferenceData2; //objTranStatusResponse.TransactionId;

                    paymentObj.PaymentRefValues = paymentBO.PaymentReferenceValue;// objQ.ToString(Formatting.None);
                    paymentObj.PaymentRefData3 = paymentBO.PaymentReferenceData3;// objOthers.ToString(Formatting.None);
                    paymentObj.PaymentStatus = (int)paymentBO.PaymentStatus;
                    paymentObj.PaymentMethod = paymentBO.PaymentMethod.ToString();
                    paymentObj.LastUpdatedOn = DateTime.Now;
                    db.Entry(paymentObj).State = EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        //tbl_Payment_History paymentHistory = new tbl_Payment_History();
                        //paymentHistory.Payment_Status_Code = "";
                        //paymentHistory.Payment_Status_ID = (int)paymentStatus;
                        //paymentHistory.PaymentTransactionId = paymentObj.PaymentIncrementId.ToString();
                        //paymentHistory.Record_Created_DateTime = DateTime.Now;
                        //paymentHistory.Updating_Process = "Reconcile Service";


                        //db.tbl_Payment_History.Add(paymentHistory);

                        //int j = db.SaveChanges();

                        //if (j > 0)
                        //{
                        paymentReconciled = true;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL PaymentReconcileSuccess Exception " + ex.ToString());
            }
            return paymentReconciled;
        }

        public bool PaymentReconcileStatusUpdateNew(PaymentBO paymentBO)//, PayTMTransactionBO objTranStatusResponse,RazorpayCardBO objRazorpayCardBO)
        {
            bool paymentReconciled = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == paymentBO.PaymentTransactionId).SingleOrDefault();

                    paymentObj.PaymentSource = (int)paymentBO.PaymentSource; //PaymentGatway.PayTM.ToString();
                    paymentObj.PaymentRefData2 = paymentBO.PaymentReferenceData2; //objTranStatusResponse.TransactionId;
                    paymentObj.PaymentRefValues = paymentBO.PaymentReferenceValue;// objQ.ToString(Formatting.None);
                    paymentObj.PaymentRefData3 = paymentBO.PaymentReferenceData3;// objOthers.ToString(Formatting.None);
                    paymentObj.PaymentStatus = (int)paymentBO.PaymentStatus;
                    paymentObj.PaymentMethod = (int)paymentBO.PaymentMethod;
                    paymentObj.LastUpdatedOn = DateTime.Now;
                    db.Entry(paymentObj).State = EntityState.Modified;
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        if (!PaymentReconcileUpdateHistoryNew((int)paymentBO.PaymentStatus, paymentBO.PaymentTransactionId))
                        {
                            objLogger.Info("Update failed in history table for MeruPaymentId:" + paymentBO.PaymentTransactionId);
                        }
                        paymentReconciled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL PaymentReconcileSuccess Exception " + ex.ToString());
            }
            return paymentReconciled;
        }

        public bool PaymentReconcileUpdateHistoryNew(int Payment_Status_ID, string PaymentTransactionId)
        {
            bool paymentReconciledInHistoryTable = false;
            try
            {
                tbl_Payment_History Historydata = new tbl_Payment_History();

                Historydata.Payment_Status_Code = null;
                Historydata.Record_Created_DateTime = DateTime.Now;
                Historydata.Updating_Process = "Reconcile Service";
                Historydata.Payment_Status_ID = Payment_Status_ID;
                Historydata.PaymentTransactionId = PaymentTransactionId;

                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    //Update Histroy table with Creating New Record
                    db.tbl_Payment_History.Add(Historydata);
                    int i = db.SaveChanges();

                    if (i > 0)
                    {
                        paymentReconciledInHistoryTable = true;
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL PaymentReconcileSuccess Exception " + ex.ToString());
            }
            return paymentReconciledInHistoryTable;
        }

        public bool PaymentReconcileStatusUpdate(string PaymentTransactionId, PaymentStatus paymentStatus, string RazorOrderId, PayTMTransactionBO objTranStatusResponse)
        {
            bool paymentReconciled = false;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var paymentObj = db.tbl_PaymentTransaction.Where(a => a.Payment_Transaction_ID == PaymentTransactionId).SingleOrDefault();

                    paymentObj.PaymentSource = (int)PaymentGatway.PayTM;
                    paymentObj.PaymentRefData2 = objTranStatusResponse.TransactionId;
                    if (paymentStatus == PaymentStatus.PaymentSuccess)
                    {
                        JObject objQ = new JObject(
                            new JProperty("RazorOrderId", RazorOrderId),
                            new JProperty("PayTMTransId", objTranStatusResponse.TransactionId),
                            new JProperty("BankTransId", objTranStatusResponse.BankTransactionId),
                            new JProperty("CustomerId", objTranStatusResponse.CustomerId),
                            new JProperty("Amount", objTranStatusResponse.TransactionAmount),
                            new JProperty("GatewayName", objTranStatusResponse.GatewayName),
                            new JProperty("BankName", objTranStatusResponse.BankName),
                            new JProperty("PaymentMode", objTranStatusResponse.PaymentMode)
                        );
                        paymentObj.PaymentRefValues = objQ.ToString(Formatting.None);
                    }
                    else
                    {
                        JObject objOthers = new JObject(
                            new JProperty("Response Code", objTranStatusResponse.ResponseCode),
                            new JProperty("Response Message", objTranStatusResponse.ResponseMessage)
                        );
                        paymentObj.PaymentRefData3 = objOthers.ToString(Formatting.None);
                    }

                    //paymentObj.Payment_Reconciled = true;
                    paymentObj.PaymentStatus = (int)paymentStatus;
                    paymentObj.LastUpdatedOn = DateTime.Now;
                    
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        tbl_Payment_History paymentHistory = new tbl_Payment_History();
                        paymentHistory.Payment_Status_Code = "";
                        paymentHistory.Payment_Status_ID = (int)paymentStatus;
                        paymentHistory.PaymentTransactionId = paymentObj.Payment_Transaction_ID;
                        paymentHistory.Record_Created_DateTime = DateTime.Now;
                        paymentHistory.Updating_Process = "Reconcile Service";

                        db.tbl_Payment_History.Add(paymentHistory);

                        int j = db.SaveChanges();

                        if (j > 0)
                        {
                            paymentReconciled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "PaymentDAL PaymentReconcileSuccess Exception " + ex.ToString());
            }
            return paymentReconciled;
        }

        //public bool SaveCardDetails(RazorpayCardBO objRazorpayCardBO, RazorpayPaymentBO objRazorPaymentBO)
        //{
        //    bool returnStatus = false;

        //    try
        //    {
        //        tbl_Payment_Customer_Card_Details objtbl_Payment_Customer_Card_Details = new tbl_Payment_Customer_Card_Details();
        //        objtbl_Payment_Customer_Card_Details.Card_ID = objRazorpayCardBO.CardId;
        //        objtbl_Payment_Customer_Card_Details.Card_Issuer_Code = objRazorpayCardBO.Issuer;
        //        objtbl_Payment_Customer_Card_Details.Card_Status = 1;// 1 = Active
        //        objtbl_Payment_Customer_Card_Details.Card_Type = objRazorpayCardBO.CardType;
        //        objtbl_Payment_Customer_Card_Details.Is_Default = false;
        //        objtbl_Payment_Customer_Card_Details.Last_Four_Digit = objRazorpayCardBO.Last4;
        //        objtbl_Payment_Customer_Card_Details.Name_On_Card = objRazorpayCardBO.FullName;
        //        objtbl_Payment_Customer_Card_Details.Network_Name = objRazorpayCardBO.Network;
        //        objtbl_Payment_Customer_Card_Details.Provider_Customer_ID = objRazorPaymentBO.CustomerId;
        //        objtbl_Payment_Customer_Card_Details.Token_ID = objRazorPaymentBO.TokenId;
        //        objtbl_Payment_Customer_Card_Details.Record_Created_DateTime = DateTime.Now;
        //        objtbl_Payment_Customer_Card_Details.Record_Update_DateTime = DateTime.Now;

        //        using (CDSBusinessEntities db = new CDSBusinessEntities())
        //        {
        //            if ((from r in db.tbl_Payment_Customer_Card_Details where r.Token_ID == objRazorPaymentBO.TokenId select r).FirstOrDefault() != null)
        //            {
        //                objLogger.Info(string.Format("Token {0} already exist in the table", objRazorPaymentBO.TokenId));
        //                return returnStatus;
        //            }

        //            db.tbl_Payment_Customer_Card_Details.Add(objtbl_Payment_Customer_Card_Details);
        //            db.SaveChanges();
        //            returnStatus = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.Error(ex);
        //    }
        //    return returnStatus;
        //}

        //public RazorpayCustomerBO GetCustomerDetail(string mobile, PaymentGatway paymentGatway, out string returnMessage)
        //{
        //    try
        //    {
        //        returnMessage = "";
        //        int paymentGatewayId = (int)PaymentGatway.Razorpay;
        //        using (CDSBusinessEntities db = new CDSBusinessEntities())
        //        {
        //            var customer = (from r in db.tbl_Payment_Customer_Details
        //                            where r.Customer_Mobile_No == mobile && r.Payment_Gateway_ID == paymentGatewayId
        //                            select r).FirstOrDefault();

        //            if (customer == null)
        //            {
        //                returnMessage = string.Format("Customer with mobile {0} is no in DB.", mobile);
        //                objLogger.Info(returnMessage);
        //                return null;
        //            }

        //            return new RazorpayCustomerBO
        //            {
        //                CustomerEmail = customer.Customer_Email,
        //                CustomerId = customer.Provider_Customer_ID,
        //                CustomerMobile = customer.Customer_Mobile_No,
        //                CustomerName = customer.Customer_Name
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.Error(ex);
        //        returnMessage = ex.Message;
        //        return null;
        //    }
        //}

        //public bool SaveRazorpayCustomer(RazorpayCustomerBO razorpayCustomerBO)
        //{
        //    bool returnStatus = false;
        //    int paymentGateway = (int)PaymentGatway.Razorpay;
        //    try
        //    {
        //        tbl_Payment_Customer_Details objtbl_Payment_Customer_Details = new tbl_Payment_Customer_Details();
        //        objtbl_Payment_Customer_Details.Customer_Email = razorpayCustomerBO.CustomerEmail;
        //        objtbl_Payment_Customer_Details.Customer_Mobile_No = razorpayCustomerBO.CustomerMobile;
        //        objtbl_Payment_Customer_Details.Customer_Name = razorpayCustomerBO.CustomerName;
        //        objtbl_Payment_Customer_Details.Payment_Gateway_ID = (int)PaymentGatway.Razorpay;
        //        objtbl_Payment_Customer_Details.Provider_Customer_ID = razorpayCustomerBO.CustomerId;
        //        objtbl_Payment_Customer_Details.Record_Created_DateTime = DateTime.Now;
        //        objtbl_Payment_Customer_Details.Record_Update_DateTime = DateTime.Now;

        //        using (CDSBusinessEntities db = new CDSBusinessEntities())
        //        {
        //            if ((from r in db.tbl_Payment_Customer_Details
        //                 where r.Customer_Mobile_No == razorpayCustomerBO.CustomerMobile && r.Payment_Gateway_ID == paymentGateway
        //                 select r).FirstOrDefault() != null)
        //            {
        //                objLogger.Info(string.Format("Customer {0} already exist in the table", razorpayCustomerBO.CustomerMobile));
        //                return returnStatus;
        //            }

        //            db.tbl_Payment_Customer_Details.Add(objtbl_Payment_Customer_Details);
        //            db.SaveChanges();
        //            returnStatus = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.Error(ex);
        //    }
        //    return returnStatus;
        //}

        /// <summary>
        /// This method is used to get all the pending events from the DB
        /// </summary>
        /// <returns></returns>
        public List<PaymentBO> GetAllFailedPayments()
        {
            try
            {
                List<PaymentBO> pendingPayments = new List<PaymentBO>();
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    pendingPayments = entities.usp_Wallet_GetPartialAmountTransactionDetails()
                         .Select(rows => new PaymentBO
                         {
                             JobId = rows.JobID,
                             Mobile = rows.MobileNumber,
                             Amount = Convert.ToInt64(rows.Amount * 100),
                             ProviderId = rows.ProviderID,
                             CreatedOn = Convert.ToDateTime(rows.RecordedDateTime),
                             OrderId = rows.OrderId,
                             Email = rows.EmailAddress,
                             PaymentType = rows.AmountType,
                             TripId = rows.TripID,
                             Id = rows.ID
                         }).ToList();
                }
                return pendingPayments;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public PaymentBO GetPaymentDetailsByTripIdAndPaymentType(string paymentType, string tripId)
        {
            PaymentBO dbPaymentDetails = new PaymentBO();

            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    dbPaymentDetails = entities.tbl_PaymentTransaction
                        .Where(x => x.Payment_Type == paymentType && x.RequestRefId == tripId)
                        .Select(x =>
                            new PaymentBO
                            {
                                Amount = x.Payment_Amount_Paise,
                                CreatedOn = x.CreatedOn,
                                DeviceId = x.DeviceId,
                                Email = x.Email,
                                FullName = x.FullName,
                                LastUpdatedOn = x.LastUpdatedOn,
                                PaymentTransactionId = x.Payment_Transaction_ID,
                                Mobile = x.Contact,
                                OrderId = x.PaymentRefData1,
                                //PaymentMethod = x.PaymentMethod,
                                PaymentReferenceData1 = x.PaymentRefData1,
                                PaymentReferenceData2 = x.PaymentRefData2,
                                PaymentReferenceData3 = x.PaymentRefData3,
                                PaymentReferenceValue = x.PaymentRefValues,
                                //PaymentSource = x.PaymentSource,
                                //PaymentStatus = x.PaymentStatus,
                                PaymentType = x.Payment_Type,
                                //PaymentMethodRefId = x.PaymentMethodRefId,
                                PurchaseDesc = x.PurchaseDescription,
                                RefundAmount = x.Refund_Amount_Paise,
                                RequestReferenceVal = x.RequestRefValues,
                                RequestSource = x.RequestSource,
                            }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Eception Occured while fetching Transaction details by Payment type and trip id");
            }
            return dbPaymentDetails;
        }
    }
}