using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class CardAutoChargeBAL
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> _returnValue = null;
        private bool _enableAuthToken = true;

        public CardAutoChargeBAL()
        {
            _logHelper = new LogHelper("CardAutoChargeBAL");
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(CardAutoChargeBO _cardAutoChargeBO)
        {
            _logHelper.MethodName = "ProcessRequest()";
            Dictionary<string, string> returnData = null;

            try
            {
                #region GET SOURCE DETAIL

                SourceDetail sourceDetail = new SourceDetail();
                _returnValue = sourceDetail.BySourceName(_cardAutoChargeBO.AppSource);
                if (_returnValue.Item1 != "200")
                {
                    return _returnValue;
                }
                _cardAutoChargeBO.AppReturnURL = _returnValue.Item3["AppReturnURL"];
                _cardAutoChargeBO.AppSecret = _returnValue.Item3["AppSecret"];
                _enableAuthToken = Convert.ToBoolean(_returnValue.Item3["EnableAuthToken"]);

                #endregion

                #region CHECKSUM VALIDATION

                ChecksumValidation checksum = new ChecksumValidation();
                if (!checksum.Validate(_cardAutoChargeBO.AppSecret, _cardAutoChargeBO.Checksum, _cardAutoChargeBO.RawRequest))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "400",
                    "Checksum validation failed.",
                    null);
                }

                #endregion

                #region AUTH TOKEN VALIDATION

                if (_enableAuthToken)
                {
                    AuthorizationValidation authorization = new AuthorizationValidation();
                    if (!authorization.Validate(_cardAutoChargeBO.Contact, _cardAutoChargeBO.AuthToken))
                    {
                        return new Tuple<string, string, Dictionary<string, string>>(
                        "401",
                        "Auth token validation failed.",
                        null);
                    }
                }

                #endregion

                //TODO: IP VALIDATION

                #region VALIDATE PRE CHARGE

                PreChargeValidation appRequestValidation = new PreChargeValidation();
                Tuple<string, string, bool> cardValidate = appRequestValidation.Validate(_cardAutoChargeBO.PaymentType, _cardAutoChargeBO.AppRequestId, _cardAutoChargeBO.Amount, _cardAutoChargeBO.CardId, _cardAutoChargeBO.Contact);
                if (cardValidate.Item1 != "200")
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                        cardValidate.Item1,
                        cardValidate.Item2,
                        null);
                }

                #endregion

                #region GET AUTH CARD DETAIL

                GetAuthCard getAuthCard = new GetAuthCard();
                Tuple<string, string, CardBO> returnCard = getAuthCard.ByValidCardId_Mobile(_cardAutoChargeBO.CardId, _cardAutoChargeBO.Contact);
                if (returnCard.Item1 != "200")
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                        returnCard.Item1,
                        returnCard.Item2,
                        null);
                }

                _cardAutoChargeBO.PaymentGateway = returnCard.Item3.PaymentGateway; //get the value from above
                PaymentMethod pm = PaymentMethod.card;
                if (returnCard.Item3.CardType == "credit")
                {
                    pm = PaymentMethod.credit;
                }

                if (returnCard.Item3.CardType == "debit")
                {
                    pm = PaymentMethod.debit;
                }

                #endregion

                //#region VALIDATE AUTH CARD

                //AuthCardValidation authCardValidation = new AuthCardValidation();
                //Tuple<string, string, bool> cardValidate = authCardValidation.Validate();
                //if (cardValidate.Item1 != "200")
                //{
                //    return new Tuple<string, string, Dictionary<string, string>>(
                //        cardValidate.Item1,
                //        cardValidate.Item2,
                //        null);
                //}

                //#endregion

                #region GET CUSTOMER DETAIL

                GetCustomer getCustomer = new GetCustomer();
                Tuple<string, string, CustomerBO> returnCustomerValue = getCustomer.ByMobileNo(_cardAutoChargeBO.Contact, _cardAutoChargeBO.PaymentGateway);
                if (returnCustomerValue.Item1 != "200")
                {
                    return new Tuple<string, string, Dictionary<string, string>>("500", returnCustomerValue.Item2, null);
                }

                #endregion

                #region CREATE ORDER

                CreateOrder createOrder = new CreateOrder();
                Tuple<string, string, Dictionary<string, string>> returnOrder = createOrder.ProcessRequest(new OrderBO
                {
                    Amount = _cardAutoChargeBO.Amount,
                    AppRequestId = _cardAutoChargeBO.AppRequestId,
                    AppSource = _cardAutoChargeBO.AppSource,
                    Contact = _cardAutoChargeBO.Contact,
                    Desc = _cardAutoChargeBO.Desc,
                    Email = _cardAutoChargeBO.Email,
                    FullName = returnCustomerValue.Item3.FullName,
                    OrderType = _cardAutoChargeBO.PaymentType, //"AUTOCHARGE",
                    //PaymentId = _cardAutoChargeBO.PaymentId,
                    //PGOrderId = _cardAutoChargeBO.PGOrderId,
                    PaymentMethod = pm,
                    PaymentMethodRefId = _cardAutoChargeBO.CardId
                });

                #endregion

                #region AUTO CHARGE

                if (returnOrder.Item1 != "200")
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    returnOrder.Item2,
                    null);
                }

                returnData = new Dictionary<string, string>();
                returnData.Add("PaymentId", returnOrder.Item3["PaymentId"]);

                AutoChargeBAL autoChargeBAL = new AutoChargeBAL();
                Tuple<string, string, Dictionary<string, string>> returnPayment = autoChargeBAL.ProcessRequest(returnOrder.Item3["PaymentId"],
                    _cardAutoChargeBO.AppRequestId,
                    returnOrder.Item3["RazorpayOrderId"],
                    returnCard.Item3.PGCardTokenId,
                    returnCard.Item3.PGCustomerId,
                    _cardAutoChargeBO.Amount,
                    _cardAutoChargeBO.Email,
                    _cardAutoChargeBO.Contact, _cardAutoChargeBO.Desc);

                if (returnPayment.Item1 != "200")
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    returnPayment.Item2,
                    returnData);
                }

                #endregion

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnData);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing auto charge request.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
