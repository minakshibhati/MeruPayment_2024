using AutoMapper;
using MeruCommonLibrary;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MeruPaymentBAL
{
    public class CardAuthCheckoutBAL
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> _returnValue = null;

        public CardAuthCheckoutBAL()
        {
            _logHelper = new LogHelper("CardAuthCheckoutBAL");
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(ref CardAuthCheckoutBO cardAuthCheckoutBO)
        {
            _logHelper.MethodName = "ProcessRequest()";

            try
            {
                cardAuthCheckoutBO.Amount = Convert.ToInt32(ConfigurationManager.AppSettings["CardAuthenticationCharge"]);
                cardAuthCheckoutBO.PaymentMethod = MeruPaymentBO.PaymentMethod.card;
                cardAuthCheckoutBO.OrderType = "CARD_AUTH";

                #region GET SOURCE DETAIL

                SourceDetail sourceDetail = new SourceDetail();
                _returnValue = sourceDetail.BySourceName(cardAuthCheckoutBO.AppSource);
                if (_returnValue.Item1 != "200")
                {
                    return _returnValue;
                }
                cardAuthCheckoutBO.AppReturnURL = Convert.ToString(_returnValue.Item3["AppReturnURL"]);
                cardAuthCheckoutBO.AppSecret = Convert.ToString(_returnValue.Item3["AppSecret"]);
                cardAuthCheckoutBO.EnableAuthToken = Convert.ToBoolean(_returnValue.Item3["EnableAuthToken"]);

                #endregion

                #region CHECKSUM VALIDATION

                ChecksumValidation checksum = new ChecksumValidation();
                if (!checksum.Validate(cardAuthCheckoutBO.AppSecret, cardAuthCheckoutBO.Checksum, cardAuthCheckoutBO.RawRequest))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "400",
                    "Checksum validation failed.",
                    null);
                }

                #endregion

                #region AUTH TOKEN VALIDATION

                if (cardAuthCheckoutBO.EnableAuthToken)
                {
                    AuthorizationValidation authorization = new AuthorizationValidation();
                    if (!authorization.Validate(cardAuthCheckoutBO.Contact, cardAuthCheckoutBO.AuthToken))
                    {
                        return new Tuple<string, string, Dictionary<string, string>>(
                        "401",
                        "Auth token validation failed.",
                        null);
                    }
                }

                #endregion

                #region CREATE CUSTOMER

                CreateCustomer createCustomer = new CreateCustomer();
                _returnValue = createCustomer.ProcessRequest(new CustomerBO
                {
                    Contact = cardAuthCheckoutBO.Contact,
                    Email = cardAuthCheckoutBO.Email,
                    PaymentGateway = PaymentGatway.Razorpay,
                    FullName = cardAuthCheckoutBO.FullName
                });

                if (_returnValue.Item1 != "200")
                {
                    return _returnValue;
                }

                #endregion

                #region OBJECT MAPPING

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<CardAuthCheckoutBO, OrderBO>();
                });

                IMapper iMapper = config.CreateMapper();

                OrderBO orderBO = iMapper.Map<CardAuthCheckoutBO, OrderBO>(cardAuthCheckoutBO);

                #endregion

                #region CREATE ORDER

                CreateOrder createOrder = new CreateOrder();
                return createOrder.ProcessRequest(orderBO);

                #endregion
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing card capturing request.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
