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
    public class DisablePaymentInstrumentTest
    {
        [TestCase("9833240124")]
        public void ProcessIndividual_Test(string mobile)
        {
            DisablePaymentInstrument disablePaymentInstrument = new DisablePaymentInstrument();
            disablePaymentInstrument.ProcessIndividual(mobile);
        }
    }
}
