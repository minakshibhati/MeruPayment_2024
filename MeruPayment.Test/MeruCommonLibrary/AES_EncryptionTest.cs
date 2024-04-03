using MeruCommonLibrary;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruCommonLibrary
{
    [TestFixture]
    public class AES_EncryptionTest
    {
        [TestCase("xyz", ExpectedResult = "pT145bY8WVH01apYvzopUQ==")]
        //[TestCase("token_CSAYun2ldQUvRQ", ExpectedResult = "7udxIRf+GvthPMLdGzu8DUmz94hhXmZmjit+Jlw/AKU=")]
        [TestCase("token_CSAYun2ldQUvRQ", ExpectedResult = "7udxIRf+GvthPMLdGzu8DUmz94hhXmZmjit+Jlw/AKU=")]
        public string Encrypt_Given_Correct_Value(string value)
        {
            string eValue = "";

            AES_Encryption aES_Encryption = new AES_Encryption();
            eValue = aES_Encryption.Encrypt(value);

            return eValue;
        }

        [TestCase("pT145bY8WVH01apYvzopUQ==", ExpectedResult = "xyz")]
        [TestCase("7udxIRf+GvthPMLdGzu8DUmz94hhXmZmjit+Jlw/AKU=", ExpectedResult = "token_CSAYun2ldQUvRQ")]
        [TestCase("7nj5cPMhU8Ez+Qkkr7DXMpbLcx49Dth98XvBEwViJA4=", ExpectedResult = "token_CSAYun2ldQUvRQ")]
        [TestCase("O+/Jqd0ZFH0DeXRU9f3FA50s9JdvjKEJHfW9t2uAyPA=", ExpectedResult = "token_CSAYun2ldQUvRQ")]
        public string Decrypt_Given_Correct_Value(string eValue)
        {
            string value = "";
            List<string> tokenList = new List<string>();
            tokenList.Add("ZZd1dA0BFqYUtKIKfKxhETPogG8wnBy+UdeTE2OoR+0=");

            List<string> decrypt = new List<string>();
            foreach (string aValue in tokenList)
            {
                AES_Encryption aES_Encryption = new AES_Encryption();
                value = aES_Encryption.Decrypt(aValue);
                decrypt.Add(value);
            }


            return value;
        }
    }
}
