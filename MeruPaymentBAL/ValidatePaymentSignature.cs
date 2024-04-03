using java.security;
using javax.crypto;
using javax.crypto.spec;
using javax.xml.bind;
using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL;
using Newtonsoft.Json;
//using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class ValidatePaymentSignature
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private OrderDAL _orderDAL = null;
        private Razorpay _razorpay = null;
        private string _razorpayOrderId = "";
        private static  String HMAC_SHA256_ALGORITHM = "HmacSHA256";
        public ValidatePaymentSignature()
        {
            _logHelper = new LogHelper("CreateOrder()");
            _orderDAL = new OrderDAL();
            _razorpay = new Razorpay();
        }

        public Tuple<string, string, Dictionary<string, string>> ProcessRequest(OrderBO orderBO)
        {
            _logHelper.MethodName = "ProcessRequest(OrderBO orderBO)";
            string OrderId = "", PaymentId = "";
            try
            {
                if (orderBO.Amount < 100)
                {
                    _logHelper.WriteInfo(string.Format("Amount cannot be less than 1 rupee. Contact {0}", orderBO.Contact));
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    string.Format("Amount cannot be less than 1 rupee. Contact {0}", orderBO.Contact),
                    null);
                }

                returnValue = _orderDAL.CreateOrder(orderBO);

                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                };
                OrderId = returnValue.Item3["OrderId"];
                PaymentId = returnValue.Item3["PaymentId"];
                _razorpayOrderId = returnValue.Item3["PGOrderId"];

                if (_razorpayOrderId == null || _razorpayOrderId.Length == 0)
                {
                    _razorpayOrderId = _razorpay.CreateOrder(orderBO.AppRequestId, orderBO.Amount.ToString());
                    if (_razorpayOrderId.Length == 0)
                    {
                        return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Order creation failed at razorpay",
                        null);
                    }
                }

                returnValue = _orderDAL.UpdatePGOrderId(returnValue.Item3["PaymentId"], _razorpayOrderId);
                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                }

                Dictionary<string, string> returnData = new Dictionary<string, string>();
                returnData.Add("RazorpayOrderId", _razorpayOrderId);
                returnData.Add("OrderId", OrderId);
                returnData.Add("PaymentId", PaymentId);

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnData);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while creating order.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }


        public bool verifyPayment(SuccessCallBackResponse resp,string key_secret)
        {

            #region GET SOURCE DETAIL

            SourceDetail sourceDetail = new SourceDetail();
            Tuple<string, string, Dictionary<string, string>> _returnSourceValue = sourceDetail.BySourceName(resp.appsource.ToString());

            #endregion
            var obj = new
            {
                id = resp.razorpay_order_id.ToString(),
                app_generated_id = resp.Order_Id.ToString(),
                mobile_number = resp.mobilenum.ToString()

            };
            //var json = JsonSerializer.Serialize(obj);
            //Console.WriteLine(json);
            string jsonobj = JsonConvert.SerializeObject(obj);

            #region CHECKSUM VALIDATION

            ChecksumValidation checksum = new ChecksumValidation();
            if (!checksum.Validate(_returnSourceValue.Item3["AppSecret"] + resp.mobilenum, resp.CheckSum, jsonobj))
            {
                

                return false;
            }

            #endregion

            CommonMethods objCommonMethods = new CommonMethods();
            //return objCommonMethods.ValidateData_HMACSHAH256(resp.razorpay_signature, resp.Order_Id+ "|" + resp.razorpay_payment_id, key_secret);
            return objCommonMethods.ValidateData_HMACSHAH256(resp.razorpay_signature, resp.razorpay_order_id+ "|" + resp.razorpay_payment_id, key_secret);

            var generated_signature = hmac_sha256(resp.razorpay_order_id + "|" + resp.razorpay_payment_id, key_secret);

            if (generated_signature == resp.razorpay_signature)
            {
                return true;
            }
            return false;


        }

        private string  hmac_sha256(string data, string key_secret)
        {
            string result = "";
            try
            {
                SecretKeySpec signingKey = new SecretKeySpec(Encoding.ASCII.GetBytes(key_secret), HMAC_SHA256_ALGORITHM);
                Mac mac = Mac.getInstance(HMAC_SHA256_ALGORITHM);
                mac.init(signingKey);
                byte[] rawHmac = mac.doFinal(Encoding.ASCII.GetBytes(data));
                result = DatatypeConverter.printHexBinary(rawHmac).ToLower();


            }
            catch (Exception e)
            {
                throw new SignatureException("Failed to generate HMAC : " + e.Message);
            }


            return result;
        }
    }
}
