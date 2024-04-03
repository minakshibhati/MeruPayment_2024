using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using MeruPaymentDAL.DAL;
using MeruPaymentBO;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Configuration;
using MeruCommonLibrary;


namespace MeruPaymentBAL
{
    public class ReconcileBAL
    {
        private bool disposed = false;
        private LogHelper objLogger;
        private PaymentDAL objPaymentDAL = null;
        private PaymentRequestSystemDAL objPaymentRequestSystemDAL = null;
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        private RazorpayPaymentBO objRazorpayPaymentBO = null;
        private RazorpayOrderBO objRazorpayOrderBO = null;
        private RazorpayCardBO objRazorpayCardBO = null;
        private PayTMTransactionBO objPayTMTransactionBO = null;
        private string CriticalEmailIds = "";
        private string CriticalEmailSub = "";

        public ReconcileBAL()
        {
            objLogger = new LogHelper("ReconcileBAL");
            objPaymentDAL = new PaymentDAL();
            objPaymentRequestSystemDAL = new PaymentRequestSystemDAL();
            objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();
            CriticalEmailIds = ConfigurationManager.AppSettings["CriticalEmailId"];
            CriticalEmailSub = ConfigurationManager.AppSettings["CriticalEmailSub"];
        }

        ~ReconcileBAL()
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
                    //LogManager.Flush();
                }
                disposed = true;
            }
        }

        private bool NewProcess()
        {
            objLogger.MethodName = "NewProcess()";
            #region Reconciling New Table
            //1
            int minutes = Convert.ToInt32(ConfigurationManager.AppSettings["recon_minutes_old"]);
            var payTmPayments = objPaymentDAL.GetAllPaymentsForReconcileNew(minutes);
            List<string> criticalPaymentIssues = new List<string>();
            List<string> updatedStatus = new List<string>();
            List<string> sameStatus = new List<string>();
            List<string> NoReconcile = new List<string>();

            Paytm objPayTm = null;
            Razorpay objRazorpay = null;
            string meruPaymentId = "";
            try
            {
                //if (payTmPayments == null)
                //{
                //    //objLogger.Info("No data for reconcile in new table");
                //    return true;
                //}
                objLogger.WriteInfo("Total data picked for reconcile from new table is " + payTmPayments.Count.ToString());

                foreach (PaymentBO paymentObject in payTmPayments)
                {
                    meruPaymentId = paymentObject.PaymentTransactionId;
                    try
                    {
                        objRazorpayPaymentBO = null;
                        objPayTMTransactionBO = null;
                        objRazorpayCardBO = null;

                        //objLogger.Info("Processing reconcile for Meru Payment Id : " + meruPaymentId);

                        if (paymentObject.PaymentReferenceData1 == null || paymentObject.PaymentReferenceData1.Length == 0) //|| paymentObject.PaymentSource == PaymentGatway.Unknown)
                        {
                            //2
                            //objLogger.Info("No Order generated at razorpay for PaymentTransactionId : " + paymentObject.PaymentTransactionId);
                            sameStatus.Add(paymentObject.PaymentTransactionId);
                            using (PaymentDAL dal = new PaymentDAL())
                            {
                                //3 - Table reference (Payment_Transaction_ID == PaymentTransactionId)
                                dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                            }
                            continue;
                        }

                        if (paymentObject.PaymentSource == PaymentGatway.Razorpay)
                        {
                            objRazorpay = new Razorpay();
                            objRazorpayPaymentBO = objRazorpay.GetPaymentDetailByOrderId(paymentObject.PaymentReferenceData1);
                            if (objRazorpayPaymentBO == null)
                            {
                                objRazorpayOrderBO = objRazorpay.GetOrderDetail(paymentObject.PaymentReferenceData1);
                                
                               
                                if (objRazorpayOrderBO == null)
                                {
                                    //objLogger.Warn(string.Format("Unable to fetch detail for razorpay order Id: {0}. Meru payment id {1}", paymentObject.PaymentReferenceData1, meruPaymentId));
                                    criticalPaymentIssues.Add(paymentObject.PaymentTransactionId);
                                    using (PaymentDAL dal = new PaymentDAL())
                                    {
                                        //3 - Table reference (Payment_Transaction_ID == PaymentTransactionId)
                                        dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                                    }
                                    continue;
                                }

                                if (objRazorpayOrderBO.Status == OrderStatus.OrderAttempted)
                                {
                                    //objLogger.Info(string.Format("Order is in attemp stage. Order Id: {0}, attemps: {1}", objRazorpayOrderBO.OrderId, objRazorpayOrderBO.Attempts));
                                    NoReconcile.Add(paymentObject.PaymentTransactionId);
                                    continue;
                                }

                                if (objRazorpayOrderBO.Status == OrderStatus.OrderCreated)
                                {
                                    //objLogger.Warn(string.Format("Order is in created stage and not proceed. Order Id: {0}.", objRazorpayOrderBO.OrderId));
                                    //objLogger.Info(string.Format("No payment associated with razorpay for Order Id: {0}. Meru payment {1}", objRazorpayOrderBO.OrderId, meruPaymentId));
                                    //using (PaymentDAL dal = new PaymentDAL())
                                    //{
                                    //4 - Table reference (Payment_Transaction_ID == PaymentTransactionId)
                                    //dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId); 

                                    //}
                                    //criticalPaymentIssues.Add(paymentObject);
                                    //continue;
                                }
                                //TODO: Fill values to object objRazorpayPaymentBO from objRazorpayOrderBO
                            }
                        }

                        if (paymentObject.PaymentSource == PaymentGatway.PayTM || objRazorpayPaymentBO == null)
                        {
                            objPayTm = new Paytm();
                            //5
                            objPayTMTransactionBO = objPayTm.TransactionStatusRequest(paymentObject.PaymentTransactionId);
                            using (PaymentDAL dal = new PaymentDAL())
                            {
                                    dal.updateResponse(JsonConvert.SerializeObject(objPayTMTransactionBO).ToString(), paymentObject.PaymentTransactionId);
                            }


                            if (objPayTMTransactionBO == null || objPayTMTransactionBO.ResponseCode == "334")
                            {
                                //6
                                //objLogger.Info("Not a PayTm Transaction PaymentTransactionId : " + paymentObject.PaymentTransactionId);
                                objPayTMTransactionBO = null;
                            }
                        }

                        // if both RazorPay and PayTM didnt fetched data
                        if (objRazorpayPaymentBO == null && objPayTMTransactionBO == null)
                        {
                            //7
                            //objLogger.Warn("Unable to fetch details from payment gateway PaymentTransactionId : " + paymentObject.PaymentTransactionId);
                            sameStatus.Add(paymentObject.PaymentTransactionId);
                            using (PaymentDAL dal = new PaymentDAL())
                            {
                                //8
                                dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                            }
                            continue;
                        }

                        PaymentStatus payTxnStatus = PaymentStatus.PaymentUnkown;

                        if (objRazorpayPaymentBO != null)
                        {
                            //objLogger.Info("Payment Data from Razorpay : " + JsonConvert.SerializeObject(objRazorpayPaymentBO));

                            if (paymentObject.Amount.ToString() != objRazorpayPaymentBO.Amount)
                            {
                                //9
                                //objLogger.Warn("Critical Payment found PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBAmount : " + paymentObject.Amount.ToString() + "PGAmunt : " + objRazorpayPaymentBO.Amount);
                                criticalPaymentIssues.Add(paymentObject.PaymentTransactionId);
                                using (PaymentDAL dal = new PaymentDAL())
                                {
                                    dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                                }
                                continue;
                            }

                            paymentObject.PaymentReferenceData2 = objRazorpayPaymentBO.PaymentId;
                            paymentObject.PaymentSource = PaymentGatway.Razorpay;
                            payTxnStatus = objRazorpayPaymentBO.PaymentStatus;


                            if (objRazorpayPaymentBO.PaymentStatus == PaymentStatus.PaymentRefunded && objRazorpayPaymentBO.AmountRefunded != null)
                            {
                                paymentObject.RefundAmount = Convert.ToInt64(objRazorpayPaymentBO.AmountRefunded);
                            }



                            //if (payTxnStatus == PaymentStatus.PaymentSuccess)
                            //{
                            JObject objRef = new JObject { 
                                new JProperty("PGOrderId",paymentObject.PaymentReferenceData1),
                                new JProperty("PGPaymentId",objRazorpayPaymentBO.PaymentId)
                            };
                            paymentObject.PaymentReferenceValue = objRef.ToString(Formatting.None);

                            JObject objDetail = new JObject();
                            if (objRazorpayPaymentBO.PaymentMethod == PaymentMethod.card)
                            {
                                objRazorpayCardBO = objRazorpay.GetCardDetail(objRazorpayPaymentBO.PaymentMethodDetail);
                                objDetail.Add(new JProperty("Name", objRazorpayCardBO.FullName));
                                objDetail.Add(new JProperty("Last4", objRazorpayCardBO.Last4));
                                objDetail.Add(new JProperty("Issuer", objRazorpayCardBO.Issuer));
                                objDetail.Add(new JProperty("International", objRazorpayCardBO.IsInternational));
                                objDetail.Add(new JProperty("Emi", objRazorpayCardBO.IsEMI));
                                if (objRazorpayCardBO.CardType == "credit")
                                {
                                    objRazorpayPaymentBO.PaymentMethod = PaymentMethod.credit;
                                }
                                else if (objRazorpayCardBO.CardType == "debit")
                                {
                                    objRazorpayPaymentBO.PaymentMethod = PaymentMethod.debit;
                                }
                            }
                            else
                            {
                                objDetail.Add(new JProperty("Issuer", objRazorpayPaymentBO.PaymentMethodDetail));
                            }
                            paymentObject.PaymentMethod = objRazorpayPaymentBO.PaymentMethod;
                            paymentObject.PaymentReferenceData3 = objDetail.ToString(Formatting.None);
                            //}
                            //else
                            //{
                            if (objRazorpayPaymentBO.ErrorCode.Length > 0)
                            {
                                JObject objOthers = new JObject(
                                    new JProperty("Error Code", objRazorpayPaymentBO.ErrorCode),
                                    new JProperty("Error Description", objRazorpayPaymentBO.ErrorDescription)
                                );
                                paymentObject.PaymentReferenceData3 += objOthers.ToString(Formatting.None);
                            }
                            //}
                        }

                        if (objPayTMTransactionBO != null)
                        {
                            //objLogger.Info("Payment Data from PayTm : " + JsonConvert.SerializeObject(objPayTMTransactionBO, Formatting.None) + " merupaymentid:" + meruPaymentId);

                            if (paymentObject.Amount.ToString() != Convert.ToInt64((Convert.ToDecimal(objPayTMTransactionBO.TransactionAmount) * 100)).ToString())
                            {
                                //10
                                //objLogger.Warn("Critical Payment found PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBAmount : " + paymentObject.Amount.ToString() + "PGAmunt : " + objPayTMTransactionBO.TransactionAmount);
                                criticalPaymentIssues.Add(paymentObject.PaymentTransactionId);
                                using (PaymentDAL dal = new PaymentDAL())
                                {
                                    dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                                }
                                continue;
                            }

                            paymentObject.PaymentReferenceData2 = objPayTMTransactionBO.TransactionId;
                            paymentObject.PaymentSource = PaymentGatway.PayTM;
                            switch (objPayTMTransactionBO.Status)
                            {
                                case "TXN_SUCCESS":
                                    if (objPayTMTransactionBO.ResponseCode == "01")
                                    {
                                        payTxnStatus = PaymentStatus.PaymentSuccess;

                                        if (objPayTMTransactionBO.RefundAmount != null)
                                        {
                                            paymentObject.RefundAmount = Convert.ToInt64(Convert.ToDecimal(objPayTMTransactionBO.RefundAmount) * 100);
                                        }
                                        if (paymentObject.RefundAmount > 0)
                                        {
                                            payTxnStatus = PaymentStatus.PaymentRefunded;
                                        }
                                    }
                                    break;

                                case "TXN_FAILURE":
                                    payTxnStatus = PaymentStatus.PaymentFailed;
                                    break;

                                case "PENDING":
                                    payTxnStatus = PaymentStatus.PaymentPending;
                                    break;
                            }


                            //if (paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
                            //{
                            JObject objQ = new JObject(
                                new JProperty("RazorOrderId", paymentObject.PaymentReferenceData1),
                                new JProperty("PayTMTransId", objPayTMTransactionBO.TransactionId)
                            );

                            JObject objQ1 = new JObject(
                                new JProperty("BankTransId", objPayTMTransactionBO.BankTransactionId),
                                new JProperty("CustomerId", objPayTMTransactionBO.CustomerId),
                                new JProperty("Amount", paymentObject.Amount),
                                new JProperty("GatewayName", objPayTMTransactionBO.GatewayName),
                                new JProperty("BankName", objPayTMTransactionBO.BankName),
                                new JProperty("PaymentMode", objPayTMTransactionBO.PaymentMode)
                            );

                            paymentObject.PaymentReferenceValue = objQ.ToString(Formatting.None);
                            paymentObject.PaymentReferenceData3 = objQ1.ToString(Formatting.None);
                            //}
                            //else
                            //{
                            JObject objOthers = new JObject(
                                new JProperty("Response Code", objPayTMTransactionBO.ResponseCode),
                                new JProperty("Response Message", objPayTMTransactionBO.ResponseMessage)
                            );
                            paymentObject.PaymentReferenceData3 += objOthers.ToString(Formatting.None);
                            //}
                            paymentObject.PaymentMethod = PaymentMethod.wallet;
                        }

                        if (paymentObject.PaymentStatus == payTxnStatus)
                        {
                            //11
                            //objLogger.Info("No status difference for MeruPaymentId: " + paymentObject.PaymentTransactionId);
                            sameStatus.Add(paymentObject.PaymentTransactionId);
                            using (PaymentDAL dal = new PaymentDAL())
                                //12
                                dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                        }
                        else
                        {
                            //if (payTxnStatus != PaymentStatus.PaymentSuccess && paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
                            //{
                            //    //13
                            //    //objLogger.Warn("Critical Payment found PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                            //    criticalPaymentIssues.Add(paymentObject.PaymentTransactionId);
                            //}
                            ////else if (payTxnStatus == PaymentStatus.PaymentRefunded)
                            ////{
                            ////    Implement refund logic
                            ////}
                            //else if (paymentObject.PaymentStatus != payTxnStatus) //(payTxnStatus == PaymentStatus.PaymentSuccess && objPayTMTransactionBO.ResponseCode == "01")
                            //{
                            //14
                            //objLogger.Info("Status mismatch for PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);

                            paymentObject.PaymentStatus = payTxnStatus;

                            updatedStatus.Add(paymentObject.PaymentTransactionId);
                            using (PaymentDAL dal = new PaymentDAL())
                                //15
                                dal.PaymentReconcileStatusUpdateNew(paymentObject);
                            if (payTxnStatus == PaymentStatus.PaymentSuccess)
                            {
                                bool pushed = PushToQueue(paymentObject);
                                if (!pushed)
                                {
                                    //16
                                    objLogger.WriteInfo("Pushing to Queue Failed PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                                }
                                else
                                {
                                    //17
                                    objLogger.WriteInfo("Pushing to Queue Success PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                                }
                            }
                            using (PaymentDAL dal = new PaymentDAL())
                                //18
                                dal.PaymentReconcileSuccessNew(paymentObject.PaymentTransactionId);
                            //}
                            //else if (payTxnStatus == PaymentStatus.PaymentRefunded)
                            //{
                            //    //objLogger.Info("Statuses Mismatched Updating in DB MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTxnStatus);

                            //    //paymentObject.PaymentStatus = payTxnStatus;
                            //    //using (PaymentDAL dal = new PaymentDAL())
                            //    //{
                            //    //    dal.PaymentReconcileStatusUpdateOld(paymentObject);
                            //    //    dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                            //    //}

                            //}
                            //else
                            //{
                            //    objLogger.Info("Critical Payment found MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTxnStatus);
                            //    criticalPaymentIssues.Add(paymentObject);
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        objLogger.WriteError(ex, "");
                    }
                }

                objLogger.WriteInfo(string.Format("Total data reconciled is {0} and meru payment id's are {1}.", sameStatus.Count().ToString(), string.Join(",", sameStatus)));
                objLogger.WriteInfo(string.Format("Total data reconciled and updated is {0} and meru payment id's are {1}.", updatedStatus.Count().ToString(), string.Join(",", updatedStatus)));
                objLogger.WriteWarn(string.Format("Total data reconciled and found critical is {0} and meru payment id's are {1}.", criticalPaymentIssues.Count().ToString(), string.Join(",", criticalPaymentIssues)));
                objLogger.WriteInfo(string.Format("Total data not reconciled is {0} and meru payment id's are {1}.", NoReconcile.Count().ToString(), string.Join(",", NoReconcile)));

                //if (criticalPaymentIssues.Count > 0)
                //{
                //    StringBuilder CriticalMeruPaymentId = new StringBuilder();
                //    foreach (PaymentBO item in criticalPaymentIssues)
                //    {
                //        CriticalMeruPaymentId.Append(string.Format("{0},", item.PaymentTransactionId));
                //    }
                //    objLogger.WriteWarn(string.Format("Unable to recocile Meru Paymnent transaction Id's {0}", CriticalMeruPaymentId.ToString()));

                //    //using (EmailHelper objEmailHelper = new EmailHelper())
                //    //{
                //    //    objEmailHelper.SendNoReplyMail(CriticalEmailIds, CriticalMeruPaymentId.ToString(), CriticalEmailSub);
                //    //}
                //}
            }
            catch (Exception ex1)
            {
                objLogger.WriteError(ex1, "");
            }
            finally
            {
                if (objRazorpay != null)
                    objRazorpay.Dispose();
            }

            #endregion

            //#region Reconciling New Table


            //List<PaymentBO> lstNewPaymentBO = objPaymentDAL.GetAllPaymentsForReconcile(StartDate, EndDate);


            //List<PaymentBO> criticalPaymentIssuesNew = new List<PaymentBO>();
            //var payTmPaymentsNew = lstNewPaymentBO.Where(a => a.PaymentMethod == PaymentMethod.wallet).ToList();



            //foreach (PaymentBO paymentObject in payTmPaymentsNew)
            //{
            //    objLogger.Info("Processing Payment Object from DB New: " + JsonConvert.SerializeObject(paymentObject));
            //    var payTmPaymentData = objPayTm.TransactionStatusRequest(paymentObject.PaymentTransactionId.ToString());

            //    if (payTmPaymentData == null || payTmPaymentData.ResponseCode == "334")
            //    {
            //        objLogger.Info("Not a PayTm Transaction MeruPaymentId : " + paymentObject.MeruPaymentId);
            //        continue;
            //    }
            //    paymentObject.PaymentReferenceData2 = payTmPaymentData.TransactionId;

            //    objLogger.Info("Payment Data from PayTm : " + JsonConvert.SerializeObject(payTmPaymentData));

            //    PaymentStatus payTmStatus = PaymentStatus.PaymentUnkown;

            //    switch (payTmPaymentData.Status)
            //    {

            //        case "TXN_SUCCESS":
            //            payTmStatus = PaymentStatus.PaymentSuccess;
            //            break;

            //        case "TXN_FAILURE":
            //            payTmStatus = PaymentStatus.PaymentFailed;
            //            break;

            //        case "PENDING":
            //            payTmStatus = PaymentStatus.PaymentPending;
            //            break;
            //    }

            //    if (paymentObject.PaymentStatus == payTmStatus)
            //    {
            //        objLogger.Info("Statuses matched Updating in DB New: " + paymentObject.PaymentTransactionId);
            //        PaymentDAL dal = new PaymentDAL();
            //        dal.PaymentReconcileSuccess(paymentObject.PaymentTransactionId);
            //    }
            //    else
            //    {

            //        if (payTmStatus != PaymentStatus.PaymentSuccess && paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
            //        {
            //            objLogger.Info("Critical Payment found PaymentTransactionId :" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //            criticalPaymentIssuesNew.Add(paymentObject);
            //        }
            //        else
            //        {
            //            objLogger.Info("Statuses Mismatched Updating in DB PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //            PaymentDAL dal = new PaymentDAL();
            //            dal.PaymentReconcileStatusUpdate(paymentObject.PaymentTransactionId, payTmStatus, paymentObject.PaymentReferenceData1, payTmPaymentData);

            //            if (payTmStatus == PaymentStatus.PaymentSuccess)
            //            {
            //                bool pushed = PushToQueue(paymentObject);
            //                if (!pushed)
            //                {
            //                    objLogger.Info("Pushing to Queue Failed PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //                }
            //                else
            //                {
            //                    objLogger.Info("Pushing to Queue Success PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //                }
            //                dal.PaymentReconcileSuccess(paymentObject.PaymentTransactionId);
            //            }


            //        }
            //    }
            //}
            //#endregion

            return true;
        }

        public bool Process()
        {
            objLogger.MethodName = "Process()";
            int minutes = Convert.ToInt32(ConfigurationManager.AppSettings["recon_minutes_old"]);
            #region Reconciling Old Table
            var payTmPayments = objPaymentDAL.GetAllPaymentsForReconcileOld(minutes);

            Paytm objPayTm = null;
            Razorpay objRazorpay = null;
            string meruPaymentId = "";

            List<string> criticalPaymentIssues = new List<string>();
            List<string> updatedStatus = new List<string>();
            List<string> sameStatus = new List<string>();
            List<string> NoReconcile = new List<string>();
            try
            {
                objLogger.WriteInfo("Total data picked from old table for reconcile is " + payTmPayments.Count.ToString());

                foreach (PaymentBO paymentObject in payTmPayments)
                {
                    meruPaymentId = paymentObject.MeruPaymentId.ToString();
                    try
                    {
                        objRazorpayPaymentBO = null;
                        objPayTMTransactionBO = null;
                        objRazorpayCardBO = null;

                        //objLogger.Info("Processing reconcile for Meru Payment Id : " + meruPaymentId);

                        if (paymentObject.PaymentReferenceData1 == null || paymentObject.PaymentReferenceData1.Length == 0) //|| paymentObject.PaymentSource == PaymentGatway.Unknown)
                        {
                            //objLogger.Info("No Order generated at razorpay for MeruPaymentId : " + meruPaymentId);
                            sameStatus.Add(paymentObject.MeruPaymentId.ToString());
                            using (PaymentDAL dal = new PaymentDAL())
                            {
                                dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                            }
                            continue;
                        }

                        if (paymentObject.PaymentSource == PaymentGatway.Razorpay)
                        {
                            objRazorpay = new Razorpay();
                            objRazorpayPaymentBO = objRazorpay.GetPaymentDetailByOrderId(paymentObject.PaymentReferenceData1);
                            if (objRazorpayPaymentBO == null)
                            {
                                objRazorpayOrderBO = objRazorpay.GetOrderDetail(paymentObject.PaymentReferenceData1);
                                if (objRazorpayOrderBO == null)
                                {
                                    //objLogger.Warn(string.Format("Unable to fetch detail for razorpay order Id: {0}. Meru payment id {1}", paymentObject.PaymentReferenceData1, meruPaymentId));
                                    criticalPaymentIssues.Add(paymentObject.MeruPaymentId.ToString());
                                    using (PaymentDAL dal = new PaymentDAL())
                                    {
                                        dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                                    }
                                    continue;
                                }

                                if (objRazorpayOrderBO.Status == OrderStatus.OrderAttempted)
                                {
                                    //objLogger.Info(string.Format("Order is in attemp stage. Order Id: {0}, attemps: {1}", objRazorpayOrderBO.OrderId, objRazorpayOrderBO.Attempts));
                                    //criticalPaymentIssues.Add(paymentObject);
                                    NoReconcile.Add(paymentObject.MeruPaymentId.ToString());
                                    continue;
                                }

                                if (objRazorpayOrderBO.Status == OrderStatus.OrderCreated)
                                {
                                    //objLogger.Info(string.Format("No payment associated with razorpay for Order Id: {0}. Meru payment {1}", objRazorpayOrderBO.OrderId, meruPaymentId));
                                    //continue;
                                }
                            }
                        }

                        if (paymentObject.PaymentSource == PaymentGatway.PayTM || objRazorpayPaymentBO == null)
                        {
                            objPayTm = new Paytm();
                            objPayTMTransactionBO = objPayTm.TransactionStatusRequest(paymentObject.MeruPaymentId.ToString());
                            if (objPayTMTransactionBO == null || objPayTMTransactionBO.ResponseCode == "334")
                            {
                                //objLogger.Info("Not a PayTm Transaction MeruPaymentId : " + paymentObject.MeruPaymentId);
                                objPayTMTransactionBO = null;
                            }
                        }

                        // if both RazorPay and PayTM didnt fetched data
                        if (objRazorpayPaymentBO == null && objPayTMTransactionBO == null)
                        {
                            //objLogger.Warn("Might be a cancelled transaction. Unable to fetch payment details from PG for MeruPaymentId : " + paymentObject.MeruPaymentId);
                            sameStatus.Add(paymentObject.MeruPaymentId.ToString());
                            using (PaymentDAL dal = new PaymentDAL())
                            {
                                dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                            }
                            continue;
                        }

                        PaymentStatus payTxnStatus = PaymentStatus.PaymentUnkown;

                        if (objRazorpayPaymentBO != null)
                        {
                            //objLogger.Info("Payment Data from Razorpay : " + JsonConvert.SerializeObject(objRazorpayPaymentBO, Formatting.None) + " Meru paymentid:" + meruPaymentId);

                            if (paymentObject.Amount.ToString() != objRazorpayPaymentBO.Amount)
                            {
                                //objLogger.Warn("Critical Payment found MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBAmount : " + paymentObject.Amount.ToString() + "PGAmunt : " + objRazorpayPaymentBO.Amount);
                                criticalPaymentIssues.Add(paymentObject.MeruPaymentId.ToString());
                                using (PaymentDAL dal = new PaymentDAL())
                                {
                                    dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                                }
                                continue;
                            }

                            paymentObject.PaymentReferenceData2 = objRazorpayPaymentBO.PaymentId;
                            paymentObject.PaymentSource = PaymentGatway.Razorpay;
                            payTxnStatus = objRazorpayPaymentBO.PaymentStatus;

                            if (objRazorpayPaymentBO.PaymentStatus == PaymentStatus.PaymentRefunded && objRazorpayPaymentBO.AmountRefunded != null)
                            {
                                paymentObject.RefundAmount = Convert.ToInt64(objRazorpayPaymentBO.AmountRefunded);
                            }


                            //if (payTxnStatus == PaymentStatus.PaymentSuccess)
                            //{
                            JObject objRef = new JObject { 
                                new JProperty("PGOrderId",paymentObject.PaymentReferenceData1),
                                new JProperty("PGPaymentId",objRazorpayPaymentBO.PaymentId)
                            };
                            paymentObject.PaymentReferenceValue = objRef.ToString(Formatting.None);

                            JObject objDetail = new JObject();
                            if (objRazorpayPaymentBO.PaymentMethod == PaymentMethod.card)
                            {
                                objRazorpayCardBO = objRazorpay.GetCardDetail(objRazorpayPaymentBO.PaymentMethodDetail);
                                objDetail.Add(new JProperty("Name", objRazorpayCardBO.FullName));
                                objDetail.Add(new JProperty("Last4", objRazorpayCardBO.Last4));
                                objDetail.Add(new JProperty("Issuer", objRazorpayCardBO.Issuer));
                                objDetail.Add(new JProperty("International", objRazorpayCardBO.IsInternational));
                                objDetail.Add(new JProperty("Emi", objRazorpayCardBO.IsEMI));
                                if (objRazorpayCardBO.CardType == "credit")
                                {
                                    objRazorpayPaymentBO.PaymentMethod = PaymentMethod.credit;
                                }
                                else if (objRazorpayCardBO.CardType == "debit")
                                {
                                    objRazorpayPaymentBO.PaymentMethod = PaymentMethod.debit;
                                }
                            }
                            else
                            {
                                objDetail.Add(new JProperty("Issuer", objRazorpayPaymentBO.PaymentMethodDetail));
                            }
                            paymentObject.PaymentMethod = objRazorpayPaymentBO.PaymentMethod;
                            paymentObject.PaymentReferenceData3 = objDetail.ToString(Formatting.None);
                            //}
                            //else
                            //{
                            if (objRazorpayPaymentBO.ErrorCode.Length > 0)
                            {
                                JObject objOthers = new JObject(
                                    new JProperty("Error Code", objRazorpayPaymentBO.ErrorCode),
                                    new JProperty("Error Description", objRazorpayPaymentBO.ErrorDescription)
                                );
                                paymentObject.PaymentReferenceData3 += objOthers.ToString(Formatting.None);
                            }
                            //}
                        }

                        if (objPayTMTransactionBO != null)
                        {
                            //objLogger.Info("Payment Data from PayTm : " + JsonConvert.SerializeObject(objPayTMTransactionBO, Formatting.None) + " merupaymentid:" + meruPaymentId);

                            if (paymentObject.Amount.ToString() != Convert.ToInt64((Convert.ToDecimal(objPayTMTransactionBO.TransactionAmount) * 100)).ToString())
                            {
                                //objLogger.Warn("Critical Payment found MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBAmount : " + paymentObject.Amount.ToString() + "PGAmunt : " + objPayTMTransactionBO.TransactionAmount);
                                criticalPaymentIssues.Add(paymentObject.MeruPaymentId.ToString());
                                using (PaymentDAL dal = new PaymentDAL())
                                {
                                    dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                                }
                                continue;
                            }

                            paymentObject.PaymentReferenceData2 = objPayTMTransactionBO.TransactionId;
                            paymentObject.PaymentSource = PaymentGatway.PayTM;
                            switch (objPayTMTransactionBO.Status)
                            {
                                case "TXN_SUCCESS":
                                    if (objPayTMTransactionBO.ResponseCode == "01")
                                    {
                                        payTxnStatus = PaymentStatus.PaymentSuccess;
                                        if (objPayTMTransactionBO.RefundAmount !=null)
                                        {
                                            paymentObject.RefundAmount = Convert.ToInt64(Convert.ToDecimal(objPayTMTransactionBO.RefundAmount) * 100);
                                            
                                        }
                                        if (paymentObject.RefundAmount > 0)
                                        {
                                            payTxnStatus = PaymentStatus.PaymentRefunded;
                                        }
                                    }
                                    //TODO: REFUND
                                    break;

                                case "TXN_FAILURE":
                                    payTxnStatus = PaymentStatus.PaymentFailed;
                                    //TODO: REFUND
                                    break;

                                case "PENDING":
                                    payTxnStatus = PaymentStatus.PaymentPending;
                                    break;
                            }


                            //if (paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
                            //{
                            JObject objQ = new JObject(
                                new JProperty("RazorOrderId", paymentObject.PaymentReferenceData1),
                                new JProperty("PayTMTransId", objPayTMTransactionBO.TransactionId)
                            );

                            JObject objQ1 = new JObject(
                                new JProperty("BankTransId", objPayTMTransactionBO.BankTransactionId),
                                new JProperty("CustomerId", objPayTMTransactionBO.CustomerId),
                                new JProperty("Amount", paymentObject.Amount),
                                new JProperty("GatewayName", objPayTMTransactionBO.GatewayName),
                                new JProperty("BankName", objPayTMTransactionBO.BankName),
                                new JProperty("PaymentMode", objPayTMTransactionBO.PaymentMode)
                            );

                            paymentObject.PaymentReferenceValue = objQ.ToString(Formatting.None);
                            paymentObject.PaymentReferenceData3 = objQ1.ToString(Formatting.None);
                            //}
                            //else
                            //{
                            JObject objOthers = new JObject(
                                new JProperty("Response Code", objPayTMTransactionBO.ResponseCode),
                                new JProperty("Response Message", objPayTMTransactionBO.ResponseMessage)
                            );
                            paymentObject.PaymentReferenceData3 += objOthers.ToString(Formatting.None);
                            //}
                            paymentObject.PaymentMethod = PaymentMethod.wallet;
                        }

                        if (paymentObject.PaymentStatus == payTxnStatus)
                        {
                            //objLogger.Info("No status difference for MeruPaymentId: " + paymentObject.MeruPaymentId);
                            sameStatus.Add(paymentObject.MeruPaymentId.ToString());
                            using (PaymentDAL dal = new PaymentDAL())
                                dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                        }
                        else
                        {
                            //if (payTxnStatus != PaymentStatus.PaymentSuccess && paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
                            //{
                            //    //objLogger.Warn("Critical Payment found MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                            //    criticalPaymentIssues.Add(paymentObject.MeruPaymentId.ToString());
                            //}
                            //else if (payTxnStatus == PaymentStatus.PaymentRefunded)
                            //{
                            //    Implement refund logic
                            //}
                            //else 
                            //    if (paymentObject.PaymentStatus != payTxnStatus) //(payTxnStatus == PaymentStatus.PaymentSuccess && objPayTMTransactionBO.ResponseCode == "01")
                            //{
                            //objLogger.WriteInfo("Status mismatched. for MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);

                            paymentObject.PaymentStatus = payTxnStatus;

                            updatedStatus.Add(paymentObject.MeruPaymentId.ToString());
                            using (PaymentDAL dal = new PaymentDAL())
                                dal.PaymentReconcileStatusUpdateOld(paymentObject);

                            if (payTxnStatus == PaymentStatus.PaymentSuccess)
                            {
                                bool pushed = PushToQueue(paymentObject);
                                if (!pushed)
                                {
                                    objLogger.WriteInfo("Pushing to Queue Failed MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                                }
                                else
                                {
                                    objLogger.WriteInfo("Pushing to Queue Success MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PGPaymentStatus : " + payTxnStatus);
                                }
                            }
                            using (PaymentDAL dal = new PaymentDAL())
                                dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                            //}
                            //else if (payTxnStatus == PaymentStatus.PaymentRefunded)
                            //{
                            //    //objLogger.Info("Statuses Mismatched Updating in DB MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTxnStatus);

                            //    //paymentObject.PaymentStatus = payTxnStatus;
                            //    //using (PaymentDAL dal = new PaymentDAL())
                            //    //{
                            //    //    dal.PaymentReconcileStatusUpdateOld(paymentObject);
                            //    //    dal.PaymentReconcileSuccessOld(paymentObject.MeruPaymentId);
                            //    //}

                            //}
                            //else
                            //{
                            //    objLogger.Info("Critical Payment found MeruPaymentId:" + paymentObject.MeruPaymentId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTxnStatus);
                            //    criticalPaymentIssues.Add(paymentObject);
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        objLogger.WriteError(ex, "");
                    }
                }

                objLogger.WriteInfo(string.Format("Total data reconciled from old table is {0} and meru payment id's are {1}.", sameStatus.Count().ToString(), string.Join(",", sameStatus)));
                objLogger.WriteInfo(string.Format("Total data reconciled and updated from old table is {0} and meru payment id's are {1}.", updatedStatus.Count().ToString(), string.Join(",", updatedStatus)));
                objLogger.WriteWarn(string.Format("Total data reconciled and found critical from old table is {0} and meru payment id's are {1}.", criticalPaymentIssues.Count().ToString(), string.Join(",", criticalPaymentIssues)));
                objLogger.WriteInfo(string.Format("Total data not reconciled from old table is {0} and meru payment id's are {1}.", NoReconcile.Count().ToString(), string.Join(",", NoReconcile)));

                NewProcess();
                //if (criticalPaymentIssues.Count > 0)
                //{
                //    StringBuilder CriticalMeruPaymentId = new StringBuilder();
                //    foreach (PaymentBO item in criticalPaymentIssues)
                //    {
                //        CriticalMeruPaymentId.Append(string.Format("{0},", item.MeruPaymentId));
                //    }
                //    objLogger.WriteWarn(string.Format("Unable to recocile Meru Paymnent Id's {0}", CriticalMeruPaymentId.ToString()));
                //}
            }
            catch (Exception ex1)
            {
                objLogger.WriteError(ex1, "");
            }
            finally
            {
                if (objRazorpay != null)
                    objRazorpay.Dispose();
                objLogger.WriteInfo("Meru Payment Reconciliation Service completed");
            }

            #endregion

            //#region Reconciling New Table


            //List<PaymentBO> lstNewPaymentBO = objPaymentDAL.GetAllPaymentsForReconcile(StartDate, EndDate);


            //List<PaymentBO> criticalPaymentIssuesNew = new List<PaymentBO>();
            //var payTmPaymentsNew = lstNewPaymentBO.Where(a => a.PaymentMethod == PaymentMethod.wallet).ToList();



            //foreach (PaymentBO paymentObject in payTmPaymentsNew)
            //{
            //    objLogger.Info("Processing Payment Object from DB New: " + JsonConvert.SerializeObject(paymentObject));
            //    var payTmPaymentData = objPayTm.TransactionStatusRequest(paymentObject.PaymentTransactionId.ToString());

            //    if (payTmPaymentData == null || payTmPaymentData.ResponseCode == "334")
            //    {
            //        objLogger.Info("Not a PayTm Transaction MeruPaymentId : " + paymentObject.MeruPaymentId);
            //        continue;
            //    }
            //    paymentObject.PaymentReferenceData2 = payTmPaymentData.TransactionId;

            //    objLogger.Info("Payment Data from PayTm : " + JsonConvert.SerializeObject(payTmPaymentData));

            //    PaymentStatus payTmStatus = PaymentStatus.PaymentUnkown;

            //    switch (payTmPaymentData.Status)
            //    {

            //        case "TXN_SUCCESS":
            //            payTmStatus = PaymentStatus.PaymentSuccess;
            //            break;

            //        case "TXN_FAILURE":
            //            payTmStatus = PaymentStatus.PaymentFailed;
            //            break;

            //        case "PENDING":
            //            payTmStatus = PaymentStatus.PaymentPending;
            //            break;
            //    }

            //    if (paymentObject.PaymentStatus == payTmStatus)
            //    {
            //        objLogger.Info("Statuses matched Updating in DB New: " + paymentObject.PaymentTransactionId);
            //        PaymentDAL dal = new PaymentDAL();
            //        dal.PaymentReconcileSuccess(paymentObject.PaymentTransactionId);
            //    }
            //    else
            //    {

            //        if (payTmStatus != PaymentStatus.PaymentSuccess && paymentObject.PaymentStatus == PaymentStatus.PaymentSuccess)
            //        {
            //            objLogger.Info("Critical Payment found PaymentTransactionId :" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //            criticalPaymentIssuesNew.Add(paymentObject);
            //        }
            //        else
            //        {
            //            objLogger.Info("Statuses Mismatched Updating in DB PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //            PaymentDAL dal = new PaymentDAL();
            //            dal.PaymentReconcileStatusUpdate(paymentObject.PaymentTransactionId, payTmStatus, paymentObject.PaymentReferenceData1, payTmPaymentData);

            //            if (payTmStatus == PaymentStatus.PaymentSuccess)
            //            {
            //                bool pushed = PushToQueue(paymentObject);
            //                if (!pushed)
            //                {
            //                    objLogger.Info("Pushing to Queue Failed PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //                }
            //                else
            //                {
            //                    objLogger.Info("Pushing to Queue Success PaymentTransactionId:" + paymentObject.PaymentTransactionId + " : DBPaymentStatus : " + paymentObject.PaymentStatus + "PayTmPaymentStatus : " + payTmStatus);
            //                }
            //                dal.PaymentReconcileSuccess(paymentObject.PaymentTransactionId);
            //            }


            //        }
            //    }
            //}
            //#endregion

            return true;
        }

        public bool PushToQueue(PaymentBO objPaymentBO)
        {
            objLogger.MethodName = "PushToQueue(PaymentBO objPaymentBO)";

            objPaymentRequestSystemMasterBO = objPaymentRequestSystemDAL.GetDetailBySystemCode(objPaymentBO.RequestSource);
            if (objPaymentRequestSystemMasterBO == null)
            {
                objLogger.WriteInfo(string.Format("Unable to get source detail for {0}", objPaymentBO.RequestSource));
                return false;
            }
            if (objPaymentRequestSystemMasterBO.QueueName != null && objPaymentRequestSystemMasterBO.QueueName.Length > 0)
            {
                JObject objQ = new JObject(
                    new JProperty("MeruPaymentId", objPaymentBO.PaymentTransactionId.Length == 0 ? objPaymentBO.MeruPaymentId.ToString() : objPaymentBO.PaymentTransactionId),
                    new JProperty("Amount", objPaymentBO.Amount),
                    new JProperty("PaymentMethod", objPaymentBO.PaymentMethod.ToString()),
                    new JProperty("PaymentSource", objPaymentBO.PaymentSource.ToString()),
                    new JProperty("PaymentId", objPaymentBO.PaymentReferenceData2)
                    );
                Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(objPaymentBO.RequestReferenceVal);
                foreach (KeyValuePair<string, string> item in obj.ToList<KeyValuePair<string, string>>())
                {
                    objQ.Add(item.Key, item.Value);
                }

                CommonMethods objCommonMethods = new CommonMethods();
                return objCommonMethods.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, objQ.ToString(Formatting.None));
            }
            return true;
        }
    }
}
