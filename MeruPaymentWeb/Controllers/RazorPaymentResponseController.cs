using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MeruPaymentWeb.Controllers
{
    public class RazorPaymentResponseController : Controller
    {
        // GET: RazorPaymentResponse
        public ActionResult Index(FormCollection collection)
        {
            StringBuilder LogData = new StringBuilder();
            LogHelper objLogger = new LogHelper("RazorPaymentResponseController");
            RazorCheckoutResponseBAL objRazorCheckoutResponseBAL = new RazorCheckoutResponseBAL();
            string returnURL = "", successMsg = "";
            string mpid = Convert.ToString(Request["mpid"]);

            try
            {
                objLogger.MethodName = "Index(FormCollection collection)";

                #region READ RESPONSE VALUE
               
                LogData.Append("Meru Payment Id: " + mpid + " ");

                string msg = Convert.ToString(Request["msg"]);
                LogData.Append("Message: " + msg + " ");

                string errorCode = Request["error[code]"];
                LogData.Append("Error Code: " + errorCode + " ");

                string errorDescription = Request["error[description]"];
                LogData.Append("Error Desc: " + errorDescription + " ");

                string RazorPaymentId = Convert.ToString(Request["razorpay_payment_id"]);
                LogData.Append("Razorpay PaymentId: " + RazorPaymentId + " ");

                string RazorOrderId = Convert.ToString(Request["razorpay_order_id"]);
                LogData.Append("Razorpay order id: " + RazorOrderId + " ");

                string RazorSignature = Convert.ToString(Request["razorpay_signature"]);
                LogData.Append("Razorpay signature: " + RazorSignature + " ");

                objLogger.WriteInfo("Razorpay checkout response " + LogData.ToString());

                #endregion

                #region READ PAYMENT DETAIL FROM DB

                PaymentBO objPaymentBO = objRazorCheckoutResponseBAL.GetMeruPaymentDetail(mpid);
                if (objPaymentBO == null)
                {
                    ViewBag.Message = "Invalid Data.";
                    objLogger.WriteInfo(string.Format("Payment data is not present for Meru payment Id {0}", mpid));
                    return View();
                }

                string AmountInDB = objPaymentBO.Amount.ToString();
                string OrderIdInDB = objPaymentBO.PaymentReferenceData1;

                #endregion

                #region GET SOURCE DETAIL

                PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = objRazorCheckoutResponseBAL.GetSourceDetail(objPaymentBO.RequestSource);
                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.WriteInfo(string.Format("Source is either unknown or null. Source: {0}", LogData.ToString()));
                    ViewBag.Message = "Invalid Data.";
                    return View();
                }
                returnURL = objPaymentRequestSystemMasterBO.ReturnURL;

                #endregion

                #region UPDATE CANCELLED STATUS

                //TODO: Encrypt msg
                if (msg != null && msg.Length > 0 && msg.ToLower().Trim() == "cancelled")
                {
                    objLogger.WriteInfo(string.Format("Payment Cancelled {0}", LogData.ToString()));
                    objRazorCheckoutResponseBAL.UpdatePaymentCancelledStatus(mpid);
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "cancelled?message=" + string.Format(ConfigurationManager.AppSettings["cancelpaymsg"], mpid));
                    return Redirect(returnURL + "cancelled?message=" + string.Format(ConfigurationManager.AppSettings["cancelpaymsg"], mpid));
                }

                #endregion

                #region UPDATE FAILED STATUS

                if (errorCode != null && errorCode.Length > 0)
                {
                    objLogger.WriteInfo(string.Format("Payment Failed {0}", LogData.ToString()));
                    objRazorCheckoutResponseBAL.UpdatePaymentFailedStatus(mpid, errorCode, errorDescription);
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], mpid, errorDescription));
                    return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], mpid, errorDescription));
                }

                #endregion

                #region VALIDATE SIGNATURE

                bool isValidSignature = false;
                bool isValidTrasaction = false;
                if (RazorPaymentId == null || RazorPaymentId.Length == 0)
                {
                    objLogger.WriteInfo(string.Format("Payment Id not receied from Razorpay {0}", LogData.ToString()));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                }

                isValidSignature = objRazorCheckoutResponseBAL.ValidateResponseSignature(RazorSignature, RazorOrderId + "|" + RazorPaymentId, ConfigurationManager.AppSettings["Razor_Key_Secret"]);

                if (!isValidSignature)
                {
                    objLogger.WriteInfo(string.Format("Razorpay response signature mismatch {0}", LogData.ToString()));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                }

                isValidTrasaction = objRazorCheckoutResponseBAL.ValidateTransactionResponse(RazorPaymentId, objPaymentBO.PaymentReferenceData1, objPaymentBO.Amount);

                if (!isValidTrasaction)
                {
                    objLogger.WriteWarn(string.Format("Transaction data mismatch {0}", LogData.ToString()));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                }

                #endregion

                #region UPDATE PENDING STATUS

                if (RazorPaymentId != null && RazorPaymentId.Length > 0)
                {
                    if (!isValidSignature)
                    {
                        objLogger.WriteInfo(string.Format("Razorpay checkout reponse signature validation failed. {0}", LogData.ToString()));
                        objRazorCheckoutResponseBAL.UpdatePaymentPendingStatus(mpid, RazorPaymentId);

                        objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                        return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    }
                }

                #endregion

                #region UPDATE SUCCESS STATUS

                bool SuccessDBUpdate = false;
                if (RazorPaymentId != null && RazorPaymentId.Length > 0)
                {
                    if (isValidSignature && isValidTrasaction)
                    {
                        SuccessDBUpdate = objRazorCheckoutResponseBAL.UpdatePaymentSuccessStatus(mpid, RazorOrderId, RazorPaymentId);
                        objPaymentBO.PaymentReferenceData2 = RazorPaymentId;
                        objLogger.WriteInfo(string.Format("Payment Success {0}", LogData.ToString()));
                        objRazorCheckoutResponseBAL.UpdatePostPaymentDetail(mpid, RazorPaymentId);
                    }
                }

                #endregion

                #region PUSH RESPONSE TO QUEUE FOR SUCCESS

                if (objPaymentRequestSystemMasterBO.QueueName != null && objPaymentRequestSystemMasterBO.QueueName.Length > 0)
                {
                    if (SuccessDBUpdate)
                    {
                        if (!objRazorCheckoutResponseBAL.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, objPaymentBO))
                        {
                            objLogger.WriteInfo(string.Format("Failed to push transaction data to queue: {0} {1}", objPaymentRequestSystemMasterBO.QueueName, LogData.ToString()));
                        }
                    }
                }

                #endregion

                #region SEND RESPONSE

                if (SuccessDBUpdate)
                {
                    successMsg = string.Format(ConfigurationManager.AppSettings["successpaymsg"], mpid, (Convert.ToDecimal(objPaymentBO.Amount) / 100).ToString("#.00"));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "success?message=" + successMsg + "&mpid=" + mpid);
                    return Redirect(returnURL + "success?message=" + successMsg + "&mpid=" + mpid);
                }

                #endregion
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, LogData.ToString());
                objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], mpid, "Internal Server Error."));
                return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], mpid, "Internal Server Error."));
            }

            ViewBag.Message = successMsg;
            return View();
        }
    }
}