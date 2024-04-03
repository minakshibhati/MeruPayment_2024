using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MeruPaymentBAL;
using MeruPaymentBO;

namespace MeruPayment.Test.MeruPaymentBAL
{
    [TestFixture]
    class CreateOrderTest
    {
        CreateOrder createOrder = new CreateOrder();

        [Test]
        public void Process_GivenNewValue()
        {
            Tuple<string, string, Dictionary<string, string>> returnData = createOrder.ProcessRequest(new OrderBO { Amount = 100, AppRequestId = "25006", AppSource = "com.merucabs.merucabs", Contact = "9833240124", Desc = "Card Authorization", DeviceId = "64880402846117cc", Email = "prathamesh.dabre@meru.in", FullName = "bhav", OrderType = "CARD_AUTH", PaymentId = "MP24068770034", PaymentMethod = PaymentMethod.card, PaymentMethodRefId = 0, PGOrderId = "order_DXolp4DUwWYN6m" });
            Assert.AreEqual(returnData.Item1, "200");
            Dictionary<string, string> returnDatas = returnData.Item3;
            Assert.AreEqual(returnData.Item1, "200");
            Assert.AreNotEqual(returnDatas["RazorpayOrderId"], "order_DXST66xMYAYb9D");
        }

        [Test]
        public void Process_GivenExistingValue()
        {
            Tuple<string, string, Dictionary<string, string>> returnData = createOrder.ProcessRequest(new OrderBO { Amount = 100, AppRequestId = "31359", AppSource = "com.winit.merucab", Contact = "8824363728", Desc = "Card Authorization", DeviceId = "64880402846117cc", Email = "bhav@gmail.com", FullName = "bhav", OrderType = "CARD_AUTH", PaymentId = "MP23918750091", PaymentMethod = PaymentMethod.card, PaymentMethodRefId = 0, PGOrderId = "order_DXPhDK3zgFrDsW" });
            Assert.AreEqual(returnData.Item1, "200");
            Dictionary<string, string> returnDatas = returnData.Item3;
            Assert.AreEqual(returnData.Item1, "200");
            Assert.AreEqual(returnDatas["RazorpayOrderId"], "order_DXPhDK3zgFrDsW");
        }
    }
}
