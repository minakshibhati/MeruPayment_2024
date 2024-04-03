using MeruPaymentBAL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruPaymentBAL
{
    [TestFixture]
    class SaveAuthorisedCardsTest
    {
        [Test]
        public void ProcessRequest_Given_Correct_Value()
        {
            //ARRANGE
            string PGCustomerID = "cust_CHtAqfZSQzGWQN";
            string PaymentId = "xby";

            //ACT
            SaveAuthorisedCards saveAuthorisedCards = new SaveAuthorisedCards();
            Tuple<string, string, Dictionary<string, string>> val = saveAuthorisedCards.ProcessRequest(PGCustomerID, PaymentId);

            //ASSERT
            Assert.IsNotNull(val);
            Assert.AreEqual("200", val.Item1);
        }
    }
}
