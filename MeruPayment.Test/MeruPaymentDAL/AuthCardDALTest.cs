using MeruPaymentBO;
using MeruPaymentDAL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentDAL
{
    [TestFixture]
    class AuthCardDALTest
    {
        private AuthCardDAL authCardDAL = null;
        public AuthCardDALTest()
        {
            authCardDAL = new AuthCardDAL();
        }

        [TestCase("9833240124", ExpectedResult = "200")]
        public string GetLatestValidCardByMobile_Test(string mobile)
        {
            Tuple<string, string, CardBO> returnValue = authCardDAL.GetLatestValidCardByMobile(mobile);
            return returnValue.Item1;
        }

        [TestCase("KKBK", "3960", "Visa", "credit", "2019-04-15 10:24:40.957", ExpectedResult = "200")]
        [TestCase("KKBK", "1111", "Master Card", "credit", "2019-04-15 10:24:40.957", ExpectedResult = "500")]
        public string GetCardDetail_Test(string issurcode, string lastfour, string network, string cardtype, DateTime cardExpiryDateTime)
        {
            Tuple<string, string, CardBO> returnValue = authCardDAL.GetCardDetail(issurcode, lastfour, network, cardtype, cardExpiryDateTime);
            return returnValue.Item1;
        }
    }
}
