using MeruPaymentBO;
using MeruPaymentDAL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentDAL
{
    [TestFixture]
    class CustomerDALTest
    {
        [Test]
        public void UpdatePGCustomerId_Given_Correct_Value()
        {
            //ARRANGE
            string PGCustomerID = "cust_CHtAqfZSQzGWQN",
                CustomerContact = "9892401994";

            string returnMessage = "";

            //ACT
            CustomerDAL customerDAL = new CustomerDAL();
            Tuple<string, string, Dictionary<string, string>> val = customerDAL.UpdatePGCustomerId(CustomerContact, MeruPaymentBO.PaymentGatway.Razorpay, PGCustomerID);

            //ASSERT
            Assert.IsNotNull(val);
            Assert.IsNotNull(val.Item3["PGCustomerId"]);
        }

        [TestCase(CustomerStatus.Active, ExpectedResult = 0)]
        public int GetCustomerDetailByCustomerStatus_Test(CustomerStatus customerStatus)
        {
            CustomerDAL customerDAL = new CustomerDAL();
            Tuple<string, string, List<CustomerBO>> returnCustomer = customerDAL.GetCustomerDetailByCustomerStatus(customerStatus);

            if (returnCustomer == null || returnCustomer.Item1 != "200")
            {
                return -1;
            }

            return returnCustomer.Item3.Count();
        }
    }
}
