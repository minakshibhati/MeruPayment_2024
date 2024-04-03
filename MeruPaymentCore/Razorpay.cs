using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Razorpay.Api;
using System.Configuration;
using System.Net;
using MeruPaymentBO;
using NLog;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using MeruCommonLibrary;
using Newtonsoft.Json.Linq;

namespace MeruPaymentCore
{
    internal class RazorpayPaymentResponseDTO
    {
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
    }

    internal class RazorpayCardAuthRequetDTO
    {
        public string email { get; set; }
        public string contact { get; set; }
        public string currency { get; set; }
        public string order_id { get; set; }
        public string amount { get; set; }
        public string customer_id { get; set; }
        public string token { get; set; }
        public string recurring { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> notes { get; set; }
    }

    public class Razorpay : IDisposable
    {
        private string key;
        private string secret;
        private RazorpayClient client;
        private RazorpayPaymentBO objRazorpayPaymentBO = null;
        private RazorpayOrderBO objRazorpayOrderBO = null;
        private RazorpayCardBO objRazorpayCardBO = null;
        private Payment payment;
        private Order order;
        private LogHelper objLogger;
        private bool disposed = false;
        private string _razorpayUrl;

        public Razorpay()
        {
            key = ConfigurationManager.AppSettings["Razor_Key_Id"];
            secret = ConfigurationManager.AppSettings["Razor_Key_Secret"];
            _razorpayUrl = ConfigurationManager.AppSettings["Razor_Url"];

            client = new RazorpayClient(key, secret);
            objLogger = new LogHelper("Razorpay");

            objRazorpayPaymentBO = null;
            objRazorpayCardBO = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        ~Razorpay()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //Free managed resources
                    //LogManager.Flush();
                    client = null;
                    objRazorpayPaymentBO = null;
                    objRazorpayCardBO = null;
                }
                // Free native or unmanaged resources
                disposed = true;
            }
        }

