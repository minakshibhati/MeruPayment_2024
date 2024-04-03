using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentBAL;

namespace MeruPayment.Test.MeruPaymentBAL
{
    [TestFixture]
    class PreChargeValidationTest
    {

        //[Test]
        [TestCase("TIP", "47221502", "9870043463", 100, 1060)]
        public void Process_GivenCorrectValue(string PaymentType, string AppRequestId, string Mobile, long Amount, int PaymentMethodRefId)
        {
            //ARRANGE
            PreChargeValidation preChargeValidation = new PreChargeValidation();

            //string PaymentType = "TIP", AppRequestId = "47221502", Mobile = "9870043463";
            //long Amount = 100;
            //int PaymentMethodRefId = 1060;

            //ACT
            Tuple<string, string, bool> returnValidation = preChargeValidation.Validate(PaymentType, AppRequestId, Amount, PaymentMethodRefId, Mobile);

            //ASSERT
            Assert.IsNotNull(returnValidation);
            Assert.AreEqual(returnValidation.Item1, "200");
            Assert.IsTrue(returnValidation.Item3);
        }
    }
}
