using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MeruPaymentWeb.Controllers
{
    public class PaytmPaymentController : Controller
    {
        private LogHelper objLogger = null;
        private PaymentBO objPaymentBO = new PaymentBO();
        private PayTMCheckoutFormBAL objPayTMCheckoutFormBAL = new PayTMCheckoutFormBAL();
        private PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = null;
        private PayTMCheckout PayTMCheckoutModel = new PayTMCheckout();
        private StringBuilder LogData = new StringBuilder();
        private string ReturnUrl = "";
        private string TransURL = "";
        private string Industry = "";
        private string Website = "";
        private const string ChannelId = "WAP";
        private PayTMPaymentRequestBO objPayTMPaymentRequestBO = new PayTMPaymentRequestBO();

        //[NonAction]
        public ActionResult Index()
        {
            objLogger = new LogHelper("PaytmPaymentController");
            objLogger.MethodName = "Index()";

            string MeruPaymentId = Request["mpid"];
            string Host = ConfigurationManager.AppSettings["MeruPayment_Host"];

            Industry = ConfigurationManager.AppSettings["Meru_Industry"];
            Website = ConfigurationManager.AppSettings["PayTM_Website"];
            PayTMCheckoutModel.MID = ConfigurationManager.AppSettings["PayTM_MechantId"];
            PayTMCheckoutModel.TRANURL = ConfigurationManager.AppSettings["PayTM_TransactionURL"];
            PayTMCheckoutModel.CALLBACK_URL = Host + ConfigurationManager.AppSettings["PayTM_CallBackURL"]; 

            try
            {
                #region GET PAYMENT DETAIL IF ALREADY CREATED
                if (MeruPaymentId == null || MeruPaymentId.Length == 0)
                {
                    ViewBag.Message = "Invalid Request.";
                    objLogger.WriteInfo("Request key mpid is missing.");

                    return View();
                }

                objPaymentBO = objPayTMCheckoutFormBAL.GetMeruPaymentDetail(MeruPaymentId);
                if (objPaymentBO == null)
                {
                    ViewBag.Message = "Invalid Request.";
                    objLogger.WriteInfo(string.Format("Payment status is not created for Meru payment Id {0}", MeruPaymentId));
                    return View();
                }

                TimeSpan span = DateTime.Now - objPaymentBO.CreatedOn;
                if (objPaymentBO.PaymentStatus != PaymentStatus.PaymentInitiated || span.TotalMinutes > 1)
                {
                    ViewBag.Message = "Invalid Request.";
                    objLogger.WriteInfo(string.Format("No payment found against Meru payment Id {0}", MeruPaymentId));
                    return View();
                }

                #endregion

                #region GET SOURCE DETAIL

                objPaymentRequestSystemMasterBO = objPayTMCheckoutFormBAL.GetSourceDetail(objPaymentBO.RequestSource);
                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.WriteInfo(string.Format("Source is either unknown or null. Source: {0}", LogData.ToString()));
                    ViewBag.Message = "Invalid Request.";
                    return View();
                }

                ReturnUrl = objPaymentRequestSystemMasterBO.ReturnURL;

                #endregion


                objPayTMPaymentRequestBO = new PayTMPaymentRequestBO();
                if (objPaymentBO.RequestReferenceVal.Length > 0)
                {
                    string _CustomerId = "";
                    Dictionary<string, string> obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(objPaymentBO.RequestReferenceVal);
                    foreach (KeyValuePair<string, string> item in obj.ToList<KeyValuePair<string, string>>())
                    {
                        _CustomerId += item.Value + "_";
                    }
                    objPayTMPaymentRequestBO.CustomerId = _CustomerId.TrimEnd('_');
                }
                else
                {
                    objPayTMPaymentRequestBO.CustomerId = objPaymentBO.Mobile;
                }
                objPayTMPaymentRequestBO.ChannelId = ChannelId;
                objPayTMPaymentRequestBO.Website = Website;
                objPayTMPaymentRequestBO.EmailId = objPaymentBO.Email;
                objPayTMPaymentRequestBO.MobileNo = objPaymentBO.Mobile;
                objPayTMPaymentRequestBO.OrderId = objPaymentBO.PaymentTransactionId;
                objPayTMPaymentRequestBO.TransAmount = objPaymentBO.Amount.ToString();

                PayTMCheckoutModel.TRANURL += "?ORDER_ID=" + objPayTMPaymentRequestBO.OrderId;
                PayTMCheckoutModel.CHANNEL_ID = objPayTMPaymentRequestBO.ChannelId;
                PayTMCheckoutModel.WEBSITE = objPayTMPaymentRequestBO.Website;
                PayTMCheckoutModel.EMAIL = objPayTMPaymentRequestBO.EmailId;
                PayTMCheckoutModel.MOBILE_NO = objPayTMPaymentRequestBO.MobileNo;
                PayTMCheckoutModel.CUST_ID = objPayTMPaymentRequestBO.CustomerId;
                PayTMCheckoutModel.MERC_UNQ_REF = objPayTMPaymentRequestBO.CustomerId;
                PayTMCheckoutModel.ORDER_ID = objPayTMPaymentRequestBO.OrderId;
                PayTMCheckoutModel.TXN_AMOUNT = objPayTMPaymentRequestBO.TransAmount;
                PayTMCheckoutModel.INDUSTRY_TYPE_ID = Industry;
                PayTMCheckoutModel.PAYMENT_MODE_ONLY = "YES";
                PayTMCheckoutModel.AUTH_MODE = "USRPWD";
                PayTMCheckoutModel.PAYMENT_TYPE_ID = "PPI";

                PayTMCheckoutModel.CHECKSUMHASH = objPayTMCheckoutFormBAL.GenerateChecksum(objPayTMPaymentRequestBO);// objPayTMBAL paytm.CheckSum.generateCheckSum(MerchantKey, parameters);

                if (PayTMCheckoutModel.CHECKSUMHASH == null || PayTMCheckoutModel.CHECKSUMHASH.Length == 0)
                {
                    objLogger.WriteInfo(string.Format(" Checksum generation failed for order id {0} mobile {1}", PayTMCheckoutModel.ORDER_ID, PayTMCheckoutModel.MOBILE_NO));
                    return Redirect(ReturnUrl + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], MeruPaymentId, "Error!"));
                }

                LogData.Append(string.Format(" TransURl is {0}, Industry is {1}, Website is {2} ", TransURL, Industry, Website));
                objLogger.WriteInfo("Before posting data to Paytm " + JsonConvert.SerializeObject(PayTMCheckoutModel, Formatting.None) + LogData.ToString());
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, LogData.ToString());
                return Redirect(ReturnUrl + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], MeruPaymentId, "Error!"));
            }
            finally
            {
                objLogger.Dispose();
            }

            return View(PayTMCheckoutModel);
        }
    }
}