using MeruCommonLibrary;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class AuthorizationValidation
    {
        private string _mobileNo;
        private string _authToken;
        private string _ATVURL;

        private LogHelper _logHelper;

        public AuthorizationValidation()
        {
            _logHelper = new LogHelper("AuthorizationValidation");
            _ATVURL = ConfigurationManager.AppSettings["ATVURL"];
        }

        public bool Validate(string mobileNo, string authToken)
        {
            _mobileNo = mobileNo;
            _authToken = authToken;

            _logHelper.MethodName = "Validate(string mobileNo, string authToken)";

            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            string responseData = string.Empty;

            string Url = string.Empty;
            string Params = string.Empty;
            string sResponse = string.Empty;

            try
            {
                Params = "MobileNo=" + _mobileNo + "&Token=" + _authToken;
                Url = _ATVURL + "?" + Params;
                _logHelper.WriteDebug("Calling authorization URL : " + Url);
                httpRequest = System.Net.WebRequest.Create(new Uri(Url)) as System.Net.HttpWebRequest;
                httpRequest.Method = "GET";
                httpRequest.ContentType = "application/text";
                httpRequest.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                try
                {
                    httpResponse = httpRequest.GetResponse() as System.Net.HttpWebResponse;
                    using (System.IO.Stream resStream = httpResponse.GetResponseStream())
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(resStream);
                        responseData = reader.ReadToEnd();
                        _logHelper.WriteDebug("Authorization response : " + responseData);
                        var jbook = JsonConvert.DeserializeObject(responseData) as dynamic;
                        sResponse = jbook.CheckTokenResult;
                        if (sResponse != "success")
                        {
                            //_logHelper.WriteInfo("Authtoken validation failed");
                            return false;
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)wex.Response)
                        {
                            using (var reader1 = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                responseData = reader1.ReadToEnd();

                                #region Error Log
                                _logHelper.WriteError(wex, "Web exception while authentication mobile: " + mobileNo + "response :" + responseData);
                                #endregion
                            }
                        }
                    }
                    if (wex != null)
                    {
                        _logHelper.WriteError(wex, "Web exception while authentication mobile:" + mobileNo);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Exception while authentication mobile:" + mobileNo);
                return false;
            }
            return true;
        }

    }
}
