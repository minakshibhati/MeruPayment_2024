using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL.EntityModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentDAL
{
    public class CustomerDAL : IDisposable
    {
        private LogHelper _logHelper;
        private Dictionary<string, string> returnValue = null;

        public CustomerDAL()
        {
            _logHelper = new LogHelper("CustomerDAL");
            returnValue = new Dictionary<string, string>();
        }

        public Tuple<string, string, Dictionary<string, string>> CreateCustomer(CustomerBO customerBO)
        {
            try
            {
                int paymentGatewayId = (int)customerBO.PaymentGateway;
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    tbl_Payment_Customer_Details customerDetail = (from r in db.tbl_Payment_Customer_Details
                                                                   where r.Customer_Mobile_No == customerBO.Contact && r.Payment_Gateway_ID == paymentGatewayId
                                                                   select r).FirstOrDefault();

                    if (customerDetail == null)
                    {
                        customerDetail = (from r in db.tbl_Payment_Customer_Details
                                          where r.Customer_Mobile_No == customerBO.Contact
                                          select r).FirstOrDefault();
                    }


                    if (customerDetail != null)
                    {

                        if (customerDetail.Customer_Status == (int)CustomerStatus.Block)
                        {
                            _logHelper.WriteInfo(string.Format("Customer with mobile number {0} cannot be created as it is blocked in DB.", customerBO.Contact));

                            return new Tuple<string, string, Dictionary<string, string>>(
                                "500",
                                "Customer cannot be created.",
                                null);
                        }

                        _logHelper.WriteInfo("Customer already exist in DB.");
                        returnValue.Add("CustomerId", customerDetail.ID.ToString());
                        returnValue.Add("PGCustomerId", customerDetail.Provider_Customer_ID);
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "200",
                            "Customer already exist in DB.",
                            returnValue);

                    }

                    customerDetail = new tbl_Payment_Customer_Details();
                    customerDetail.Customer_Email = customerBO.Email;
                    customerDetail.Customer_Mobile_No = customerBO.Contact;
                    customerDetail.Customer_Name = customerBO.FullName;
                    customerDetail.Payment_Gateway_ID = (int)customerBO.PaymentGateway;
                    customerDetail.Provider_Customer_ID = customerBO.PGCustomerId;
                    customerDetail.Record_Created_DateTime = DateTime.Now;
                    customerDetail.Record_Update_DateTime = DateTime.Now;
                    customerDetail.Customer_Status = (int)CustomerStatus.Active;

                    db.tbl_Payment_Customer_Details.Add(customerDetail);
                    //db.Entry(customerDetail).State = EntityState.Added;
                    if (db.SaveChanges() <= 0)
                    {
                        _logHelper.WriteInfo("Customer creation in DB failed.");
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "Customer creation in DB failed.",
                            null);
                    }


                    returnValue.Add("CustomerId", customerDetail.ID.ToString());
                    returnValue.Add("PGCustomerId", customerDetail.Provider_Customer_ID);
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while save customer to DB");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, Dictionary<string, string>> UpdatePGCustomerId(string mobile, PaymentGatway paymentGateway, string PGCustomerId)
        {
            _logHelper.MethodName = "UpdatePGCustomerId(string mobile, PaymentGatway paymentGateway, string PGCustomerId)";
            Dictionary<string, string> returnValue = null;
            int paymentGatewayId = (int)paymentGateway;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var customer = (from r in db.tbl_Payment_Customer_Details
                                    where r.Customer_Mobile_No == mobile && r.Payment_Gateway_ID == paymentGatewayId && r.Customer_Status == (int)CustomerStatus.Active
                                    select r).FirstOrDefault();
                    if (customer == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find customer detail for mobile {0}", mobile));
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            string.Format("Unable to find customer detail for mobile {0}", mobile),
                            null);
                    }
                    customer.Provider_Customer_ID = PGCustomerId;
                    customer.Payment_Gateway_ID = paymentGatewayId;
                    customer.Record_Update_DateTime = DateTime.Now;

                    db.Entry(customer).State = EntityState.Modified;
                    if (db.SaveChanges() <= 0)
                    {
                        _logHelper.WriteInfo("PG customer Id update failed in DB.");
                        return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "PG custome Id update failed in DB.",
                            null);
                    }
                }
                returnValue = new Dictionary<string, string>();
                returnValue.Add("PGCustomerId", PGCustomerId);
                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    returnValue);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while updating customer to DB");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CustomerBO> GetCustomerDetailByMobileNo(string mobileNo, PaymentGatway paymentGateway)
        {
            _logHelper.MethodName = "GetCustomerDetailByMobileNo(string mobileNo)";
            try
            {
                int paymentGatewayId = (int)paymentGateway;
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var customer = (from r in db.tbl_Payment_Customer_Details
                                    where r.Customer_Mobile_No == mobileNo && r.Payment_Gateway_ID == paymentGatewayId && r.Customer_Status == (int)CustomerStatus.Active
                                    select r).FirstOrDefault();
                    if (paymentGateway == PaymentGatway.Unknown)
                    {
                        customer = (from r in db.tbl_Payment_Customer_Details
                                    where r.Customer_Mobile_No == mobileNo && r.Customer_Status == (int)CustomerStatus.Active
                                    select r).FirstOrDefault();
                    }


                    if (customer == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find customer detail for mobile no. {0}", mobileNo));
                        return new Tuple<string, string, CustomerBO>(
                            "500",
                            string.Format("Unable to find customer detail for mobile no. {0}", mobileNo),
                            null);
                    }

                    return new Tuple<string, string, CustomerBO>(
                   "200",
                   "Success",
                   new CustomerBO
                   {
                       PGCustomerId = customer.Provider_Customer_ID,
                       PaymentGateway = (PaymentGatway)customer.Payment_Gateway_ID,
                       FullName = customer.Customer_Name,
                       Email = customer.Customer_Email,
                       CustomerStatus = (CustomerStatus)customer.Customer_Status,
                       Contact = customer.Customer_Mobile_No
                   });
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting customer from DB");
                return new Tuple<string, string, CustomerBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, List<CustomerBO>> GetCustomerDetailByCustomerStatus(CustomerStatus customerStatus)
        {
            _logHelper.MethodName = "GetCustomerDetailByCustomerStatus(CustomerStatus customerStatus)";
            List<CustomerBO> customerList = null;
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var customers = (from r in db.tbl_Payment_Customer_Details
                                     where r.Customer_Status == (int)customerStatus
                                     select r).ToList();

                    if (customers == null)
                    {
                        _logHelper.WriteInfo(string.Format("Unable to find customers with status {0}", customerStatus));
                        return new Tuple<string, string, List<CustomerBO>>(
                            "500",
                            string.Format("Unable to find customers with status {0}", customerStatus),
                            null);
                    }
                    customerList = new List<CustomerBO>();
                    foreach (var item in customers)
                    {
                        customerList.Add(new CustomerBO
                        {
                            Contact = item.Customer_Mobile_No,
                            CustomerStatus = (CustomerStatus)item.Customer_Status,
                            Email = item.Customer_Email,
                            FullName = item.Customer_Name,
                            PaymentGateway = (PaymentGatway)item.Payment_Gateway_ID,
                            PGCustomerId = item.Provider_Customer_ID
                        });
                    }

                    return new Tuple<string, string, List<CustomerBO>>(
                   "200",
                   "Success",
                   customerList);
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting customer from DB");
                return new Tuple<string, string, List<CustomerBO>>(
                    "500",
                    ex.Message,
                    customerList);
            }
        }

        public string GetCustomerId(string mobileNumber, string emailAddress)
        {
            string providerCustomerId = null;
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    providerCustomerId = entities.tbl_Payment_Customer_Details
                        .Where(x => x.Customer_Email == emailAddress || x.Customer_Mobile_No == mobileNumber)
                        .Select(x => x.Provider_Customer_ID).FirstOrDefault().ToString();
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Exception occured while fetching Customer Id for Mobile: " + mobileNumber + " emailAddress:" + emailAddress);
            }
            return providerCustomerId;
        }

        public void Dispose() { }
    }
}