        public RazorpayRefundBO RefundPayment(string PaymentId, Int64 RefundAmount, bool IsPartialRefund, string Note)
        {
            RazorpayRefundBO objRazorpayRefundBO = null;
            try
            {
                Payment payment = client.Payment.Fetch(PaymentId);
                Refund refund;
                if (!IsPartialRefund)
                {
                    refund = payment.Refund();
                }
                else
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("amount", RefundAmount);
                    if (Note.Length > 0)
                    {
                        Dictionary<string, string> dctNotes = new Dictionary<string, string>();
                        dctNotes.Add("Reason", Note);
                        data.Add("notes", dctNotes);
                    }

                    refund = payment.Refund(data);
                }

                if (refund != null)
                {
                    objRazorpayRefundBO = new RazorpayRefundBO();
                    if (refund["error"] != null)
                    {
                        if (refund["error"]["code"] != null)
                        {
                            objRazorpayRefundBO.ErrorCode = refund["error"]["code"];
                            objRazorpayRefundBO.ErrorDescription = refund["error"]["description"];
                        }
                    }

                    if (objRazorpayRefundBO.ErrorCode == null || objRazorpayRefundBO.ErrorCode.Length == 0)
                    {
                        objRazorpayRefundBO.RefundId = refund["id"];
                        objRazorpayRefundBO.PaymentId = refund["payment_id"];
                        objRazorpayRefundBO.RefundAmount = Convert.ToInt64(refund["amount"]);
                        if (objRazorpayRefundBO.RefundAmount != RefundAmount)
                        {
                            objRazorpayRefundBO.RefundAmount = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured during payment refund at razorpay.");
            }
            return objRazorpayRefundBO;
        }

        public string CreateOrder(string ReceiptNumber, string Amount)
        {
            string OrderId = "CreateOrder(string ReceiptNumber, string Amount)";
            try
            {
                objLogger.MethodName = "";
                Dictionary<string, object> input = new Dictionary<string, object>();
                input.Add("amount", Amount);
                input.Add("currency", "INR");
                input.Add("receipt", ReceiptNumber);
                input.Add("payment_capture", 1);
                //Log Data posted to Razorpay
                objLogger.WriteInfo("Data posted to Razor pay in CreateOrder Method is : " + Newtonsoft.Json.JsonConvert.SerializeObject(input, Formatting.None));
                Order order = client.Order.Create(input);
                OrderId = order["id"].ToString();
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while creating order at razorpay.");
            }
            return OrderId;
        }

        //public RazorpayOrderBO CreateOrderAutoCapture(string ReceiptNumber, string Amount, out string returnMessage)
        //{
        //    returnMessage = "";
        //    try
        //    {
        //        if (ReceiptNumber.Length > 40)
        //        {
        //            returnMessage = "Receipt number cannot be greater than 40 in length.";
        //            objLogger.WriteInfo(string.Format(returnMessage + " receipt no. {0}", ReceiptNumber));
        //            return null;
        //        }

        //        Dictionary<string, object> input = new Dictionary<string, object>();
        //        input.Add("amount", Amount);
        //        input.Add("currency", "INR");
        //        input.Add("receipt", ReceiptNumber); // Maximum length 40 chars
        //        input.Add("payment_capture", 1);
        //        Order order = client.Order.Create(input);

        //        if (order == null)
        //        {
        //            returnMessage = "Create order failed at razorpay.";
        //            objLogger.WriteInfo(string.Format(returnMessage + " receipt no. {0}", ReceiptNumber));
        //            return null;
        //        }

        //        return new RazorpayOrderBO
        //        {
        //            Attempts = order["attempts"],
        //            OrderAmount = order["amount"],
        //            OrderId = order["id"],
        //            ReceiptId = order["receipt"],
        //            Status = GetOrderStatus(order["status"].ToString())
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        returnMessage = ex.Message;
        //        objLogger.WriteError(ex, string.Format("Error occured while creating order at razorpay for receipt no {0}", ReceiptNumber));
        //        return null;
        //    }
        //}

        private PaymentMethod GetPaymentMethod(string CardId, string Wallet, string Bank, string VPA)
        {
            try
            {
                if (CardId != null && CardId.Length > 0)
                {
                    return PaymentMethod.card;
                }

                if (Wallet != null && Wallet.Length > 0)
                {
                    return PaymentMethod.wallet;
                }

                if (Bank != null && Bank.Length > 0)
                {
                    return PaymentMethod.netbanking;
                }

                if (VPA != null && VPA.Length > 0)
                {
                    return PaymentMethod.upi;
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting payment method.");
            }
            return PaymentMethod.Unknown;
        }

        public RazorpayPaymentBO GetPaymentDetail(string PaymentId)
        {
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "GetPaymentDetail(string PaymentId)";
                objLogger.WriteInfo("Data posted to Razor pay in GetPaymentDetail Method is : " + PaymentId);
                payment = client.Payment.Fetch(PaymentId);

                if (payment != null)
                {
                    objRazorpayPaymentBO = new RazorpayPaymentBO();
                    objRazorpayPaymentBO.Amount = Convert.ToString(payment["amount"]);
                    objRazorpayPaymentBO.OrderId = Convert.ToString(payment["order_id"]);
                    objRazorpayPaymentBO.PaymentMethod = GetPaymentMethod(Convert.ToString(payment["card_id"]), Convert.ToString(payment["wallet"]), Convert.ToString(payment["bank"]), Convert.ToString(payment["vpa"]));
                    objRazorpayPaymentBO.PaymentMethodDetail = GetPaymentMethodDetail(payment);//objRazorpayPaymentBO.PaymentMethod);
                    objRazorpayPaymentBO.PaymentStatus = GetPaymentStatus(Convert.ToString(payment["status"]));
                    objRazorpayPaymentBO.TokenId = Convert.ToString(payment["token_id"]);
                    objRazorpayPaymentBO.PaymentId = Convert.ToString(payment["id"]);
                    objRazorpayPaymentBO.ErrorCode = Convert.ToString(payment["error_code"]); ;
                    objRazorpayPaymentBO.ErrorDescription = Convert.ToString(payment["error_description"]);
                    objRazorpayPaymentBO.AmountRefunded = Convert.ToString(payment["amount_refunded"]);
                    objRazorpayPaymentBO.RefundStatus = Convert.ToString(payment["refund_status"]);
                    objRazorpayPaymentBO.CustomerId = Convert.ToString(payment["customer_id"]);
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting payment details from razorpay.");
            }

            return objRazorpayPaymentBO;
        }

        public OrderStatus GetOrderStatus(string status)
        {
            OrderStatus orderStatus = OrderStatus.OrderCreated;
            switch (status.ToLower())
            {
                case "created":
                    orderStatus = OrderStatus.OrderCreated;
                    break;
                case "attempted":
                    orderStatus = OrderStatus.OrderAttempted;
                    break;
                case "paid":
                    orderStatus = OrderStatus.OrderPaid;
                    break;
            }
            return orderStatus;
        }

        public RazorpayOrderBO GetOrderDetail(string OrderId)
        {
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "GetOrderDetail(string OrderId)";
                objLogger.WriteInfo("Data posted to Razor pay in GetOrderDetail Method is to fetch Order is : " + OrderId);
                order = client.Order.Fetch(OrderId);
                if (order != null)
                {
                    string status = order["status"].Value;
                    objRazorpayOrderBO = new RazorpayOrderBO
                    {
                        Attempts = order["attempts"].Value,
                        OrderAmount = order["amount"].Value,
                        OrderId = order["id"].Value,
                        ReceiptId = order["receipt"].Value
                    };

                    objRazorpayOrderBO.Status = GetOrderStatus(status);
                }

               
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting order detail from razorpay.");
            }
            return objRazorpayOrderBO;
        }

        private PaymentStatus GetPaymentStatus(string Status)
        {
            PaymentStatus objStatus = PaymentStatus.PaymentUnkown;
            switch (Status)
            {
                case "created":
                    objStatus = PaymentStatus.PaymentInitiated;
                    break;
                case "authorized":
                    objStatus = PaymentStatus.PaymentAuthorised;
                    break;
                case "captured":
                case "paid":
                    objStatus = PaymentStatus.PaymentSuccess;
                    break;
                case "refunded":
                    objStatus = PaymentStatus.PaymentRefunded;
                    break;
                case "failed":
                    objStatus = PaymentStatus.PaymentFailed;
                    break;
            }
            return objStatus;
        }

        public RazorpayPaymentBO GetPaymentDetailByOrderId(string OrderId)
        {
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "GetPaymentDetailByOrderId(string OrderId)";
                objLogger.WriteInfo("Data posted to Razor pay in GetPaymentDetailByOrderId Method is : " + OrderId);
                order = client.Order.Fetch(OrderId);

                if (order == null)
                {
                    return null;
                }
                //objLogger.WriteInfo(Newtonsoft.Json.JsonConvert.SerializeObject(order.Payments()));
                Payment payment = order.Payments().Where(a => a["status"] == "captured").FirstOrDefault();
                if (payment == null)
                {
                    payment = order.Payments().FirstOrDefault();
                }

                if (payment == null)
                {
                    return null;
                }

                objRazorpayPaymentBO = new RazorpayPaymentBO();
                objRazorpayPaymentBO.PaymentId = Convert.ToString(payment["id"]);
                objRazorpayPaymentBO.Amount = Convert.ToString(payment["amount"]);
                objRazorpayPaymentBO.OrderId = Convert.ToString(payment["order_id"]);
                objRazorpayPaymentBO.PaymentMethod = GetPaymentMethod(Convert.ToString(payment["card_id"]), Convert.ToString(payment["wallet"]), Convert.ToString(payment["bank"]), Convert.ToString(payment["vpa"]));
                objRazorpayPaymentBO.PaymentMethodDetail = GetPaymentMethodDetail(payment);//objRazorpayPaymentBO.PaymentMethod, payment);
                objRazorpayPaymentBO.PaymentStatus = GetPaymentStatus(Convert.ToString(payment["status"]));
                objRazorpayPaymentBO.ErrorCode = Convert.ToString(payment["error_code"]); ;
                objRazorpayPaymentBO.ErrorDescription = Convert.ToString(payment["error_description"]); ;
                objRazorpayPaymentBO.AmountRefunded = Convert.ToString(payment["amount_refunded"]);
                objRazorpayPaymentBO.RefundStatus = Convert.ToString(payment["refund_status"]);
                objRazorpayPaymentBO.TokenId = Convert.ToString(payment["token_id"]);
                objRazorpayPaymentBO.CustomerId = Convert.ToString(payment["customer_id"]);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occoured while getting payment detail using order id " + OrderId + " from razorpay.");
            }

            return objRazorpayPaymentBO;
        }

        private string GetPaymentMethodDetail(dynamic payment)
        {
            string returnValue = "Unknown";

            try
            {
                PaymentMethod paymentMethod = GetPaymentMethod(Convert.ToString(payment["card_id"]), Convert.ToString(payment["wallet"]),
                Convert.ToString(payment["bank"]), Convert.ToString(payment["vpa"]));

                switch (paymentMethod)
                {
                    case PaymentMethod.card:
                    case PaymentMethod.debit:
                    case PaymentMethod.credit:
                        returnValue = Convert.ToString(payment["card_id"]);
                        break;
                    case PaymentMethod.netbanking:
                        returnValue = Convert.ToString(payment["bank"]);
                        break;
                    case PaymentMethod.wallet:
                        returnValue = Convert.ToString(payment["wallet"]);
                        break;
                    case PaymentMethod.upi:
                        returnValue = Convert.ToString(payment["vpa"]);
                        break;
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting payment method detail.");
            }

            return returnValue;

        }

        public RazorpayCardBO GetCardDetail(string CardId)
        {
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "GetCardDetail(string CardId)";
                objLogger.WriteInfo("Data posted to Razor pay in GetCardDetail Method is : " + CardId);
                Card card = client.Card.Fetch(CardId);
                if (card != null)
                {
                    objRazorpayCardBO = new RazorpayCardBO();

                    objRazorpayCardBO.CardId = Convert.ToString(card["id"]); //Values expected: credit, debit, unknown
                    objRazorpayCardBO.CardType = Convert.ToString(card["type"]);
                    objRazorpayCardBO.FullName = Convert.ToString(card["name"]);
                    objRazorpayCardBO.Last4 = Convert.ToString(card["last4"]);
                    objRazorpayCardBO.Network = Convert.ToString(card["network"]); //Values expected : American Express, Diners Club, Discover, JCB, Maestro, MasterCard, RuPay, Unknown, Visa, Union Pay
                    objRazorpayCardBO.Issuer = Convert.ToString(card["issuer"]);
                    objRazorpayCardBO.IsInternational = Convert.ToBoolean(card["international"]);
                    objRazorpayCardBO.IsEMI = Convert.ToBoolean(card["emi"]);
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting card detail from razorpay.");
            }

            return objRazorpayCardBO;
        }

        //public List<RazorpayCardBO> GetCardDetail(string customerId)
        //{
        //    try
        //    {
        //        Card card = client.Card.Fetch(CardId);
        //        if (card != null)
        //        {
        //            objRazorpayCardBO = new RazorpayCardBO();

        //            //Values expected: credit, debit, unknown
        //            objRazorpayCardBO.CardId = Convert.ToString(card["id"]);
        //            objRazorpayCardBO.CardType = Convert.ToString(card["type"]);
        //            objRazorpayCardBO.FullName = Convert.ToString(card["name"]);
        //            objRazorpayCardBO.Last4 = Convert.ToString(card["last4"]);

        //            //Values expected : American Express, Diners Club, Discover, JCB, Maestro, MasterCard, RuPay, Unknown, Visa, Union Pay
        //            objRazorpayCardBO.Network = Convert.ToString(card["network"]);
        //            objRazorpayCardBO.Issuer = Convert.ToString(card["issuer"]);
        //            objRazorpayCardBO.IsInternational = Convert.ToBoolean(card["international"]);
        //            objRazorpayCardBO.IsEMI = Convert.ToBoolean(card["emi"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        objLogger.WriteError(ex, "Error occured while getting card detail from razorpay.");
        //    }

        //    return objRazorpayCardBO;
        //}

        public bool CapturePayment(string PaymentId, string Amount)
        {
            bool returnStatus = false;
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "CapturePayment(string PaymentId, string Amount)";
                objLogger.WriteInfo("Data posted to Razor pay in CapturePayment Method is PaymentId = " + PaymentId + " Amount = " + Amount);
                Payment payment = client.Payment.Fetch(PaymentId);
                returnStatus = Convert.ToBoolean(payment["captured"]);
                if (!returnStatus)
                {
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    options.Add("amount", Amount);
                    Payment paymentCaptured = payment.Capture(options);
                    returnStatus = Convert.ToBoolean(paymentCaptured["captured"]);
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while capturing payment at razorpay.");
            }
            return returnStatus;
        }

        public Tuple<string, string, Dictionary<string, string>> CreateCustomer(string name, string email, string contact)
        {
            try
            {
                if (name.Length == 0)
                {
                    objLogger.WriteInfo("Name cannot be empty.");
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Name cannot be empty.",
                        null);
                }

                if (email.Length == 0)
                {
                    objLogger.WriteInfo("Email cannot be empty.");
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Email cannot be empty.",
                        null);
                }

                if (contact.Length == 0)
                {
                    objLogger.WriteInfo("Contact cannot be empty.");
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Contact cannot be empty.",
                        null);
                }

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("name", name);
                options.Add("email", email);
                options.Add("contact", contact);
                options.Add("fail_existing", "0");

                //Log Data posted to Razorpay
                objLogger.MethodName = "CreateCustomer(string name, string email, string contact)";
                objLogger.WriteInfo("Data posted to Razor pay in CreateCustomer Method is : " + Newtonsoft.Json.JsonConvert.SerializeObject(options, Formatting.None));
                Customer customer = client.Customer.Create(options);

                if (customer == null)
                {
                    objLogger.WriteInfo(string.Format("Create customer failed at razorpay for contact {0}", contact));
                    return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                       string.Format("Create customer failed at razorpay for contact {0}", contact),
                        null);
                }

