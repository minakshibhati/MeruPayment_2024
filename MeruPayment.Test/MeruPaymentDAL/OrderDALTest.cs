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
    class OrderDALTest
    {
        [Test]
        public void GetOrderDetailByPaymentId_Given_Correct_Value()
        {
            //ARRANGE
            string PaymentId = "MP1060624496";

            //ACT
            OrderDAL orderDAL = new OrderDAL();
            Tuple<string, string, OrderBO> val = orderDAL.GetOrderDetailByPaymentId(PaymentId);

            //ASSERT
            Assert.IsNotNull(val);
            Assert.IsNotNull(val.Item3);
        }

        [Test]
        public void CreateOrder() 
        {
            ////ARRANGE
            //OrderBO orderBO = new OrderBO();
            //orderBO.AppSource = "website";
            //orderBO.AppRequestId = "9821354464";
            //orderBO.Amount = 100;
            //orderBO.PaymentMethod = "card";
            //orderBO.PaymentMethodRefId = "";
            //orderBO.Desc = "";
            //orderBO.FullName = "";
            //orderBO.Email = "";
            //orderBO.Contact = "";
            //orderBO.OrderType = "";
            //orderBO.DeviceId = "";

            ////ACT
            //OrderDAL orderDAL = new OrderDAL();
            //orderDAL.CreateOrder(orderBO);
        }
    }
}
