using MeruPaymentBO;
using MeruPaymentDAL.DAL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentDAL
{
    [TestFixture]
    class PaymentDALTest
    {
        [Test]
        public void SuccessPayment_Given_Correct_Value()
        {
            //ARRANGE
            string PaymentId = "MP1122673243", PGPaymentId = "pay_CIFfFdNeeVhPCF";

            //ACT
            PaymentDAL paymentDAL = new PaymentDAL();
            Tuple<string, string, Dictionary<string, string>> val = paymentDAL.SuccessPayment(PaymentId, PGPaymentId, MeruPaymentBO.PaymentGatway.Razorpay);

            //ASSERT
            Assert.IsNotNull(val);
            Assert.AreEqual("200", val.Item1);
        }

        [Test]
        public void CreatePayment_Given_Correct_Value() 
        {
            //PaymentBO paymentBO = new PaymentBO();
            //paymentBO.RequestSource = "outstationweb";
            //paymentBO.RequestReferenceVal = "";
            //paymentBO.RequestUniqueId = "";
            //paymentBO.Amount = 100;
            //paymentBO.PaymentMethod = PaymentMethod.card;
            //paymentBO.PaymentStatus = (int)PaymentStatus.PaymentCreated;
            //paymentBO.PaymentSource = (int)PaymentGatway.Unknown;
            //paymentBO.PurchaseDesc = "paymentBO.Payment";
            //paymentBO.FullName = "Shraddha";
            //paymentBO.Email = "shraddha.kasbe@meru.in";
            //paymentBO.Mobile = "8879572250";
            //paymentBO.PaymentType = "";






        }   
    }       
}           
            
            
            
            
            
            