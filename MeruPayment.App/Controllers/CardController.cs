using AutoMapper;
using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace MeruPayment.App.Controllers
{
    public class CardDeleteRequestDTO
    {
        public string CardId { get; set; }
    }

    public class CardController : ApiController
    {
        private LogHelper _logHelper;
        //private List<string> _headerKeys;
        //private Tuple<string, string, bool> _returnValue = null;

        public CardController()
        {
            _logHelper = new LogHelper("CardController()");
        }

        //public IHttpActionResult Delete(CardDeleteRequestDTO cardDeleteRequestDTO)
        //{
        //    _logHelper.MethodName = "Delete";
        //    try
        //    {
        //        #region CAPTURE REQUEST DATA

        //        string documentContents;
        //        using (Stream receiveStream = HttpContext.Current.Request.InputStream)
        //        {
        //            using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
        //            {
        //                documentContents = readStream.ReadToEnd();
        //            }
        //        }
        //        _logHelper.WriteDebug("Request Body: " + documentContents);
               
        //        _logHelper.WriteDebug("Request Header: " + Convert.ToString(HttpContext.Current.Request.Headers));

        //        string _authorization = "", _checksum = "", _mobile = "", _appSource = "", _deviceId = "";

        //        _authorization = Convert.ToString(Request.Headers.Authorization.Parameter); //reqHeader["Authorization"];
        //        _checksum = Convert.ToString(HttpContext.Current.Request.Headers["Checksum"]);
        //        _mobile = Convert.ToString(HttpContext.Current.Request.Headers["Mobile"]);
        //        _appSource = Convert.ToString(HttpContext.Current.Request.Headers["AppSource"]);
        //        _deviceId = Convert.ToString(HttpContext.Current.Request.Headers["DeviceId"]);

        //        //_headerKeys = new List<string>();
        //        //_headerKeys.Add("Authorization");
        //        //_headerKeys.Add("Checksum");
        //        //_headerKeys.Add("Mobile");
        //        //_headerKeys.Add("AppSource");

        //        //CaptureRequest captureRequest = new CaptureRequest(_headerKeys);
        //        //Tuple<string, string, Dictionary<string, string>> _returnValue = captureRequest.Process();
        //        //if (_returnValue.Item1 != "200")
        //        //{
        //        //    return Content(HttpStatusCode.InternalServerError, new CardAutoChargeResponseDTO
        //        //    {
        //        //        StatusCode = Convert.ToInt32(_returnValue.Item1),
        //        //        StatusMessage = _returnValue.Item2
        //        //    });
        //        //}

        //        #endregion

        //        #region OBJECT MAPPING

        //        var config = new MapperConfiguration(cfg =>
        //        {
        //            cfg.CreateMap<CardDeleteRequestDTO, CardDeleteBO>();
        //        });

        //        IMapper iMapper = config.CreateMapper();

        //        CardDeleteBO cardDeleteBO = iMapper.Map<CardDeleteRequestDTO, CardDeleteBO>(cardDeleteRequestDTO);

        //        cardDeleteBO.AuthToken = _authorization;// _returnValue.Item3["Authorization"];
        //        cardDeleteBO.Checksum = _checksum; //_returnValue.Item3["Checksum"];
        //        cardDeleteBO.Contact = _mobile; //_returnValue.Item3["Mobile"];
        //        cardDeleteBO.AppSource = _appSource; //_returnValue.Item3["AppSource"];

        //        #endregion

        //        #region PROCESS

        //        CardDeleteBAL cardDeleteBAL = new CardDeleteBAL();
        //        Tuple<string, string, bool>  _returnCardDeleteValue = cardDeleteBAL.ProcessRequest(cardDeleteBO);

        //        if (_returnCardDeleteValue.Item1 != "200")
        //        {
        //            return Content(HttpStatusCode.BadRequest, new CardAutoChargeResponseDTO
        //            {
        //                StatusCode = Convert.ToInt32(_returnCardDeleteValue.Item1),
        //                StatusMessage = _returnCardDeleteValue.Item2
        //            });
        //        }

        //        return Content(HttpStatusCode.OK, new CardAutoChargeResponseDTO
        //        {
        //            StatusCode = Convert.ToInt32(_returnCardDeleteValue.Item1),
        //            StatusMessage = _returnCardDeleteValue.Item2
        //        });

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        _logHelper.WriteError(ex, "Error occured while deleting card.");
        //        return Content(HttpStatusCode.InternalServerError, new CardAutoChargeResponseDTO { StatusCode = 500, StatusMessage = "Error" });
        //    }
        //    finally
        //    {
        //        _logHelper.Dispose();
        //    }
        //}
    }
}
