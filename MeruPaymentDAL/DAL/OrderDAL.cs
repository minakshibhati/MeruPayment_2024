using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using MeruPaymentDAL.EntityModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentDAL
{
    public class OrderDAL
    {
        private LogHelper _logHelper;
        private PaymentHistoryDAL paymentHistoryDAL;

        public OrderDAL()
        {
            _logHelper = new LogHelper("OrderDAL");
            paymentHistoryDAL = new PaymentHistoryDAL();
        }

        public Tuple<string, string, Dictionary<string, string>> CreateOrder(OrderBO orderBO)
        {
            _logHelper.MethodName = "CreateOrder(OrderBO orderBO)";
            Dictionary<string, string> returnValue = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    tbl_PaymentTransaction objMeruPayments = new tbl_PaymentTransaction();

                    objMeruPayments = db.tbl_PaymentTransaction.Where(a => a.RequestRefId == orderBO.AppRequestId
                        && a.Payment_Type == orderBO.OrderType
                        && a.Payment_Amount_Paise == orderBO.Amount
                        && a.Contact == orderBO.Contact
                        && objMeruPayments.PaymentStatus != (int)PaymentStatus.PaymentSuccess).SingleOrDefault();

                    if (objMeruPayments == null)
                    {
                        objMeruPayments = new tbl_PaymentTransaction();
                        objMeruPayments.RequestSource = orderBO.AppSource;
                        objMeruPayments.RequestRefId = orderBO.AppRequestId;
                        objMeruPayments.Payment_Amount_Paise = orderBO.Amount;
                        objMeruPayments.PaymentMethod = (int)orderBO.PaymentMethod;
                        objMeruPayments.PaymentMethodRefId = orderBO.PaymentMethodRefId;
                        objMeruPayments.RequestRefValues = orderBO.AppRequestRefVal;
                        objMeruPayments.PaymentStatus = (int)PaymentStatus.PaymentCreated;
                        objMeruPayments.PaymentSource = (int)PaymentGatway.Razorpay;
                        objMeruPayments.CreatedOn = DateTime.Now;
                        objMeruPayments.LastUpdatedOn = DateTime.Now;
                        objMeruPayments.PurchaseDescription = orderBO.Desc;
                        objMeruPayments.FullName = orderBO.FullName;
                        objMeruPayments.Email = orderBO.Email;
                        objMeruPayments.Contact = orderBO.Contact;
                        objMeruPayments.Payment_Transaction_ID = Guid.NewGuid().ToString();
                        objMeruPayments.Payment_Type = orderBO.OrderType;
                        objMeruPayments.DeviceId = orderBO.DeviceId;

                        db.tbl_PaymentTransaction.Add(objMeruPayments);
                        if (db.SaveChanges() <= 0)
                        {
                            _logHelper.WriteInfo("Order creation in DB failed.");
                            return new Tuple<string, string, Dictionary<string, string>>(
                                "500",
                                "Order creation in DB failed.",
                                null);
                        }

                        objMeruPayments.Payment_Transaction_ID = "MP" + DateTime.Now.ToString("dd") +
                            orderBO.AppRequestId.Substring(orderBO.AppRequestId.Length - 2) +
                            objMeruPayments.PaymentIncrementId.ToString() +
                            new Random().Next(999).ToString("D3");

                        
                    }

                    objMeruPayments.PaymentStatus = (int)PaymentStatus.PaymentCreated;
                    objMeruPayments.LastUpdatedOn = DateTime.Now;
                    db.Entry(objMeruPayments).State = EntityState.Modified;

                    if (db.SaveChanges() <= 0)
                    {
                        _logHelper.WriteInfo("Payment Id generation failed in DB.");
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "Payment Id generation failed in DB.",
                            null);
                    }

                    returnValue = new Dictionary<string, string>();
                    returnValue.Add("PaymentId", objMeruPayments.Payment_Transaction_ID);
                    returnValue.Add("OrderId", objMeruPayments.PaymentIncrementId.ToString());
                    returnValue.Add("PGOrderId", objMeruPayments.PaymentRefData1);

                    paymentHistoryDAL.AddStatusChange(objMeruPayments.Payment_Transaction_ID, PaymentStatus.PaymentCreated, orderBO.OrderType);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while creating order in DB");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        /*added on 22-02-2023 for Razorpay SDK integration for driverapp
          create Order*/
        public Tuple<string, string, Dictionary<string, string>> CreateOrder_DA(OrderBORazorPay orderBO)
        {
            _logHelper.MethodName = "CreateOrder_DA(OrderRazorPayorderBO)";
            Dictionary<string, string> returnValue = null;
            try
            {
                JObject response = JObject.Parse(JsonConvert.SerializeObject(orderBO));
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    tbl_PaymentTransaction objMeruPayments = new tbl_PaymentTransaction();

                    objMeruPayments = db.tbl_PaymentTransaction.Where(a =>
                    a.RequestRefId == orderBO.id
                       // && a.Payment_Type == orderBO.OrderType
                        && a.Payment_Amount_Paise == orderBO.amount
                      //  && a.Contact == orderBO.Contact
                        && objMeruPayments.PaymentStatus != (int)PaymentStatus.PaymentSuccess).SingleOrDefault();

                    if (objMeruPayments == null)
                    {
                        var purchasedescription ="";
                        dynamic data = JObject.Parse(orderBO.notes.ToString());
                      //  dynamic request = JsonConvert.DeserializeObject(orderBO.notes);
                        
                        objMeruPayments = new tbl_PaymentTransaction();
                        objMeruPayments.RequestSource = orderBO.appsource;
                        objMeruPayments.RequestRefId = orderBO.id;
                        objMeruPayments.Payment_Amount_Paise = orderBO.amount;
                       
                        objMeruPayments.PaymentRefData1 = orderBO.id;
                       
                        objMeruPayments.PaymentSource = (int)PaymentGatway.Razorpay;
                        objMeruPayments.CreatedOn = DateTime.Now;
                        objMeruPayments.LastUpdatedOn = DateTime.Now;
                        //objMeruPayments.PurchaseDescription = orderBO.Desc;
                        objMeruPayments.FullName = orderBO.name;
                        objMeruPayments.Email = orderBO.email;
                        objMeruPayments.Contact = orderBO.mobile_number ;

                        if (data.ContainsKey("PaymentMethod"))
                        {
                            if (data["PaymentMethod"].ToString().ToLower() == "card")
                                objMeruPayments.PaymentMethod = (int)PaymentMethod.card;
                             if(data["PaymentMethod"].ToString().ToLower()=="upi")
                                objMeruPayments.PaymentMethod = (int)PaymentMethod.upi;
                            if (data["PaymentMethod"].ToString().ToLower()== "netbanking")
                                objMeruPayments.PaymentMethod = (int)PaymentMethod.netbanking;

                        }
                        else
                                objMeruPayments.PaymentMethod = (int)PaymentMethod.Unknown;
                        #region getting sp ID from the cab NO
                        var cabno = "";
                        var spid = "";
                        if (data.ContainsKey("SPId"))
                          spid   = data["SPId"].ToString();
                        if (data.ContainsKey("Meru_PaymentId"))
                        
                            objMeruPayments.Payment_Transaction_ID = data["Meru_PaymentId"].ToString();
                       
                        tblCabMaster objcabdet = new tblCabMaster();
                        if (data.ContainsKey("PurchaseDescription"))
                        {
                            objMeruPayments.PurchaseDescription = data["PurchaseDescription"].ToString();
                            cabno = data["PurchaseDescription"].ToString().Replace("Payment for ", "");
                            //**getting sp ID from the cab NO
                           if(spid=="")
                           {
                                objcabdet = db.tblCabMaster.Where(a =>
                                              a.CabRegistrationNo == cabno
                                               ).SingleOrDefault();
                                spid = objcabdet.SPID.ToString();
                           }
                              

                        }
                       

                        objMeruPayments.RequestRefValues = new JObject { new JProperty("SPId", spid.ToString()), new JProperty("CarNo", cabno.ToString()) }.ToString(Formatting.None);
                        objMeruPayments.PaymentRefValues = new JObject { new JProperty("RazorOrderId", orderBO.id) }.ToString(Formatting.None);
                        objMeruPayments.PaymentStatus = (int)PaymentStatus.PaymentCreated;
                        #endregion
                        //objMeruPayments.Payment_Transaction_ID = Guid.NewGuid().ToString();                      
                        // objMeruPayments.Payment_Type = orderBO.OrderType;
                        //objMeruPayments.DeviceId = orderBO.DeviceId;
                        objMeruPayments.Notes = orderBO.notes.ToString();
                        objMeruPayments.Offer_Id = orderBO.OfferId.ToString();
                        objMeruPayments.Receipt = orderBO.receipt.ToString();
                        objMeruPayments.AmountDue = orderBO.amount_due;
                        objMeruPayments.AmountPaid = orderBO.amount_paid;
                        objMeruPayments.OrderResponse = response.ToString();

                        db.tbl_PaymentTransaction.Add(objMeruPayments);
                        if (db.SaveChanges() <= 0)
                        {
                            _logHelper.WriteInfo("Order creation in DB failed.");
                            return new Tuple<string, string, Dictionary<string, string>>(
                                "500",
                                "Order creation in DB failed.",
                                null);
                        }
                        if (objMeruPayments.Payment_Transaction_ID==null && !data.ContainsKey("Meru_PaymentId"))
                            objMeruPayments.Payment_Transaction_ID = "MP" + DateTime.Now.ToString("dd") +
                                orderBO.created_at.Substring(orderBO.created_at.Length - 2) +

                                objMeruPayments.PaymentIncrementId.ToString() +
                                 new Random().Next(999).ToString("D3");


                    }
                    else
                    {
                        _logHelper.WriteInfo("Payment Id already exists.");
                    }
                    objMeruPayments.PaymentStatus = (int)PaymentStatus.PaymentCreated;
                    objMeruPayments.LastUpdatedOn = DateTime.Now;
                    db.Entry(objMeruPayments).State = EntityState.Modified;

                    if (db.SaveChanges() <= 0)
                    {
                        _logHelper.WriteInfo("Payment Id generation failed in DB.");
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "Payment Id generation failed in DB.",
                            null);
                    }
                    _logHelper.WriteInfo(string.Format("Payment Id generation Successfull in DB."));
                    returnValue = new Dictionary<string, string>();
                    returnValue.Add("PaymentId", objMeruPayments.Payment_Transaction_ID);
                    returnValue.Add("OrderId", objMeruPayments.PaymentIncrementId.ToString());
                    //returnValue.Add("PGOrderId", objMeruPayments.PaymentRefData1);

                    paymentHistoryDAL.AddStatusChange(objMeruPayments.Payment_Transaction_ID, PaymentStatus.PaymentCreated, "");
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while creating order in DB");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }
        public Tuple<string, string, Dictionary<string, string>> UpdatePGOrderId(string paymentId, string PGOrderId)
        {
            _logHelper.MethodName = "UpdatePGOrderId(OrderBO orderBO)";
            Dictionary<string, string> returnValue = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var payment = (from r in db.tbl_PaymentTransaction where r.Payment_Transaction_ID == paymentId select r).FirstOrDefault();
                    if (payment == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find detail for Meru payment Id {0}", paymentId));
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            string.Format("Unable to find detail for Meru payment Id {0}", paymentId),
                            null);
                    }
                    payment.PaymentRefData1 = PGOrderId;
                    payment.PaymentRefValues = new JObject { new JProperty("RazorOrderId", PGOrderId) }.ToString(Formatting.None);
                    payment.PaymentStatus = (int)PaymentStatus.PaymentInitiated;
                    payment.LastUpdatedOn = DateTime.Now;

                    db.Entry(payment).State = EntityState.Modified;
                    if (db.SaveChanges() <= 0)
                    {
                        _logHelper.WriteInfo("PG Order Id update failed in DB.");
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "PG Order Id update failed in DB.",
                            null);
                    }
                    paymentHistoryDAL.AddStatusChange(payment.Payment_Transaction_ID, PaymentStatus.PaymentInitiated, payment.Payment_Type);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while updating order to DB");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, OrderBO> GetOrderDetailByPaymentId(string paymentId)
        {
            _logHelper.MethodName = "GetOrderDetailByPaymentId(string paymentId)";
            try
            {
                int paymentStatusId = (int)PaymentStatus.PaymentInitiated;
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var payment = (from r in db.tbl_PaymentTransaction where r.Payment_Transaction_ID == paymentId && r.PaymentStatus == paymentStatusId select r).FirstOrDefault();
                    if (payment == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find detail for Meru payment Id {0} in initiated status", paymentId));
                        return new Tuple<string, string, OrderBO>(
                            "500",
                            string.Format("Unable to find detail for Meru payment Id {0}", paymentId),
                            null);
                    }

                    return new Tuple<string, string, OrderBO>(
                   "200",
                   "Success",
                   new OrderBO
                   {
                       Amount = payment.Payment_Amount_Paise,
                       AppRequestId = payment.RequestRefId,
                       AppSource = payment.RequestSource,
                       Contact = payment.Contact,
                       Desc = payment.PurchaseDescription,
                       Email = payment.Email,
                       FullName = payment.FullName,
                       OrderType = payment.Payment_Type,
                       PaymentMethod = (PaymentMethod)payment.PaymentMethod,
                       PaymentId = payment.Payment_Transaction_ID,
                       PGOrderId = payment.PaymentRefData1,
                       DeviceId = payment.DeviceId
                   });
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting order by payment Id to DB");
                return new Tuple<string, string, OrderBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
