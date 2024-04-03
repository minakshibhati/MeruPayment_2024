using MeruPaymentCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentCore
{
    [TestFixture]
    public class PaytmTest
    {
        [Test]
        public void GenerateChecksum_Given_CorrectValue()
        {
            Paytm paytm = new Paytm();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("MID", "Meruca06356066993481");
            parameters.Add("CHANNEL_ID", "WAP");
            parameters.Add("INDUSTRY_TYPE_ID", "Travel");
            parameters.Add("WEBSITE", "");
            parameters.Add("EMAIL", "");
            parameters.Add("MOBILE_NO", "9705601049");
            parameters.Add("CUST_ID", "9911110_MH01CJ7200");
            parameters.Add("MERC_UNQ_REF", "9911110_MH01CJ7200");
            parameters.Add("ORDER_ID", "MP26218476384");
            parameters.Add("TXN_AMOUNT", "1");//
            parameters.Add("CALLBACK_URL", "https://manthandev.merucabs.com/MeruPaymentWeb/PaytmPaymentResponse");// //This parameter is not mandatory. Use this to pass the callback url dynamically.
            parameters.Add("PAYMENT_MODE_ONLY", "YES");
            parameters.Add("AUTH_MODE", "USRPWD"); //For Credit/Debit card - 3D and For Wallet, Net Banking – USRPWD
            parameters.Add("PAYMENT_TYPE_ID", "PPI"); //CC payment mode – CC | DC payment mode - DC | NB payment mode - NB | Paytm wallet – PPI | EMI - EMI | UPI - UPI

            string  checksum = paytm.GenerateChecksum(parameters);

            Assert.IsNotEmpty(checksum);
        }
    }
}
