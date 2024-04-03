using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class GetAuthCard
    {
        private LogHelper _logHelper;
        private Tuple<string, string, CardBO> returnValue = null;
        private AuthCardDAL _authCardDAL = null;

        public GetAuthCard()
        {
            _logHelper = new LogHelper("GetAuthCard()");
            _authCardDAL = new AuthCardDAL();
        }

        public Tuple<string, string, CardBO> ByCardId(int cardId)
        {
            _logHelper.MethodName = "ByCardId(string cardId)";
            try
            {
                Tuple<string, string, CardBO> returnCardData = _authCardDAL.GetCardById(cardId);
                if (returnCardData.Item1 != "200")
                {
                    return returnCardData;
                }
                     
                AES_Encryption aES_Encryption = new AES_Encryption();
                returnCardData.Item3.PGCardTokenId = aES_Encryption.Decrypt(returnCardData.Item3.PGCardTokenId);

                return returnCardData;// _authCardDAL.GetCardById(cardId);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting card detail.");
                return new Tuple<string, string, CardBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CardBO> ByValidCardId_Mobile(int cardId, string mobile)
        {
            _logHelper.MethodName = "ByCardId_Mobile_Email(int cardId, string mobile, string email)";
            try
            {
                Tuple<string, string, CardBO> returnCardData = _authCardDAL.GetValidCardById_Mobile(cardId, mobile);

                if (returnCardData.Item1 != "200")
                {
                    return returnCardData;
                }

                AES_Encryption aES_Encryption = new AES_Encryption();
                returnCardData.Item3.PGCardTokenId = aES_Encryption.Decrypt(returnCardData.Item3.PGCardTokenId);

                return returnCardData;//_authCardDAL.GetValidCardById_Mobile(cardId, mobile);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting card detail.");
                return new Tuple<string, string, CardBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }
    }
}
