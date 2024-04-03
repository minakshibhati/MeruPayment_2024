using MeruPaymentBO;
using MeruPaymentDAL;
using System;

namespace MeruPaymentBAL
{
    public class ValidateCard : IDisposable
    {
        #region Private Fields
        private bool _IsValid = false;
        private AuthCardDAL objAuthCardDAL;
        #endregion

        #region Public Properties and Methods
        public bool IsValid(CardBO cardDetails)
        {
            CardBO dbBlaclListCard = new CardBO();
            objAuthCardDAL = new AuthCardDAL();
            dbBlaclListCard = objAuthCardDAL.GetBlackListedCard(cardDetails);
            if (dbBlaclListCard == null)
            {
                _IsValid = true;
            }

            return _IsValid;
        }
        #endregion

        public void Dispose() { }
    }
}
