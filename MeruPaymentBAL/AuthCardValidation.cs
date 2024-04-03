using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class AuthCardValidation
    {
        public AuthCardValidation()
        {

        }

        public Tuple<string, string, bool> Validate()
        {
            //TODO: Implementation
            return new Tuple<string, string, bool>("200", "Success", true);
        }
    }
}
