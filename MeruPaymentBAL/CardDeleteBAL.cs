using MeruCommonLibrary;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class CardDeleteBAL
    {
        private LogHelper _logHelper;


        public CardDeleteBAL()
        {
            _logHelper = new LogHelper("CardAutoChargeBAL");
        }

        public Tuple<string, string, bool> ProcessRequest(CardDeleteBO cardDeleteBO)
        {
            try
            {
                #region GET SOURCE DETAIL

                SourceDetail sourceDetail = new SourceDetail();
                Tuple<string, string, Dictionary<string, string>> _returnSourceValue = sourceDetail.BySourceName(cardDeleteBO.AppSource);
                if (_returnSourceValue.Item1 != "200")
                {
                    return new Tuple<string, string, bool>(_returnSourceValue.Item1, _returnSourceValue.Item2, false);
                }
                cardDeleteBO.AppSecret = _returnSourceValue.Item3["AppSecret"];

                #endregion

                #region CHECKSUM VALIDATION

                ChecksumValidation checksum = new ChecksumValidation();
                if (!checksum.Validate(cardDeleteBO.AppSecret, cardDeleteBO.Checksum, cardDeleteBO.AuthToken, cardDeleteBO.Contact))
                {
                    return new Tuple<string, string, bool>(
                    "400",
                    "Checksum validation failed.",
                    false);
                }

                #endregion

                #region AUTH TOKEN VALIDATION

                AuthorizationValidation authorization = new AuthorizationValidation();
                if (!authorization.Validate(cardDeleteBO.Contact, cardDeleteBO.AuthToken))
                {
                    return new Tuple<string, string, bool>(
                    "401",
                    "Auth token validation failed.",
                    false);
                }

                #endregion

                #region GET AUTH CARD DETAIL

                GetAuthCard getAuthCard = new GetAuthCard();
                Tuple<string, string, CardBO> returnCard = getAuthCard.ByCardId(cardDeleteBO.CardId);
                if (returnCard.Item1 != "200")
                {
                    return new Tuple<string, string, bool>(
                        returnCard.Item1,
                        returnCard.Item2,
                        false);
                }

                cardDeleteBO.PaymentGateway = returnCard.Item3.PaymentGateway; //get the value from above

                #endregion

                #region DELETE CARD

                DeleteCard deleteCard = new DeleteCard();
                Tuple<string, string, bool> _returnValue = deleteCard.ProcessRequest(returnCard.Item3.PGCustomerId, returnCard.Item3.PGCardTokenId, cardDeleteBO.CardId);

                return _returnValue;

                #endregion

            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing auth card delete.");
                return new Tuple<string, string, bool>(
                    "500",
                    ex.Message,
                    false);
            }
        }
    }
}
