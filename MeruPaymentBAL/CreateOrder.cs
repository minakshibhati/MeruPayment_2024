using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class CreateOrder
    {
        private LogHelper _logHelper;
        private Tuple<string, string, Dictionary<string, string>> returnValue = null;
        private OrderDAL _orderDAL = null;
        private Razorpay _razorpay = null;
        private string _razorpayOrderId = "";

        public CreateOrder()
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
       /*added on 22-02-2023 for Razorpay SDK integration for driverapp
       create Order*/
        public Tuple<string, string, Dictionary<string, string>> ProcessRequest_DA(OrderBORazorPay orderBO)
        {
           
            _logHelper.MethodName = "ProcessRequest_DA(OrderBO orderBO)";
            string OrderId = "", PaymentId = "";
            try
            {
                LogHelper objLogger = new LogHelper("RazorPaymentAPIController");
                #region GET SOURCE DETAIL

                SourceDetail sourceDetail = new SourceDetail();
                Tuple<string, string, Dictionary<string, string>> _returnSourceValue = sourceDetail.BySourceName(orderBO.appsource);

                #endregion
                var obj = new
                {
                   id = orderBO.id.ToString(),
                   amount=orderBO.amount.ToString(),
                   createdate=(orderBO.created_at).ToString(),
                   mobile_number=orderBO.mobile_number.ToString()

                };
                //var json = JsonSerializer.Serialize(obj);
                //Console.WriteLine(json);
                string jsonobj = JsonConvert.SerializeObject(obj);
                
                #region CHECKSUM VALIDATION

                ChecksumValidation checksum = new ChecksumValidation();
                if (!checksum.Validate(_returnSourceValue.Item3["AppSecret"]+ orderBO.mobile_number, orderBO.checksum, jsonobj))
                {
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "400",
                    "Checksum validation failed.",
                    null
                    );
                    _logHelper.WriteInfo(string.Format("Checksum validation failed"));

                }

                #endregion

                if (orderBO.amount < 100)
                {
                    _logHelper.WriteInfo(string.Format("Amount cannot be less than 1 rupee. Contact {0}"));
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    string.Format("Amount cannot be less than 1 rupee. Contact {0}", orderBO.status),
                    null);
                }

                returnValue = _orderDAL.CreateOrder_DA(orderBO);

                if (returnValue.Item1 != "200")
                {
                    return returnValue;
                };
                OrderId = returnValue.Item3["OrderId"];
                PaymentId = returnValue.Item3["PaymentId"];
           
                Dictionary<string, string> returnData = new Dictionary<string, string>();
               // returnData.Add("RazorpayOrderId", _razorpayOrderId);
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

    }
}
