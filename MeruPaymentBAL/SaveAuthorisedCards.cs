using MeruCommonLibrary;
using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL;
using MeruPaymentDAL.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class SaveAuthorisedCards
    {
        private LogHelper _logHelper;
        private Razorpay _razorpay;

        public SaveAuthorisedCards()
        {
            _logHelper = new LogHelper("SaveAuthorisedCards()");
            _razorpay = new Razorpay();
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(string customerId, string paymentId)
        {
            _logHelper.MethodName = "ProcessRequest(string customerId, string paymentId)";
            try
            {

                PaymentDAL paymentDAL = new PaymentDAL();
                PaymentBO paymentBO = paymentDAL.GetMeruPaymentDetail(paymentId);

                RazorpayPaymentBO razorpayPaymentBO = _razorpay.GetPaymentDetail(paymentBO.PaymentReferenceData2);

                //Tuple<string, string, List<CardBO>> returnCardValue = _razorpay.GetCardTokenDetails(customerId);

                Tuple<string, string, CardBO> returnCardValue = _razorpay.GetCardTokenDetail(customerId, razorpayPaymentBO.TokenId);

                bool enableCardBlocking = false;
                enableCardBlocking = Convert.ToBoolean(ConfigurationManager.AppSettings["enableCardBlocking"]);
                if (enableCardBlocking)
                {
                    using (ValidateCard cardValidator = new ValidateCard())
                    {
                        if (!cardValidator.IsValid(returnCardValue.Item3))
                        {
                            _logHelper.WriteWarn(String.Format("Card with issuer code: {0} , network {1} and Card Type {2} is blacklisted ", returnCardValue.Item3.Issuer, returnCardValue.Item3.Network, returnCardValue.Item3.CardType));

                            return new Tuple<string, string, Dictionary<string, string>>(
                            "400",
                            "Card is blacklisted.",
                            null);
                        }
                    }
                }

                AuthCardDAL authCardDAL = new AuthCardDAL();
                bool enableDuplicateCheck = false;
                enableDuplicateCheck = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableCardDuplicateCheck"]);
                if (enableDuplicateCheck)
                {
                    Tuple<string, string, CardBO> dbCardValue = authCardDAL.GetCardDetail(returnCardValue.Item3.Issuer, returnCardValue.Item3.Last4, returnCardValue.Item3.Network, returnCardValue.Item3.CardType, returnCardValue.Item3.ExpityDateTime);
                    if (dbCardValue.Item1 == "200")
                    {
                        _logHelper.WriteWarn("Repeated attempt to add card. Issuer: " + returnCardValue.Item3.Issuer +
                            "Last four: " + returnCardValue.Item3.Last4 +
                            "Network: " + returnCardValue.Item3.Network +
                            "Card Type: " + returnCardValue.Item3.CardType +
                            "customerId: " + customerId);
                        return new Tuple<string, string, Dictionary<string, string>>(
                        "400",
                        "Duplicate card.",
                        null);
                    }
                }

                AES_Encryption aES_Encryption = new AES_Encryption();
                string eCardToken = aES_Encryption.Encrypt(returnCardValue.Item3.PGCardTokenId);

                returnCardValue.Item3.PGCardTokenId = eCardToken;


                authCardDAL.SaveCard(returnCardValue.Item3);
                //foreach (CardBO item in returnCardValue.Item3)
                //{
                //    item.DeviceId = paymentBO.DeviceId;
                //    authCardDAL.SaveCard(item);
                //}

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    null);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while cancellaing payment.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
