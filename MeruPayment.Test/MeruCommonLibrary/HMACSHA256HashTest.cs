using MeruCommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MeruPayment.Test.MeruCommonLibrary
{
    [TestFixture]
    public class HMACSHA256HashTest
    {
        [TestCase("rLy5=GwfEfz786+=xV-MPhad", "70751c916277c37457c9684845d58c56a7549c258b83d30b5049d6696086f99a", "{\"entity\":\"event\",\"account_id\":\"acc_B5uDeJeAJelno4\",\"event\":\"invoice.paid\",\"contains\":[\"payment\",\"order\",\"invoice\"],\"payload\":{\"payment\":{\"entity\":{\"id\":\"pay_Do6l5FHwYZPwiy\",\"entity\":\"payment\",\"amount\":20500,\"currency\":\"INR\",\"status\":\"captured\",\"order_id\":\"order_Do6ixiGzB4giom\",\"invoice_id\":\"inv_Do6ixgPs7FZ3BU\",\"international\":false,\"method\":\"card\",\"amount_refunded\":0,\"refund_status\":null,\"captured\":true,\"description\":\"#inv_Do6ixgPs7FZ3BU\",\"card_id\":\"card_DcGvuIpfrEtnQ9\",\"card\":{\"id\":\"card_DcGvuIpfrEtnQ9\",\"entity\":\"card\",\"name\":\"Muthu\",\"last4\":\"2841\",\"network\":\"Visa\",\"type\":\"credit\",\"issuer\":\"CITI\",\"international\":false,\"emi\":false},\"bank\":null,\"wallet\":null,\"vpa\":null,\"email\":\"sarina.robinson1347@gmail.com\",\"contact\":\"+918976334674\",\"notes\":[],\"fee\":257,\"tax\":0,\"error_code\":null,\"error_description\":null,\"created_at\":1575457334}},\"order\":{\"entity\":{\"id\":\"order_Do6ixiGzB4giom\",\"entity\":\"order\",\"amount\":20500,\"amount_paid\":20500,\"amount_due\":0,\"currency\":\"INR\",\"receipt\":\"49938148\",\"offer_id\":null,\"offers\":{\"entity\":\"collection\",\"count\":0,\"items\":[]},\"status\":\"paid\",\"attempts\":1,\"notes\":[],\"created_at\":1575457213}},\"invoice\":{\"entity\":{\"id\":\"inv_Do6ixgPs7FZ3BU\",\"entity\":\"invoice\",\"receipt\":\"49938148\",\"invoice_number\":\"49938148\",\"customer_id\":\"cust_DXt9tNrUJGgkAx\",\"customer_details\":{\"id\":\"cust_DXt9tNrUJGgkAx\",\"name\":\"Analisa Lewis\",\"email\":\"sarina.robinson1347@gmail.com\",\"contact\":\"8976334674\",\"gstin\":null,\"billing_address\":null,\"shipping_address\":null,\"customer_name\":\"Analisa Lewis\",\"customer_email\":\"sarina.robinson1347@gmail.com\",\"customer_contact\":\"8976334674\"},\"order_id\":\"order_Do6ixiGzB4giom\",\"payment_id\":\"pay_Do6l5FHwYZPwiy\",\"status\":\"paid\",\"expire_by\":null,\"issued_at\":1575457213,\"paid_at\":1575457354,\"cancelled_at\":null,\"expired_at\":null,\"sms_status\":\"sent\",\"email_status\":\"sent\",\"date\":1575457213,\"terms\":null,\"partial_payment\":false,\"gross_amount\":20500,\"tax_amount\":0,\"taxable_amount\":0,\"amount\":20500,\"amount_paid\":20500,\"amount_due\":0,\"first_payment_min_amount\":null,\"currency\":\"INR\",\"currency_symbol\":\"\u20b9\",\"description\":\"Payment for Meru ride on 04 Dec 2019\",\"notes\":[],\"comment\":null,\"short_url\":\"https://rzp.io/i/TxfRArq\",\"view_less\":true,\"billing_start\":null,\"billing_end\":null,\"type\":\"link\",\"group_taxes_discounts\":false,\"supply_state_code\":null,\"user_id\":\"BM4nVM8UiLKP91\",\"created_at\":1575457214,\"idempotency_key\":null}}},\"created_at\":1575457354}", ExpectedResult = true)]
        public bool ValidateDataTest(string Secret, string Checksum, string Data)
        {
            bool returnValue = false;
            using (HMACSHA256Hash objHash = new HMACSHA256Hash(Secret))
            {
                byte[] hash = objHash.Encrypt(Data);


                StringBuilder builder = new StringBuilder();

                foreach (byte b in hash)
                    builder.AppendFormat("{0:x2}", b);


                returnValue = Checksum == builder.ToString();

            }
            return returnValue;
        }
    }
}
