using MeruPaymentBO;
using MeruPaymentCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentCore
{
    [TestFixture]
    public class RazorpayTest
    {
        Razorpay razorpay = new Razorpay();

        [Test]
        public void CreateCustomer_Given_Correct_Value()
        {
            //ARRANGE
            string CustomerName = "Ajay Singadiya",
                CustomerEmail = "ajsingadiya@gmail.com",
                CustomerContact = "9892401994";

            string returnMessage = "";

            //ACT
            Tuple<string, string, Dictionary<string, string>> val = razorpay.CreateCustomer(CustomerName, CustomerEmail, CustomerContact);

            //ASSERT
            Assert.IsEmpty(returnMessage);
            Assert.IsNotNull(val);
        }

        [Test]
        public void CreateOrder_Given_Correct_Value()
        {
            //ARRANGE
            string ReceiptNo = "123",
                Amount = "100";

            string returnMessage = "";

            //ACT
            returnMessage = razorpay.CreateOrder(ReceiptNo, Amount);

            //ASSERT
            Assert.IsNotEmpty(returnMessage);
            Assert.IsNotNull(returnMessage);
        }

        [Test]
        public void AutoCharge_Given_Correct_Value()
        {
            //ARRANGE
            string CustomerEmail = "kamlesh.makare@meru.in",
                CustomerContact = "8689979995",
                CustomerId = "cust_CuGFu6cpfedcYi",
                TokenId = "token_CukRfzGJmXSr",//"token_C3iCAvHZaed0Ix",
                Desc = "Unit test auto charge";

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("Meru_PaymentId", "123");
            additionalParams.Add("AppRequestId", "123");

            //ACT
            string orderId = razorpay.CreateOrder("123", "100");
            Tuple<string, string, RazorpayPaymentBO> resultAutoCharge = razorpay.AutoCharge(additionalParams, orderId, TokenId, CustomerId, 100, CustomerEmail, CustomerContact, Desc);

            //ASSERT
            Assert.AreEqual(resultAutoCharge.Item1, "200");
            Assert.IsNotNull(resultAutoCharge.Item3.PaymentId);
        }

        [Test]
        public void GetCardTokenDetails_Given_Correct_Valu()
        {
            string CustomerId = "cust_C3HU25rzxnO4ss";
            var a = razorpay.GetCardTokenDetails(CustomerId);

            Assert.IsNotNull(a);
        }

        [Test]
        public void GetPaymentDetailByOrderId_Given_Correct_Value()
        {
            string _OrderID = "order_D5reRyMS3BmC1Y";

            RazorpayPaymentBO razorpayPaymentBO = razorpay.GetPaymentDetailByOrderId(_OrderID);

            Assert.IsNotNull(razorpayPaymentBO);
        }

        [Test]
        public void GetPaymentDetail_Given_Correct_Value()
        {
            string _PaymentID = "pay_CYrbumMnv8N9IJ";

            RazorpayPaymentBO razorpayPaymentBO = razorpay.GetPaymentDetail(_PaymentID);

            Assert.IsNotNull(razorpayPaymentBO);
        }

        [Test]
        public void GetCardDetail_Given_Correct_Value()
        {
            string _cardID = "card_CYrbuoIDKmIcsW";

            RazorpayCardBO razorpayCardBO = razorpay.GetCardDetail(_cardID);

            Assert.IsNotNull(razorpayCardBO);
        }

        [Test]
        public void GetCardTokenDetail_Given_Correct_Value()
        {
            string TokenId = "token_CSAYun2ldQUvRQ";
            string _CustomerId = "cust_C3HU25rzxnO4ss";

            Tuple<string, string, CardBO> returnCardData = razorpay.GetCardTokenDetail(_CustomerId, TokenId);

            Assert.AreEqual("200", returnCardData.Item1);
        }

        [Test]
        public void RefundPayment_Given_Correct_Value()
        {
            string PaymentId = "pay_DXpGKrH86pU9jQ", note = "test_pay_DXpGKrH86pU9jQ";
            long RefundAmount = 2000;
            bool IsPartialRefund = false;
            RazorpayRefundBO razorpayRefundBO = razorpay.RefundPayment(PaymentId, RefundAmount, IsPartialRefund, note);

            Assert.IsNull(razorpayRefundBO.ErrorCode);
            Assert.Greater(razorpayRefundBO.RefundId.Length, 0);
        }

        [Test]
        public void CreatePaymentLink_Given_Correct_Value()
        {
            Dictionary<string, string> notes = new Dictionary<string, string>();
            notes.Add("msg", "hello");
            string customerId = "cust_CdmvTMSXu3EjMC";
            string amt = "100";
            string desc = "test link";
            DateTime? expitydttime = null;// new DateTime().AddDays(1);
            razorpay.CreatePaymentLink(customerId, Guid.NewGuid().ToString(), amt, desc, expitydttime, notes);
        }

        [Test]
        public void GetPaymentLinkDetail_Given_Correct_Value()
        {
            string InvoiceId = "inv_DlmJb73L6poY5s";
            Tuple<string, string, PaymentLinkBO> returnDetail = razorpay.GetPaymentLink(InvoiceId);
            if (returnDetail.Item3 != null)
            {
                string quer = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    returnDetail.Item3.Payment_Transaction_ID,
                    returnDetail.Item3.PG_ReceiptNo,
                    returnDetail.Item3.Payment_Amount_Paise.ToString(),
                    returnDetail.Item3.PG_OrderId,
                    returnDetail.Item3.Url,
                    returnDetail.Item3.Description,
                    returnDetail.Item3.Contact,
                    returnDetail.Item3.Email);
                int i = 0;
                i = i + 1;
            }
        }
    }
}
