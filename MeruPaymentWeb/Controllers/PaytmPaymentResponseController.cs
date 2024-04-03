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
    public class PaytmPaymentResponseController : Controller
    {
        private LogHelper objLogger = new LogHelper("PaytmPaymentResponseController");
        private Dictionary<String, String> parameters = new Dictionary<string, string>();
        private StringBuilder LogData = new StringBuilder();
        private string AmountInDB = "";
        private string OrderIdInDB = "";
        private string returnURL = "";
        private PaymentBO objPaymentBO = null;
        private PayTMCheckoutResponseBAL objPayTMCheckoutResponseBAL = new PayTMCheckoutResponseBAL();
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = new PaymentRequestSystemMasterBO();

        // GET: PaytmPaymentResponse
        public ActionResult Index()
        {
            string paytmChecksum = "";
            string PayTmOrderId = "";
            string successMsg = "";
            objLogger.MethodName = "Index()";
            try
            {
                #region READ RESPONSE VALUE

                foreach (string key in Request.Form.Keys)
                {
                    parameters.Add(key.Trim(), Request.Form[key].Trim());
                    LogData.Append(key.Trim() + ":" + Request.Form[key].Trim() + " ");
                }

                if (parameters.ContainsKey("ORDERID"))
                {
                    PayTmOrderId = parameters["ORDERID"];
                }
                objLogger.WriteInfo("Received response from Paytm." + LogData.ToString());

                if (parameters.ContainsKey("CHECKSUMHASH"))
                {
                    paytmChecksum = parameters["CHECKSUMHASH"];
                    parameters.Remove("CHECKSUMHASH");
                }

                #endregion

                #region READ PAYMENT DETAIL FROM DB

                objPaymentBO = objPayTMCheckoutResponseBAL.GetMeruPaymentDetail(PayTmOrderId);
                if (objPaymentBO == null)
                {
                    ViewBag.Message = "Invalid Data.";
                    objLogger.WriteInfo(string.Format("Payment data is not present for Meru payment Id {0}", PayTmOrderId));
                    return View();
                }

                AmountInDB = objPaymentBO.Amount.ToString();
                OrderIdInDB = objPaymentBO.PaymentReferenceData1;

                #endregion

                #region GET SOURCE DETAIL

                objPaymentRequestSystemMasterBO = objPayTMCheckoutResponseBAL.GetSourceDetail(objPaymentBO.RequestSource);
                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.WriteInfo(string.Format("Source is either unknown or null. Source: {0}", LogData.ToString()));
                    ViewBag.Message = "Invalid Data.";
                    return View();
                }
                returnURL = objPaymentRequestSystemMasterBO.ReturnURL;

                #endregion


                #region VERIFY CHECKSUM

                if (!objPayTMCheckoutResponseBAL.VerifyCheckSum(parameters, paytmChecksum))
                {
                    objLogger.WriteInfo(string.Format("PayTm response signature mismatch {0}", LogData.ToString()));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                }

                #endregion


                #region UPDATE RESPONSE STATUS

                bool SuccessDBUpdate = false;

                SuccessDBUpdate = objPayTMCheckoutResponseBAL.UpdatePaymentResponseDetail(parameters, objPaymentBO.PaymentReferenceData1, objPaymentBO.PaymentTransactionId);
                objLogger.WriteInfo(string.Format("DB update status {0}, {1}", SuccessDBUpdate, LogData.ToString()));

                #endregion

                string status = "", resCode = "", resMsg = "failed.";
                if (parameters.ContainsKey("STATUS"))
                {
                    status = parameters["STATUS"];
                }
                if (parameters.ContainsKey("RESPCODE"))
                {
                    resCode = parameters["RESPCODE"];
                }
                if (parameters.ContainsKey("RESPMSG"))
                {
                    resMsg = parameters["RESPMSG"];
                }

                if (status == "TXN_SUCCESS" && resCode == "01")
                {
                    successMsg = string.Format(ConfigurationManager.AppSettings["successpaymsg"], objPaymentBO.PaymentTransactionId, (Convert.ToDecimal(objPaymentBO.Amount) / 100).ToString("#.00"));
                    returnURL += "success?message=" + successMsg + "&mpid=" + objPaymentBO.PaymentTransactionId;
                }
                else if (status == "TXN_FAILURE")
                {
                    //return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], objPaymentBO.MeruPaymentId, resMsg));
                    returnURL += "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], objPaymentBO.PaymentTransactionId, resMsg);
                }
                else if (status == "PENDING")
                {
                    //return Redirect(returnURL + "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"]);
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["failurepaymsg1"];
                }

                #region PUSH RESPONSE TO QUEUE FOR SUCCESS

                if (objPaymentRequestSystemMasterBO.QueueName != null && objPaymentRequestSystemMasterBO.QueueName.Length > 0)
                {
                    if (status == "TXN_SUCCESS" && resCode == "01")
                    {
                        if (!objPayTMCheckoutResponseBAL.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, objPaymentBO.PaymentTransactionId))
                        {
                            objLogger.WriteInfo(string.Format("Failed to push transaction data to queue: {0} {1}", objPaymentRequestSystemMasterBO.QueueName, LogData.ToString()));
                        }
                    }
                }

                #endregion

                #region SEND RESPONSE


                if (status == "TXN_SUCCESS" && resCode == "01")
                {
                    //successMsg = string.Format(ConfigurationManager.AppSettings["successpaymsg"], objPaymentBO.MeruPaymentId, (Convert.ToDecimal(objPaymentBO.Amount) / 100).ToString("#.00"));
                    //redirectTo = returnURL + "success?message=" + successMsg + "&mpid=" + objPaymentBO.MeruPaymentId;

                    //successMsg = string.Format(ConfigurationManager.AppSettings["successpaymsg"], PayTmOrderId, (Convert.ToDecimal(objPaymentBO.Amount) / 100).ToString("#.00"));
                    objLogger.WriteInfo("Redirecting to URL " + returnURL);
                    return Redirect(returnURL);//returnURL + "success?message=" + successMsg + "&mpid=" + PayTmOrderId);
                    
                }

                #endregion
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, LogData.ToString());
                if (returnURL.Length > 0)
                {
                    objLogger.WriteInfo("Redirecting to URL " + returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], objPaymentBO.PaymentTransactionId, "Internal Server Error."));
                    return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], objPaymentBO.PaymentTransactionId, "Internal Server Error."));
                }
                else
                {
                    ViewBag.Message = "Internal Server Error.";
                    return View();
                }
            }
            finally
            {
                objLogger.Dispose();
            }
            objLogger.WriteInfo("Redirecting to URL " + returnURL);
            return Redirect(returnURL);
        }
    }
}