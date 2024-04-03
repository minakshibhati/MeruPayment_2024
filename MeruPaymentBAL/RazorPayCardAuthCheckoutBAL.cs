using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class RazorPayCardAuthCheckoutBAL
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue;
        private Dictionary<string, string> returnData;

        public RazorPayCardAuthCheckoutBAL()
        {
            _logHelper = new LogHelper("RazorPayCardAuthCheckoutBAL");
        }

        public Tuple<string, string, CardAuthCheckoutBO> ProcessRequest(string paymentId)
        {
            _logHelper.MethodName = "ProcessRequest()";
            try
            {
                //TODO: SOURCE IP VALIDATION

                GetOrder getOrder = new GetOrder();
                Tuple<string, string, OrderBO> returnOrderValue = getOrder.ByPaymentId(paymentId);
                if (returnOrderValue.Item1 != "200")
                {
                    return new Tuple<string, string, CardAuthCheckoutBO>("500", returnOrderValue.Item2, null);
                }

                GetCustomer getCustomer = new GetCustomer();
                Tuple<string, string, CustomerBO> returnCustomerValue = getCustomer.ByMobileNo(returnOrderValue.Item3.Contact, PaymentGatway.Razorpay);
                if (returnCustomerValue.Item1 != "200")
                {
                    return new Tuple<string, string, CardAuthCheckoutBO>("500", returnCustomerValue.Item2, null);
                }

                #region GET SOURCE DETAIL

                SourceDetail sourceDetail = new SourceDetail();
                Tuple<string, string, Dictionary<string, string>> _returnSourceValue = sourceDetail.BySourceName(returnOrderValue.Item3.AppSource);
                if (_returnSourceValue.Item1 != "200")
                {
                    return new Tuple<string, string, CardAuthCheckoutBO>(_returnSourceValue.Item1, _returnSourceValue.Item2, null);
                }

                #endregion

                return new Tuple<string, string, CardAuthCheckoutBO>(
                    "200",
                    "Success",
                    new CardAuthCheckoutBO
                    {
                        Amount = Convert.ToInt32(returnOrderValue.Item3.Amount),
                        Desc = returnOrderValue.Item3.Desc,
                        PGOrderId = returnOrderValue.Item3.PGOrderId,
                        PGCustomerId = returnCustomerValue.Item3.PGCustomerId,
                        PaymentId = returnOrderValue.Item3.PaymentId,
                        FullName = returnOrderValue.Item3.FullName,
                        Email = returnOrderValue.Item3.Email,
                        Contact = returnOrderValue.Item3.Contact,
                        PaymentMethod = returnOrderValue.Item3.PaymentMethod,
                        AppColorCode = _returnSourceValue.Item3["AppColorCode"],
                        AppReturnURL = _returnSourceValue.Item3["AppReturnURL"]
                    });
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing card auth request.");
                return new Tuple<string, string, CardAuthCheckoutBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessCancelledResponse(string paymentId)
        {
            returnData = new Dictionary<string, string>();
            try
            {
                string returnURL = "";

                SourceDetail sourceDetail = new SourceDetail();
                returnValue = sourceDetail.ByPaymentId(paymentId);
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }
                returnURL = returnValue.Item3["AppReturnURL"];

                CancelPayment cancelPayment = new CancelPayment();
                returnValue = cancelPayment.ProcessRequest(paymentId);
                if (returnValue.Item1 != "200")
                {
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"];
                    returnData.Add("AppReturnURL", returnURL);
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Unable to process cancellation.",
                        returnData);
                }
                returnURL += "cancelled?message=" + ConfigurationManager.AppSettings["AuthCard_CancelMsg"];
                returnData.Add("AppReturnURL", returnURL);
                return new Tuple<string, string, Dictionary<string, string>>(
                        "200",
                        "Success",
                        returnData);

            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing cancellation.");

                if (returnData.ContainsKey("AppReturnURL"))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    returnData);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessFailureResponse(string paymentId, string errorCode, string errorDescription)
        {
            returnData = new Dictionary<string, string>();
            try
            {
                string returnURL = "";

                SourceDetail sourceDetail = new SourceDetail();
                returnValue = sourceDetail.ByPaymentId(paymentId);
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }
                returnURL = returnValue.Item3["AppReturnURL"];

                FailurePayment failurePayment = new FailurePayment();
                returnValue = failurePayment.ProcessRequest(paymentId, errorCode, errorDescription);

                if (returnValue.Item1 != "200")
                {
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"];
                    returnData.Add("AppReturnURL", returnURL);
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Unable to process failure.",
                        returnData);
                }

                returnURL += "failed?message=" +
                    ((errorDescription == null || errorDescription.Length == 0) ? ConfigurationManager.AppSettings["AuthCard_FailureMsg"] : errorDescription);

                returnData.Add("AppReturnURL", returnURL);
                return new Tuple<string, string, Dictionary<string, string>>(
                        "200",
                        "Success",
                        returnData);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing cancellation.");
                if (returnData.ContainsKey("AppReturnURL"))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    returnData);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessSuccessResponse(string paymentId, string razorpayOrderId, string razorpayPaymentId, string razorSignature)
        {
            returnData = new Dictionary<string, string>();
            try
            {
                string returnURL = "";

                SourceDetail sourceDetail = new SourceDetail();
                returnValue = sourceDetail.ByPaymentId(paymentId);
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }
                returnURL = returnValue.Item3["AppReturnURL"];

                CommonMethods objCommonMethods = new CommonMethods();
                if (!objCommonMethods.ValidateData_HMACSHAH256(razorSignature, razorpayOrderId + "|" + razorpayPaymentId, ConfigurationManager.AppSettings["Razor_Key_Secret"]))
                {
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"];
                    returnData.Add("AppReturnURL", returnURL);

                    return new Tuple<string, string, Dictionary<string, string>>(
                   "500",
                   "Razorpay response signature mismatch",
                   returnData);
                }

                SuccessPayment successPayment = new SuccessPayment();
                returnValue = successPayment.ProcessRequest(paymentId, razorpayPaymentId, PaymentGatway.Razorpay);
                if (returnValue.Item1 != "200")
                {
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"];
                    returnData.Add("AppReturnURL", returnURL);
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Unable to process success.",
                        returnData);
                }

                GetCustomer getCustomer = new GetCustomer();
                Tuple<string, string, CustomerBO> returnCusomerValue = getCustomer.ByPaymentId(paymentId);

                SaveAuthorisedCards saveAuthorisedCards = new SaveAuthorisedCards();
                Tuple<string, string, Dictionary<string, string>> returnSaveCard = saveAuthorisedCards.ProcessRequest(returnCusomerValue.Item3.PGCustomerId, paymentId);
                //returnStatus = objPaymentDAL.SaveCardDetails(objRazorpayCardBO, objRazorPaymentBO);

                RefundPayment refundPayment = new RefundPayment();
                Tuple<string, string, Dictionary<string, string>> returnRefundValue = refundPayment.ProcessRefund(paymentId, Convert.ToInt32(ConfigurationManager.AppSettings["CardAuthenticationCharge"]), ConfigurationManager.AppSettings["CardAuthRefundNote"]);
                if (returnSaveCard.Item1 == "400")
                {
                    returnURL += "failed?message=" + ConfigurationManager.AppSettings["AuthCard_DupFailureMsg"];
                    returnData.Add("AppReturnURL", returnURL);
                    return new Tuple<string, string, Dictionary<string, string>>(
                        returnSaveCard.Item1,
                        returnSaveCard.Item2,
                        returnData);
                }

                returnURL += "success?message=" + ConfigurationManager.AppSettings["AuthCard_SuccessMsg"] + "&mpid=" + paymentId;
                returnData.Add("AppReturnURL", returnURL);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnData);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing payment success.");
                if (returnData.ContainsKey("AppReturnURL"))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    returnData);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