                Dictionary<string, string> returnValue = new Dictionary<string, string>();
                returnValue.Add("PGCustomerId", Convert.ToString(customer["id"]));
                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while creating customer at razorpay");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, RazorpayPaymentBO> AutoCharge(Dictionary<string, string> additionalParam, string orderId, string tokenId, string customerId, Int64 amount, string email, string contact, string desc)
        {
            string _url = _razorpayUrl + "/payments/create/recurring";
            RazorpayPaymentBO razorpayPaymentBO = null;
            try
            {
                string authString = string.Format("{0}:{1}", key, secret);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)));

                    RazorpayCardAuthRequetDTO razorpayCardAuthRequetDTO = new RazorpayCardAuthRequetDTO
                    {
                        amount = amount.ToString(),
                        contact = contact,
                        currency = "INR",
                        customer_id = customerId,
                        description = desc,
                        email = email,
                        notes = additionalParam,
                        order_id = orderId,
                        recurring = "1",
                        token = tokenId
                    };
                    //Log Data posted to Razorpay
                    objLogger.MethodName = "AutoCharge(Dictionary<string, string> additionalParam, string orderId, string tokenId, string customerId, Int64 amount, string email, string contact, string desc)";
                    objLogger.WriteInfo(String.Format("Autocharge post to Razorpay: amount {0} contact {1} customerId {2} desc {3} email {4} orderId {5} token {6}", amount, contact,
                        customerId, desc, email, orderId, tokenId));
                    string razorpayResponse = "";

                    //HttpResponseMessage response = client.PostAsJsonAsync(new Uri(_url), razorpayCardAuthRequetDTO).Result;
                    //Below 3 lines in place of above one line
                    var json = JsonConvert.SerializeObject(razorpayCardAuthRequetDTO);
                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(new Uri(_url), stringContent).Result; //PostAsJsonAsync(new Uri(_url), razorpayCardAuthRequetDTO).Result;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        razorpayResponse = responseContent.ReadAsStringAsync().Result;
                        objLogger.WriteInfo(string.Format("Autocharge API execution failed with response {0}", razorpayResponse));
                        return new Tuple<string, string, RazorpayPaymentBO>(
                            "500",
                            razorpayResponse,//"Autocharge API execution failed.",
                            null);
                    }
                    else
                    {
                        var responseContent = response.Content;
                        razorpayResponse = responseContent.ReadAsStringAsync().Result;
                        objLogger.WriteDebug(string.Format("Autocharge API execution response {0}", razorpayResponse));

                        RazorpayPaymentResponseDTO razorpayPaymentResponseDTO = JsonConvert.DeserializeObject<RazorpayPaymentResponseDTO>(razorpayResponse);
                        if (razorpayPaymentResponseDTO == null || razorpayPaymentResponseDTO.razorpay_payment_id == null)
                        {
                            return new Tuple<string, string, RazorpayPaymentBO>(
                             "500",
                             "Payment Id in the response is null",
                             null);
                        }

                        using (HMACSHA256Hash objHash = new HMACSHA256Hash(secret))
                        {
                            if (!objHash.ValidateData(razorpayPaymentResponseDTO.razorpay_signature,
                                string.Format("{0}|{1}", razorpayPaymentResponseDTO.razorpay_order_id, razorpayPaymentResponseDTO.razorpay_payment_id)))
                            {
                                return new Tuple<string, string, RazorpayPaymentBO>(
                                  "500",
                                  "Auto charge response signature missmatch",
                                  null);
                            }
                        }

                        RazorpayPaymentBO objRazorPaymentBO = GetPaymentDetail(razorpayPaymentResponseDTO.razorpay_payment_id);
                        if (objRazorPaymentBO.Amount != amount.ToString() || objRazorPaymentBO.OrderId != orderId)
                        {
                            return new Tuple<string, string, RazorpayPaymentBO>(
                                  "500",
                                  "Amount missmatch in PG response",
                                  null);
                        }

                        if (objRazorPaymentBO.PaymentStatus == PaymentStatus.PaymentInitiated)
                        {
                            if (!CapturePayment(razorpayPaymentResponseDTO.razorpay_payment_id, amount.ToString()))
                            {
                                return new Tuple<string, string, RazorpayPaymentBO>(
                                  "500",
                                  "Payment capture in PG failed",
                                  null);
                            }
                        }

                        razorpayPaymentBO = new RazorpayPaymentBO
                        {
                            PaymentId = razorpayPaymentResponseDTO.razorpay_payment_id,
                            OrderId = razorpayPaymentResponseDTO.razorpay_order_id
                        };
                    }
                }

                return new Tuple<string, string, RazorpayPaymentBO>(
                     "200",
                     "Success",
                     razorpayPaymentBO);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occoured in AutoCharge");
                return new Tuple<string, string, RazorpayPaymentBO>(
                     "500",
                     ex.Message,
                     null);
            }
        }

        public Tuple<string, string, CardBO> GetCardTokenDetail(string CustomerId, string TokenId)
        {
            CardBO cardBO = null;
            try
            {
                //Log Data posted to Razorpay
                objLogger.MethodName = "GetCardTokenDetail(string CustomerId, string TokenId)";
                objLogger.WriteInfo("Data posted to Razor pay in GetCardTokenDetail Method is CustomerId = " + CustomerId + " TokenId = " + TokenId);
                Token token = client.Customer.Fetch(CustomerId).Token(TokenId);

                if (token == null)
                {
                    objLogger.WriteInfo(string.Format("Get card token detail execution failed with response."));
                    return new Tuple<string, string, CardBO>(
                    "500",
                    "Get card token detail execution failed",
                    null);
                }

                if (Convert.ToString(token["method"]).ToLower() != "card")
                {
                    objLogger.WriteInfo(string.Format("Get card token detail returned method {0}.", token["method"]));
                    return new Tuple<string, string, CardBO>(
                    "500",
                    "Get card token detail execution failed",
                    null);
                }

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime expiryDt = epoch.AddSeconds(Convert.ToDouble(token["expired_at"]));

                cardBO = new CardBO
                {
                    CardType = Convert.ToString(token["card"]["type"]),
                    ExpiryMonth = Convert.ToInt32(token["card"]["expiry_month"]),
                    ExpiryYear = Convert.ToInt32(token["card"]["expiry_year"]),
                    FullName = Convert.ToString(token["card"]["name"]),
                    IsEMI = Convert.ToBoolean(token["card"]["emi"]),
                    IsInternational = Convert.ToBoolean(token["card"]["international"]),
                    Issuer = Convert.ToString(token["card"]["issuer"]),
                    Last4 = Convert.ToString(token["card"]["last4"]),
                    Network = Convert.ToString(token["card"]["network"]),
                    PGCardTokenId = Convert.ToString(token["id"]),
                    ExpityDateTime = expiryDt,
                    PGCustomerId = CustomerId,
                    PaymentGateway = PaymentGatway.Razorpay
                };
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting card detail using token id from razorpay.");
                return new Tuple<string, string, CardBO>("500", "Failed", cardBO);
            }

            return new Tuple<string, string, CardBO>("200", "Success", cardBO);
        }

        public Tuple<string, string, List<CardBO>> GetCardTokenDetails(string customerId)
        {

            string _url = _razorpayUrl + string.Format("/customers/{0}/tokens", customerId);
            try
            {


                string authString = string.Format("{0}:{1}", key, secret);
                string responseString = "";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)));

                    HttpResponseMessage response = client.GetAsync(new Uri(_url)).Result;
                    var responseContent = response.Content;
                    responseString = responseContent.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        objLogger.WriteInfo(string.Format("Get card token API execution failed with response {0}", responseString));
                        return new Tuple<string, string, List<CardBO>>(
                        "500",
                        "Get card token API execution failed",
                        null);
                    }
                    objLogger.WriteDebug("Get card token API execution successful");
                }

                JObject objCard = JObject.Parse(responseString);
                List<CardBO> cards = new List<CardBO>();
                foreach (dynamic item in objCard["items"])
                {
                    if (item["method"] == "card")
                    {
                        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        DateTime expiryDt = epoch.AddSeconds(Convert.ToDouble(item["expired_at"]));

                        cards.Add(new CardBO
                        {
                            CardType = Convert.ToString(item["card"]["type"]),
                            ExpiryMonth = Convert.ToInt32(item["card"]["expiry_month"]),
                            ExpiryYear = Convert.ToInt32(item["card"]["expiry_year"]),
                            FullName = Convert.ToString(item["card"]["name"]),
                            IsEMI = Convert.ToBoolean(item["card"]["emi"]),
                            IsInternational = Convert.ToBoolean(item["card"]["international"]),
                            Issuer = Convert.ToString(item["card"]["issuer"]),
                            Last4 = Convert.ToString(item["card"]["last4"]),
                            Network = Convert.ToString(item["card"]["network"]),
                            PGCardTokenId = Convert.ToString(item["id"]),
                            ExpityDateTime = expiryDt,
                            PGCustomerId = customerId
                        });
                    }
                }

                return new Tuple<string, string, List<CardBO>>("200", "Success", cards);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while fetching card token from razorpay");
                return new Tuple<string, string, List<CardBO>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, bool> DeleteCard(string customerId, string tokenId)
        {
            string _url = _razorpayUrl + string.Format("/customers/{0}/tokens/{1}", customerId, tokenId);
            try
            {
                string authString = string.Format("{0}:{1}", key, secret);
                string responseString = "";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)));

                    HttpResponseMessage response = client.DeleteAsync(new Uri(_url)).Result;
                    var responseContent = response.Content;
                    responseString = responseContent.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        objLogger.WriteInfo(string.Format("Delete card token API execution failed with response {0}", responseString));
                        new Tuple<string, string, bool>("500", "Delete card token API execution failed.", false);
                    }
                    objLogger.WriteDebug(string.Format("Delete card token API execution successful with response {0}", responseString));
                }

                return new Tuple<string, string, bool>("200", "Success", true);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while deleting card token from razorpay");
                return new Tuple<string, string, bool>(
                    "500",
                    ex.Message,
                    false);
            }
        }

        public Tuple<string, string, PaymentLinkBO> CreatePaymentLink(string customerId, string ReceiptNumber, string Amount, string Description, DateTime? ExpiryDateTime, Dictionary<string, string> additionalParam)
        {

            Dictionary<string, object> input = new Dictionary<string, object>();
            try
            {
                input.Add("type", "link");
                input.Add("amount", Amount);
                input.Add("description", Description);
                input.Add("customer_id", customerId);
                //input.Add("view_less", true);
                input.Add("currency", "INR");
                input.Add("receipt", ReceiptNumber);

                if (ExpiryDateTime != null)
                {
                    Int32 unixTimeStamp;
                    DateTime currentTime = DateTime.Now;
                    DateTime zuluTime = currentTime.ToUniversalTime();
                    DateTime unixEpoch = new DateTime(1970, 1, 1);
                    unixTimeStamp = (Int32)(zuluTime.Subtract(unixEpoch)).TotalSeconds;
                    input.Add("expire_by", unixTimeStamp); //epoch/integer
                }

                input.Add("sms_notify", 1); //1 it is handled by razorpay
                input.Add("email_notify", 1);
                input.Add("notes", additionalParam);
                input.Add("partial_payment", "0");
                Invoice invoice = client.Invoice.Create(input);
                string j = invoice["id"];

                PaymentLinkBO objPaymentLinkBO = new PaymentLinkBO();
                objPaymentLinkBO.Request_RefId = invoice["receipt"];
                objPaymentLinkBO.PG_ReceiptNo = invoice["receipt"];
                objPaymentLinkBO.PG_OrderId = invoice["order_id"];
                objPaymentLinkBO.PG_PaymentId = invoice["payment_id"];
                objPaymentLinkBO.PG_InvoiceId = invoice["id"];
                objPaymentLinkBO.Payment_Transaction_ID = invoice["notes"]["Meru_PaymentId"];
                objPaymentLinkBO.Payment_Amount_Paise = invoice["amount"];
                objPaymentLinkBO.Url = invoice["short_url"];

                return new Tuple<string, string, PaymentLinkBO>("200", "Success", objPaymentLinkBO);

            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while creating payment link from razorpay");
                return new Tuple<string, string, PaymentLinkBO>(
                    "500",
                    ex.Message,
                    null);
            }

        }

        public Tuple<string, string, PaymentLinkBO> GetPaymentLink(string InvoiceId)
        {
            try
            {
                Invoice invoice = client.Invoice.Fetch(InvoiceId);
                PaymentLinkBO objPaymentLinkBO = new PaymentLinkBO();
                objPaymentLinkBO.Request_RefId = invoice["receipt"];
                objPaymentLinkBO.PG_ReceiptNo = invoice["receipt"];
                objPaymentLinkBO.PG_OrderId = invoice["order_id"];
                objPaymentLinkBO.PG_PaymentId = invoice["payment_id"];
                objPaymentLinkBO.PG_InvoiceId = invoice["id"];
                objPaymentLinkBO.Payment_Transaction_ID = invoice["notes"]["Meru_PaymentId"];
                objPaymentLinkBO.Payment_Amount_Paise = invoice["amount"];
                objPaymentLinkBO.Url = invoice["short_url"];

                return new Tuple<string, string, PaymentLinkBO>(
                    "200",
                    "Success",
                    objPaymentLinkBO);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Error occured while getting payment link detail from razorpay");
                return new Tuple<string, string, PaymentLinkBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
