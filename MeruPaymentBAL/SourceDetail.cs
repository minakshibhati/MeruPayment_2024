using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    internal class SourceDetail
    {
        private LogHelper _logHelper;
        private PaymentRequestSystemDAL paymentRequestSystemDAL = null;

        internal SourceDetail()
        {
            _logHelper = new LogHelper("SourceDetail()");
            paymentRequestSystemDAL = new PaymentRequestSystemDAL();
        }

        internal Tuple<string, string, Dictionary<string, string>> BySourceName(string sourceName)
        {
            _logHelper.MethodName = "BySourceName(string sourceName)";
            try
            {
                PaymentRequestSystemMasterBO paymentRequestSystemMasterBO = paymentRequestSystemDAL.GetDetailBySystemCode(sourceName);
                if (paymentRequestSystemMasterBO == null)
                {
                    _logHelper.WriteInfo(string.Format("Source {0} is either unknown or null", sourceName));
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "403",
                        string.Format("Source {0} is either unknown or null", sourceName),
                        null
                        );
                }

                Dictionary<string, string> returnValue = new Dictionary<string, string>();
                returnValue.Add("AppReturnURL", paymentRequestSystemMasterBO.ReturnURL);
                returnValue.Add("AppSecret", paymentRequestSystemMasterBO.SecretCode);
                returnValue.Add("AppColorCode", paymentRequestSystemMasterBO.ColorCode);
                returnValue.Add("EnableAuthToken", paymentRequestSystemMasterBO.EnableAuthToken);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error while getting source detail");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        internal Tuple<string, string, Dictionary<string, string>> ByPaymentId(string paymentId)
        {
            _logHelper.MethodName = "ByPaymentId(string paymentId)";
            try
            {
                PaymentDAL paymentDAL = new PaymentDAL();
                PaymentBO paymentBO = paymentDAL.GetMeruPaymentDetail(paymentId);
                if (paymentBO == null)
                {
                    _logHelper.WriteInfo(string.Format("Unable to find payment detail for payment id {0}", paymentId));
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        string.Format("Unable to find payment detail for payment id {0}", paymentId),
                        null
                        );
                }

                return BySourceName(paymentBO.RequestSource);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error while getting source detail");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
