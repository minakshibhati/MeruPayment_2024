using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruCommonLibrary;
using MeruPaymentDAL.DAL;
using MeruPaymentBO;
using MeruPaymentCore;
using System.Configuration;

namespace MeruPaymentBAL
{
    public class PreChargeValidation
    {
        LogHelper logHelper = null;
        public PreChargeValidation()
        {
            logHelper = new LogHelper("PreChargeValidation");
        }

        public Tuple<string, string, bool> Validate(string PaymentType, string AppRequestId, long Amount, int PaymentMethodRefId, string Mobile)
        {
            logHelper.MethodName = "Validate";
            try
            {
                Tuple<string, string, bool> returnDuplicateTransactionValidation = DuplicateTransactionValidation(PaymentType, AppRequestId, Amount);
                if (returnDuplicateTransactionValidation.Item1 != "200")
                {
                    return returnDuplicateTransactionValidation;
                }

                Tuple<string, string, bool> returnPaymentMethod_DailyTransactionLimit_Validation =
                    PaymentMethod_DailyTransactionLimit_Validation(PaymentMethodRefId, Mobile);
                if (returnPaymentMethod_DailyTransactionLimit_Validation.Item1 != "200")
                {
                    return returnPaymentMethod_DailyTransactionLimit_Validation;
                }

                return new Tuple<string, string, bool>("200", "Success", true);
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "Error occured while validation transaction request before sending to PG.");
                return new Tuple<string, string, bool>("500", "Failed", false);
            }
        }

        private Tuple<string, string, bool> PaymentMethod_DailyTransactionLimit_Validation(int PaymentMethodRefId, string Mobile)
        {
            logHelper.MethodName = "PaymentMethod_DailyTransactionLimit_Validation";

            int DailyTransactionCountLimit = 0;
            DailyTransactionCountLimit = Convert.ToInt32(ConfigurationManager.AppSettings["DailyTransactionCountLimit"]);

            int DailyTransactionAmountLimit = 0;
            DailyTransactionAmountLimit = Convert.ToInt32(ConfigurationManager.AppSettings["DailyTransactionAmountLimit"]);

            int DailyTransactionAmountLimitOverall = 0;
            DailyTransactionAmountLimitOverall = Convert.ToInt32(ConfigurationManager.AppSettings["DailyTransactionAmountLimitOverall"]);
            try
            {
                DateTime TodaysDate = DateTime.Today;

                PaymentDAL paymentDAL = new PaymentDAL();
                List<PaymentBO> lstPaymentBO = paymentDAL.GetMeruPaymentDetail_ByPaymentMethodRefId_Contact_Date(PaymentMethodRefId, Mobile, TodaysDate);
                List<PaymentBO> lstPaymentBOAll = paymentDAL.GetMeruPaymentDetail_Date(null, DateTime.Now);

                lstPaymentBO = lstPaymentBO.Where(r => r.PaymentStatus == PaymentStatus.PaymentSuccess).ToList();

                if (lstPaymentBO == null || lstPaymentBO.Count == 0)
                {
                    return new Tuple<string, string, bool>("200", "Success", true);
                }

                if (lstPaymentBO.Count >= DailyTransactionCountLimit)
                {
                    logHelper.WriteFatal(string.Format("Daily transaction limit of {0} for selected payment method {1} is exhausted", DailyTransactionCountLimit, PaymentMethodRefId));
                    return new Tuple<string, string, bool>("500", "Failed", true);
                }

                long TotalSumAmount = lstPaymentBOAll.Sum(r => r.Amount);

                if (TotalSumAmount >= DailyTransactionAmountLimitOverall)
                {
                    logHelper.WriteFatal("Daily overall transaction amount limit exhausted");
                    return new Tuple<string, string, bool>("500", "Failed", true);
                }

                long SumAmount = lstPaymentBO.Sum(r => r.Amount);

                if (SumAmount >= DailyTransactionAmountLimit)
                {
                    logHelper.WriteFatal(string.Format("Daily transaction amount limit of {0} for selected payment method {1} is exhausted", DailyTransactionAmountLimit, PaymentMethodRefId));
                    return new Tuple<string, string, bool>("500", "Failed", true);
                }

                return new Tuple<string, string, bool>("200", "Success", true);
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "Error occured while validation transaction request before sending to PG.");
                return new Tuple<string, string, bool>("500", "Failed", false);
            }
        }

        private Tuple<string, string, bool> DuplicateTransactionValidation(string PaymentType, string AppRequestId, long Amount)
        {
            logHelper.MethodName = "DuplicateTransactionValidation";
            string repeatAttemptMsg = "Repeated attempt to debit for payment type {0} app request Id {1} and amount {2}";
            repeatAttemptMsg = string.Format(repeatAttemptMsg, PaymentType, AppRequestId, Amount);

            try
            {
                PaymentDAL paymentDAL = new PaymentDAL();
                List<PaymentBO> lstPaymentBO = paymentDAL.GetMeruPaymentDetail_ByAppRequestId(AppRequestId);

                var result = lstPaymentBO.Where(r => r.PaymentType == PaymentType && r.Amount == Amount).LastOrDefault();

                if (result == null || result.PaymentStatus == PaymentStatus.PaymentFailed)
                {
                    return new Tuple<string, string, bool>("200", "Success", true);
                }

                if (result.PaymentStatus == PaymentStatus.PaymentSuccess)
                {
                    logHelper.WriteFatal(repeatAttemptMsg);
                    return new Tuple<string, string, bool>("500", repeatAttemptMsg, true);
                }

                if (result.PaymentStatus != PaymentStatus.PaymentSuccess && Convert.ToString(result.PaymentReferenceData1).Length > 0)
                {
                    Razorpay razorpay = new Razorpay();
                    RazorpayPaymentBO razorpayPaymentBO = razorpay.GetPaymentDetailByOrderId(result.PaymentReferenceData1);
                    if (razorpayPaymentBO == null)
                    {
                        //return new Tuple<string, string, bool>("200", "Success", true);
                        logHelper.WriteFatal("Unable to fetch payment or order detail from razorpay. Order Id: " + result.PaymentReferenceData1);
                        return new Tuple<string, string, bool>("500", "Failed", true);
                    }

                    if (razorpayPaymentBO.PaymentStatus == PaymentStatus.PaymentSuccess)
                    {
                        paymentDAL.SuccessPayment(result.PaymentTransactionId, razorpayPaymentBO.PaymentId, PaymentGatway.Razorpay);
                        logHelper.WriteFatal(repeatAttemptMsg);
                        return new Tuple<string, string, bool>("500", repeatAttemptMsg, true);
                    }

                    if (razorpayPaymentBO.PaymentStatus != result.PaymentStatus && razorpayPaymentBO.PaymentStatus == PaymentStatus.PaymentFailed)
                    {
                        //paymentDAL.FailurePayment(result.PaymentTransactionId, razorpayPaymentBO.ErrorDescription);
                        FailurePayment failurePayment = new FailurePayment();
                        failurePayment.ProcessRequest(result.PaymentTransactionId, razorpayPaymentBO.ErrorCode, razorpayPaymentBO.ErrorDescription);
                        return new Tuple<string, string, bool>("200", "Success", true);
                    }
                }

                logHelper.WriteFatal(string.Format("Pre charge validation unknown condition. payment type {0} app request Id {1} and amount {2}", PaymentType, AppRequestId, Amount));
                return new Tuple<string, string, bool>("500", "Failed", true);
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "Error occured while validation transaction request before sending to PG.");
                return new Tuple<string, string, bool>("500", "Failed", false);
            }
        }
    }
}
