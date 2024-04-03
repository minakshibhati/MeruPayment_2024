using MeruCommonLibrary;
using MeruPaymentBAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MeruPaymentWeb.Controllers
{
    public class RazorAddCardResponseController : Controller
    {
        // GET: RazorAddCardResponse
        public ActionResult Index()
        {
            string returnURL = "";
            LogHelper logHelper = new LogHelper("RazorAddCardResponseController");

            try
            {
                logHelper.MethodName = "Index()";

                #region CAPTURE REQUEST

                string documentContents;
                using (Stream receiveStream = Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                        documentContents = HttpUtility.UrlDecode(documentContents);
                    }
                }
                logHelper.WriteDebug("Request Body: " + documentContents);

                var reqHeader = Request.Headers;
                logHelper.WriteDebug("Request Header: " + Convert.ToString(reqHeader));

                #endregion

                #region GET REQUEST PARAMETER VALUES

                string paymentId = Request["mpid"];
                string message = Convert.ToString(Request["msg"]);
                string errorCode = Request["error[code]"];
                string errorDescription = Request["error[description]"];
                string razorpayPaymentId = Convert.ToString(Request["razorpay_payment_id"]);
                string razorpayOrderId = Convert.ToString(Request["razorpay_order_id"]);
                string razorSignature = Convert.ToString(Request["razorpay_signature"]);

                #endregion

                #region PROCESS

                Tuple<string, string, Dictionary<string, string>> returnValue = null;
                RazorPayCardAuthCheckoutBAL razorPayCardAuthCheckoutBAL = new RazorPayCardAuthCheckoutBAL();

                if (message != null && message.Length > 0 && message.ToLower().Trim() == "cancelled")
                {
                    returnValue = razorPayCardAuthCheckoutBAL.ProcessCancelledResponse(paymentId);
                }

                if (errorCode != null && errorCode.Length > 0)
                {
                    returnValue = razorPayCardAuthCheckoutBAL.ProcessFailureResponse(paymentId, errorCode, errorDescription);
                }

                if (razorpayOrderId != null && razorpayOrderId.Length > 0 && razorpayPaymentId != null && razorpayPaymentId.Length > 0 && razorSignature != null && razorSignature.Length > 0)
                {
                    returnValue = razorPayCardAuthCheckoutBAL.ProcessSuccessResponse(paymentId, razorpayOrderId, razorpayPaymentId, razorSignature);
                }

                if (returnValue.Item1 != "200")
                {
                    logHelper.WriteInfo("Razor checkout response BAL " + returnValue.Item1 + " " + returnValue.Item2);
                    ViewBag.Message = returnValue.Item2;
                }
                returnURL = returnValue.Item3["AppReturnURL"];

                #endregion
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Internal Server Error!";
                logHelper.WriteError(ex, "Error occured at card authorization response.");
            }

            if (returnURL.Length > 0)
            {
                logHelper.WriteInfo("Redirecting to URL " + returnURL);
                return Redirect(returnURL);
            }

            return View();
        }
    }
}