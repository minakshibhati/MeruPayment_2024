using AutoMapper;
using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace MeruPayment.App.Controllers
{
    internal class CardAutoChargeResponseDTO
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

    public class CardAutoChargeController : ApiController
    {
        private LogHelper _logHelper;
        //private List<string> _headerKeys;
        private Tuple<string, string, Dictionary<string, string>> _returnValue = null;
        private CardAutoChargeBO _cardAutoChargeBO = null;
        private CardAutoChargeResponseDTO _cardAutoChargeResponseDTO = null;
        public CardAutoChargeController()
        {
            _logHelper = new LogHelper("CardAutoChargeController()");
        }

        public IHttpActionResult Post(CardAutoChargeRequestDTO cardAutoChargeRequestDTO)
        {
            _logHelper.MethodName = "Post";
            try
            {
                #region CAPTURE REQUEST DATA

                string documentContents;
                using (Stream receiveStream = HttpContext.Current.Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                        documentContents = HttpUtility.UrlDecode(documentContents);
                    }
                }
                _logHelper.WriteDebug("Request Body: " + documentContents);

                //var reqHeader = HttpContext.Current.Request.Headers;
                _logHelper.WriteDebug("Request Header: " + Convert.ToString(HttpContext.Current.Request.Headers));

                string _authorization = "", _checksum = "", _mobile = "", _appSource = "", _deviceId = "";

                if (Request.Headers.Authorization != null)
                {
                    _authorization = Convert.ToString(Request.Headers.Authorization.Parameter); //reqHeader["Authorization"];
                }

                _checksum = Convert.ToString(HttpContext.Current.Request.Headers["Checksum"]);
                _mobile = Convert.ToString(HttpContext.Current.Request.Headers["Mobile"]);
                _appSource = Convert.ToString(HttpContext.Current.Request.Headers["AppSource"]);
                _deviceId = Convert.ToString(HttpContext.Current.Request.Headers["DeviceId"]);

                //_headerKeys = new List<string>();
                //_headerKeys.Add("Authorization");
                //_headerKeys.Add("Checksum");
                //_headerKeys.Add("Mobile");
                //_headerKeys.Add("AppSource");

                //CaptureRequest captureRequest = new CaptureRequest(_headerKeys);
                //_returnValue = captureRequest.Process();
                //if (_returnValue.Item1 != "200")
                //{
                //    return Content(HttpStatusCode.InternalServerError, new CardAutoChargeResponseDTO
                //    {
                //        StatusCode = Convert.ToInt32(_returnValue.Item1),
                //        StatusMessage = _returnValue.Item2
                //    });
                //}

                if (!ModelState.IsValid || _mobile.Length == 0)
                {
                    string errMsg = ModelState.Values.SelectMany(v => v.Errors).Where(r => r.ErrorMessage.Length > 0).Select(e => e.ErrorMessage.Trim()).FirstOrDefault();
                    if (errMsg == null || errMsg.Length == 0)
                    {
                        errMsg = "Bad Request";
                    }
                    _logHelper.WriteInfo(string.Format("Invalid auto charge request. Detail:{0} mobile:{1}", errMsg.Length, _mobile));
                    return Content(HttpStatusCode.BadRequest, new CardAutoChargeResponseDTO { StatusCode = 400, StatusMessage = errMsg });
                }

                if (cardAutoChargeRequestDTO.email == null || cardAutoChargeRequestDTO.email.Length == 0)
                {
                    cardAutoChargeRequestDTO.email = ConfigurationManager.AppSettings["Razor_Default_EmailId"];
                }

                #endregion

                #region OBJECT MAPPING

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<CardAutoChargeRequestDTO, CardAutoChargeBO>();
                });

                IMapper iMapper = config.CreateMapper();

                _cardAutoChargeBO = iMapper.Map<CardAutoChargeRequestDTO, CardAutoChargeBO>(cardAutoChargeRequestDTO);

                _cardAutoChargeBO.AuthToken = _authorization; //_returnValue.Item3["Authorization"];
                _cardAutoChargeBO.Checksum = _checksum; //_returnValue.Item3["Checksum"];
                _cardAutoChargeBO.Contact = _mobile; //_returnValue.Item3["Mobile"];
                _cardAutoChargeBO.AppSource = _appSource; //_returnValue.Item3["AppSource"];
                _cardAutoChargeBO.RawRequest = documentContents;

                if (_cardAutoChargeBO.Desc != null && _cardAutoChargeBO.Desc.Length == 0)
                {
                    _cardAutoChargeBO.Desc = string.Format("Card auto charge from mobile no {0} for reference id {1}", _cardAutoChargeBO.Contact, cardAutoChargeRequestDTO.apprequestid);
                }

                #endregion

                #region PROCESS

                CardAutoChargeBAL cardAutoCharge = new CardAutoChargeBAL();
                _returnValue = cardAutoCharge.ProcessRequest(_cardAutoChargeBO);

                CardAutoChargeResponseDTO cardAutoChargeResponseDTO = new CardAutoChargeResponseDTO
                {
                    StatusCode = Convert.ToInt32(_returnValue.Item1),
                    StatusMessage = _returnValue.Item2,
                    Data = _returnValue.Item3 
                };

                if (_returnValue.Item1 != "200")
                {
                    return Content(HttpStatusCode.BadRequest, cardAutoChargeResponseDTO);
                }

                return Content(HttpStatusCode.OK, cardAutoChargeResponseDTO);

                #endregion
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing card auth.");
                return Content(HttpStatusCode.InternalServerError, new CardAutoChargeResponseDTO { StatusCode = 500, StatusMessage = "Error" });
            }
            finally
            {
                _logHelper.Dispose();
            }
        }
    }
}
