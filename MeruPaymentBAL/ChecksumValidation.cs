using MeruCommonLibrary;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class ChecksumValidation
    {
        private LogHelper _logHelper;

        public ChecksumValidation()
        {
            _logHelper = new LogHelper("Checksum");
        }

        public bool Validate(string secretKey, string checksum, string value1, string value2 = "", string value3 = "")
        {
            _logHelper.MethodName = "Validate(string secretKey, string checksum, string value1, string value2 = \"\", string value3 = \"\")";

            string Value = "";

            if (value1.Length > 0)
            {
                Value += value1;
            }

            if (value2.Length > 0)
            {
                Value += string.Format("|{0}", value2);
            }

            if (value3.Length > 0)
            {
                Value += string.Format("|{0}", value3);
            }

            using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secretKey)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(Value));

                StringBuilder builder = new StringBuilder();

                foreach (byte b in hash)
                    builder.AppendFormat("{0:x2}", b);

                string checksumBuild = builder.ToString();

                if (checksum != checksumBuild)
                {
                    _logHelper.WriteInfo(string.Format("Checksum validation failed received:{0} generated:{1}", checksum, checksumBuild));
                    return false;
                }

                return true;
            }
        }
    }
}
