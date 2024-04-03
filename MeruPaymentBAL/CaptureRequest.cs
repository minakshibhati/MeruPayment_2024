using MeruCommonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MeruPaymentBAL
{
    //public class CaptureRequest
    //{
    //    private LogHelper _logHelper;
    //    private List<string> _headerKeys { get; set; }

    //    public CaptureRequest()
    //    {
    //        _logHelper = new LogHelper("CaptureRequest()");
    //    }

    //    public CaptureRequest(List<string> headerKeys)
    //    {
    //        _logHelper = new LogHelper("CaptureRequest()");
    //        _headerKeys = headerKeys;
    //    }

    //    public Tuple<string, string, Dictionary<string, string>> Process()
    //    {
    //        _logHelper.MethodName = "Process()";

    //        Dictionary<string, string> _headerKeyValue = null;
    //        Tuple<string, string, Dictionary<string, string>> _returnValue = null;

    //        try
    //        {
    //            if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.InputStream == null)
    //            {
    //                _logHelper.WriteInfo("Unable to capture the null request.");
    //                return new Tuple<string, string, Dictionary<string, string>>(
    //                    "500",
    //                    "Unable to capture the request.",
    //                    null);
    //            }

    //            string documentContents;
    //            using (Stream receiveStream = HttpContext.Current.Request.InputStream)
    //            {
    //                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
    //                {
    //                    documentContents = readStream.ReadToEnd();
    //                }
    //            }

    //            _logHelper.WriteDebug("Request Body: " + documentContents);

    //            if (_headerKeys != null && _headerKeys.Count > 0)
    //            {
    //                _headerKeyValue = new Dictionary<string, string>();

    //                var reqHeader = HttpContext.Current.Request.Headers;
    //                if (reqHeader == null)
    //                {
    //                    _logHelper.WriteInfo("Unable to capture values in null header.");
    //                    return new Tuple<string, string, Dictionary<string, string>>(
    //                        "500",
    //                        "Unable to capture values in header.",
    //                        null);
    //                }
    //                _logHelper.WriteDebug("Request Header: " + Convert.ToString(reqHeader));


    //                foreach (string item in _headerKeys)
    //                {
    //                    if (reqHeader[item] == null || reqHeader[item].Length == 0)
    //                    {
    //                        //_logHelper.WriteInfo(string.Format("{0} key is missing in the request.", item));
    //                        //_returnValue = new Tuple<string, string, Dictionary<string, string>>(
    //                        //    "400",
    //                        //    string.Format("{0} key is missing in the request.", item),
    //                        //    null);
    //                        //break;
    //                        continue;
    //                    }
    //                    if (item.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        _headerKeyValue.Add(item, reqHeader[item].Replace("Bearer", "").Replace("bearer", "").Trim());
    //                    }
    //                    else
    //                    {
    //                        _headerKeyValue.Add(item, reqHeader[item].Trim());
    //                    }
    //                }

    //                if (_returnValue != null)
    //                {
    //                    return _returnValue;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _logHelper.WriteError(ex, "Error occured while capturing request.");
    //            return new Tuple<string, string, Dictionary<string, string>>(
    //                "500",
    //                ex.Message,
    //                null);
    //        }

    //        return new Tuple<string, string, Dictionary<string, string>>(
    //                "200",
    //                "Success",
    //                _headerKeyValue);
    //    }
    //}
}
