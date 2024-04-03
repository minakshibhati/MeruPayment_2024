using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class DeleteCard
    {
        private LogHelper _logHelper;
        public DeleteCard()
        {
            _logHelper = new LogHelper("DeleteCard");
        }

        public Tuple<string, string, bool> ProcessRequest(string customerId, string tokenId, int cardId)
        {
            try
            {
                Razorpay razorpay = new Razorpay();
                Tuple<string, string, bool> resultDeleteCard = razorpay.DeleteCard(customerId, tokenId);

                if (resultDeleteCard.Item1 != "200")
                {
                    return resultDeleteCard;
                }

                AuthCardDAL authCardDAL = new AuthCardDAL();
                resultDeleteCard = authCardDAL.DeleteCard(new CardDeleteBO
                {
                    CardId = cardId
                });

                if (resultDeleteCard.Item1 != "200")
                {
                    return resultDeleteCard;
                }

                return new Tuple<string, string, bool>(
                            "200",
                            "Success",
                            true);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while processing auth card delete.");
                return new Tuple<string, string, bool>(
                    "500",
                    ex.Message,
                    false);
            }
        }
    }
}
