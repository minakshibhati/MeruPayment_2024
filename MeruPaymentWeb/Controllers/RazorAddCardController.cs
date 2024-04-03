using AutoMapper;
using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
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
    public class RazorAddCardController : Controller
    {
        // GET: RazorAddCard
        [HttpPost]
        public ActionResult Index(CardAuthCheckoutRequestDTO cardAuthCheckoutRequestDTO)
        {
            LogHelper logHelper = new LogHelper("RazorAddCardController");

            try
            {
                bool EnableAuthCard = false;
                EnableAuthCard = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableAuthCard"]);
                if (!EnableAuthCard)
                {
                    ViewBag.Message = ConfigurationManager.AppSettings["AuthCardDisableMsg"];
                    return View();
                }
                
                logHelper.MethodName = "Index(CardAuthCheckoutRequestDTO cardAuthCheckoutRequestDTO)";

                #region CAPTURE REQUEST DATA

                var documentContents = "";
                using (Stream receiveStream = Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                        documentContents = HttpUtility.UrlDecode(documentContents); //WebUtility.HtmlDecode
                    }
                }
                logHelper.WriteDebug("Request Body: " + documentContents);

                //var reqHeader = Request.Headers;
                //logHelper.WriteDebug("Request Header: " + Convert.ToString(reqHeader));

                #endregion

                #region OBJECT MAPPING

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<CardAuthCheckoutRequestDTO, CardAuthCheckoutBO>();
                });

                IMapper iMapper = config.CreateMapper();

                CardAuthCheckoutBO cardAuthCheckoutBO = new CardAuthCheckoutBO();
                cardAuthCheckoutBO = iMapper.Map<CardAuthCheckoutRequestDTO, CardAuthCheckoutBO>(cardAuthCheckoutRequestDTO);

                //cardAuthCheckoutBO.AuthToken = Request.Headers["Authorization"] != null ? Request.Headers["Authorization"].Replace("Bearer", "").Replace("bearer", "").Trim() : "";
                //cardAuthCheckoutBO.Checksum = Request.Headers["Checksum"] != null ? Request.Headers["Checksum"] : "";
                //cardAuthCheckoutBO.Contact = Request.Headers["Mobile"] != null ? Request.Headers["Mobile"] : "";
                //cardAuthCheckoutBO.AppSource = Request.Headers["AppSource"] != null ? Request.Headers["AppSource"] : "";
                //cardAuthCheckoutBO.DeviceId = Request.Headers["DeviceId"] != null ? Request.Headers["DeviceId"] : "";
                cardAuthCheckoutBO.AuthToken = cardAuthCheckoutRequestDTO.Authorization;
                cardAuthCheckoutBO.Contact = cardAuthCheckoutRequestDTO.Mobile;
                if (documentContents != null && documentContents.Length > 0)
                {
                    cardAuthCheckoutBO.RawRequest = documentContents.Substring(0, documentContents.LastIndexOf(("&Checksum")));
                }


#if DEBUG
                //cardAuthCheckoutBO.AuthToken = "";
                //cardAuthCheckoutBO.Checksum = "";
                //cardAuthCheckoutBO.Contact = "9833240124";
                //cardAuthCheckoutBO.AppSource = "website";
                //cardAuthCheckoutBO.DeviceId = Guid.NewGuid().ToString();
#endif
                cardAuthCheckoutBO.Desc = "Card Authorization";
                cardAuthCheckoutBO.AppSecret = cardAuthCheckoutBO.AppReturnURL = string.Empty;

                #endregion

                #region PROCESS PG DEFAULT

                CardAuthCheckoutBAL cardAuthCheckoutBAL = new CardAuthCheckoutBAL();
                Tuple<string, string, Dictionary<string, string>> returnValue = cardAuthCheckoutBAL.ProcessRequest(ref cardAuthCheckoutBO);

                if (returnValue.Item1 != "200")
                {
                    logHelper.WriteInfo(string.Format("Card auth execution is unsuccessful. Return URL {0}, Status {1}, Msg {2}",
                        cardAuthCheckoutBO.AppReturnURL, returnValue.Item1, returnValue.Item2));

                    if (returnValue.Item1 == "400")
                    {
                        return RedirectToAction("Index", "Invalid", new { message = ConfigurationManager.AppSettings["InvalidChecksumMsg"] });
                    }

                    if (returnValue.Item1 == "401")
                    {
                        return RedirectToAction("Index", "Unauthorised", new { message = ConfigurationManager.AppSettings["InvalidAuthTokenMsg"] });
                    }

                    if (cardAuthCheckoutBO.AppReturnURL.Length > 0)
                    {
                        return Redirect(cardAuthCheckoutBO.AppReturnURL + "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"]);
                    }

                    ViewBag.Message = string.Format("{0}-{1}", returnValue.Item1, returnValue.Item2);
                    return View();
                }

                #endregion

                #region PROCESS RAZORPAY

                RazorPayCardAuthCheckoutBAL razorPayCardAuthCheckoutBAL = new RazorPayCardAuthCheckoutBAL();
                Tuple<string, string, CardAuthCheckoutBO> returnRazorpayValue = razorPayCardAuthCheckoutBAL.ProcessRequest(returnValue.Item3["PaymentId"]);

                if (returnRazorpayValue.Item1 != "200")
                {
                    if (returnRazorpayValue.Item3.AppReturnURL.Length > 0)
                    {
                        string URL = returnRazorpayValue.Item3.AppReturnURL + "failed?message=" + ConfigurationManager.AppSettings["AuthCard_FailureMsg"];

                        logHelper.WriteInfo("Card auth checkout redirecting to URL " + URL);

                        return Redirect(URL);
                    }
                    ViewBag.Message = returnRazorpayValue.Item2;
                    return View();
                }

                return View(returnRazorpayValue.Item3);

                #endregion
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Internal Server Error!";
                logHelper.WriteError(ex, "Error occured while authorizing card.");
                return View();
            }
        }
    }
}