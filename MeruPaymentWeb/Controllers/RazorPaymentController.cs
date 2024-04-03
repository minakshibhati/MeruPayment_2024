using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MeruPaymentWeb.Controllers
{
    public class RazorPaymentController : Controller
    {
        // GET: RazorPayment
        [HttpPost]
        public ActionResult Index(PaymentCheckoutRequestDTO paymentCheckoutRequestDTO)
        {
            LogHelper objLogger = new LogHelper("RazorPaymentController");
            StringBuilder LogData = new StringBuilder();
            string returnURL = "", SecretKey = "";
            Checkout checkout = null;
            try
            {
                objLogger.MethodName = "Index()";
                CheckFormRequestBAL objCheckFormRequestBAL = new CheckFormRequestBAL();

                #region Capture RAW REQUEST JSON

                string documentContents = "";
                using (Stream receiveStream = Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                        documentContents = HttpUtility.UrlDecode(documentContents);
                    }
                }

#if DEBUG
                //documentContents = "{\"Source\":\"outstationweb\",\"Name\":\"Vaibhav\",\"Email\":\"vmakwana28@gmail.com\",\"Contact\":\"9821354464\",\"ReferenceId\":301,\"Amount\":81200,\"PurchaseDescription\":\"Outstation+Payment\",\"PaymentMethod\":\"\"}";
#endif

                objLogger.WriteInfo(string.Format("Meru Payment checkout Request: {0}", documentContents));
                objLogger.WriteInfo(string.Format("Source {0}", paymentCheckoutRequestDTO.Source));
                //objLogger.WriteInfo("Request Header: " + Convert.ToString(Request.Headers));

                if (documentContents != null && documentContents.Length > 0)
                {
                    paymentCheckoutRequestDTO.RawRequest = documentContents.Substring(0, documentContents.LastIndexOf(("&Checksum")));
                }

                #endregion

                #region GET REQUEST SOURCE

                //Dictionary<string, string> requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(documentContents);

                if (paymentCheckoutRequestDTO == null)
                {
                    ViewBag.Message = "Invalid Request.";
                    return View();
                }

                LogData.Append(string.Format("Source:{0} ", paymentCheckoutRequestDTO.Source));
                PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = objCheckFormRequestBAL.GetSourceDetail(paymentCheckoutRequestDTO.Source);

                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.WriteFatal(string.Format("Source is either unknown or null. Source: {0}", LogData.ToString()));
                    ViewBag.Message = "Invalid Request.";
                    return View();
                }

                objLogger.WriteInfo(string.Format("Successfully fetched source detail: {0}", LogData.ToString()));
                returnURL = objPaymentRequestSystemMasterBO.ReturnURL;
                SecretKey = objPaymentRequestSystemMasterBO.SecretCode;

                #endregion

                #region VALIDATE CHECKSUM

                //var reqHeader = Request.Headers;
                string signature = Convert.ToString(paymentCheckoutRequestDTO.Checksum);//reqHeader["Meru-Signature"]);

#if DEBUG
                //signature = "b4c8f55ba4ba1df6e9b1f1fd2eab819cf5e334e2101c3f816ce81b10e5becff0";
#endif

                if (signature == null || signature.Length == 0)
                {
                    ViewBag.Message = "Invalid Request.";
                    return View();
                }

                LogData.Append(string.Format("Meru sugnature:{0} ", signature));
                objLogger.WriteInfo(string.Format("Header value received: {0}", LogData.ToString()));
                SecretKey += paymentCheckoutRequestDTO.Contact;
                bool checksumStatus = objCheckFormRequestBAL.ValidateChecksum(signature, paymentCheckoutRequestDTO.RawRequest, SecretKey);
                if (!checksumStatus)
                {
                    objLogger.WriteWarn(string.Format("Invalid data received {0}", LogData.ToString()));
                    ViewBag.Message = "Invalid Request.";
                    return View();
                }

                objLogger.WriteInfo(string.Format("Checksum validation passed {0}", LogData.ToString()));

                #endregion

                #region READ REF REQUEST VALUE

                JObject objKeyInput = new JObject();
                if (objPaymentRequestSystemMasterBO.RequestKeyInput.Length > 0)
                {
                    foreach (string item in objPaymentRequestSystemMasterBO.RequestKeyInput.Split('|'))
                    {
                        objKeyInput.Add(new JProperty(item, Request[item]));//requestData[item]));
                        //LogData.Append(string.Format("{0}:{1} ", item, requestData[item]));
                    }
                }

                #endregion

                #region CREATE ENTRY IN DB

                string MeruPaymentId = "", OrderId = "";

                CreateOrder createOrder = new CreateOrder();
                Tuple<string, string, Dictionary<string, string>> returnValue = createOrder.ProcessRequest(new OrderBO
                {
                    AppSource = paymentCheckoutRequestDTO.Source,
                    AppRequestId = paymentCheckoutRequestDTO.ReferenceId,
                    AppRequestRefVal = objKeyInput.ToString(Formatting.None),
                    Amount = Convert.ToInt64(paymentCheckoutRequestDTO.Amount),
                    Desc = paymentCheckoutRequestDTO.PurchaseDescription,
                    DeviceId = "",
                    Contact = paymentCheckoutRequestDTO.Contact,
                    Email = paymentCheckoutRequestDTO.Email,
                    FullName = paymentCheckoutRequestDTO.Name,
                    OrderType = "TRANS",
                    PaymentId = "",
                    PaymentMethod = objCheckFormRequestBAL.GetPaymentMethodEnum(paymentCheckoutRequestDTO.PaymentMethod),
                    PaymentMethodRefId = 0,
                    PGOrderId = ""
                });

                if (returnValue.Item1 != "200")
                {
                    objLogger.WriteWarn(string.Format("Payment creation failed.{0}", LogData.ToString()));
                    ViewBag.Message = returnValue.Item2;
                    return View();
                };
                Dictionary<string, string> orderDetail = returnValue.Item3;
                OrderId = orderDetail["RazorpayOrderId"];
                MeruPaymentId = returnValue.Item3["PaymentId"];

                //PaymentBO objPaymentBO = new PaymentBO
                //{
                //    RequestSource = paymentCheckoutRequestDTO.Source,
                //    RequestUniqueId = paymentCheckoutRequestDTO.ReferenceId,
                //    RequestReferenceVal = objKeyInput.ToString(Formatting.None),
                //    Amount = Convert.ToInt64(paymentCheckoutRequestDTO.Amount),
                //    PurchaseDesc = paymentCheckoutRequestDTO.PurchaseDescription,
                //    FullName = paymentCheckoutRequestDTO.Name,
                //    Email = paymentCheckoutRequestDTO.Email,
                //    Mobile = paymentCheckoutRequestDTO.Contact,
                //    PaymentMethod = objCheckFormRequestBAL.GetPaymentMethodEnum(paymentCheckoutRequestDTO.PaymentMethod),
                //    PaymentType = "TRANS"
                //};

                //TODO: Cache implementation need to be done for master data.

                //string MeruPaymentId = objCheckFormRequestBAL.CreatePaymentEntry(objPaymentBO);
                if (MeruPaymentId.Length == 0)
                {
                    objLogger.WriteWarn(string.Format("Payment creation failed.{0}", LogData.ToString()));
                    ViewBag.Message = "Internal Server Error";
                    return View();
                }
                //objPaymentBO.PaymentTransactionId = MeruPaymentId;
                LogData.Append(string.Format("Meru payment Id:{0} ", MeruPaymentId.ToString()));
                objLogger.WriteInfo(string.Format("Payment created in DB: {0}", LogData.ToString()));

                #endregion

                #region CREATING OBJECT FOR RAZORPAY

                checkout = new Checkout();
                checkout.MeruPaymentId = MeruPaymentId;
                LogData.Append(string.Format("Meru Payment Id: {0} ", MeruPaymentId));
                checkout.Source = paymentCheckoutRequestDTO.Source;
                LogData.Append(string.Format("Request source: {0} ", paymentCheckoutRequestDTO.Source));
                checkout.Amount = Convert.ToInt64(paymentCheckoutRequestDTO.Amount);
                LogData.Append(string.Format("Amount: {0} ", paymentCheckoutRequestDTO.Amount));
                checkout.PurchaseDescription = paymentCheckoutRequestDTO.PurchaseDescription;
                LogData.Append(string.Format("Purchase Desc: {0} ", paymentCheckoutRequestDTO.PurchaseDescription));
                checkout.Name = paymentCheckoutRequestDTO.Name;
                LogData.Append(string.Format("Name: {0} ", paymentCheckoutRequestDTO.Name));
                checkout.Contact = paymentCheckoutRequestDTO.Contact;
                LogData.Append(string.Format("Mobile: {0} ", paymentCheckoutRequestDTO.Contact));
                checkout.Email = paymentCheckoutRequestDTO.Email;
                LogData.Append(string.Format("Email: {0} ", paymentCheckoutRequestDTO.Email));
                checkout.PaymentMethod = paymentCheckoutRequestDTO.PaymentMethod.ToString();
                LogData.Append(string.Format("PaymentMethod: {0} ", paymentCheckoutRequestDTO.PaymentMethod.ToString()));

                if (objPaymentRequestSystemMasterBO.RequestSourceName.ToLower() == "outstationweb")
                {
                    checkout.ShowPayTM = false;
                }
                else
                {
                    checkout.ShowPayTM = true;
                }
                checkout.ColorCode = objPaymentRequestSystemMasterBO.ColorCode;

                #endregion

                #region INITIALIZE PAYMENT

                //RazorCheckoutFormBAL objRazorCheckoutFormBAL = new RazorCheckoutFormBAL();

                //string OrderId = objRazorCheckoutFormBAL.InitializePaymentWithOrderId(objPaymentBO);

                if (OrderId.Length == 0)
                {
                    objLogger.WriteInfo(string.Format("Razorpay Order initialization failed {0}", LogData.ToString()));
                    return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], checkout.MeruPaymentId, "Error!"));
                }

                checkout.OrderId = OrderId;
                LogData.Append(string.Format("Razorpay Order Id: {0} ", OrderId));
                objLogger.WriteInfo(string.Format("Razorpay Order Created: {0}", LogData.ToString()));

                #endregion

                return View(checkout);

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Internal Server Error";
                objLogger.WriteError(ex, LogData.ToString());

                if (returnURL.Length > 0)
                {
                    return Redirect(returnURL + "failed?message=" + string.Format(ConfigurationManager.AppSettings["failurepaymsg"], checkout.MeruPaymentId, "Error!"));
                }
                return View();
            }
        }
    }
}