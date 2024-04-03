using MeruCommonLibrary;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruPaymentDAL.EntityModel;
using System.Data.Entity;

namespace MeruPaymentDAL
{
    public class AuthCardDAL
    {
        private LogHelper _logHelper;
        public AuthCardDAL()
        {
            _logHelper = new LogHelper("AuthCardDAL()");
        }

        public Tuple<string, string, Dictionary<string, string>> SaveCard(CardBO cardBO)
        {
            _logHelper.MethodName = "SaveCard(CardBO cardBO)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details where r.Token_ID == cardBO.PGCardTokenId select r).FirstOrDefault();
                    if (card != null)
                    {
                        return new Tuple<string, string, Dictionary<string, string>>("200", "Card already exists in DB.", null);
                    }

                    card = new tbl_Payment_Customer_Card_Details
                    {
                        Name_On_Card = cardBO.FullName,
                        Network_Name = cardBO.Network,
                        Last_Four_Digit = cardBO.Last4,
                        Token_ID = cardBO.PGCardTokenId,
                        Provider_Customer_ID = cardBO.PGCustomerId,
                        Is_Default = false,
                        Card_Type = cardBO.CardType,
                        Card_Issuer_Code = cardBO.Issuer,
                        Card_Status = 1,
                        Record_Created_DateTime = DateTime.Now,
                        Record_Update_DateTime = DateTime.Now,
                        Card_Exipry_DateTime = cardBO.ExpityDateTime,
                        IsDeleted = false,
                        DeviceId = cardBO.DeviceId
                    };

                    db.tbl_Payment_Customer_Card_Details.Add(card);
                    db.SaveChanges();
                }

                return new Tuple<string, string, Dictionary<string, string>>(
                    "200",
                    "Success",
                    null);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while upserting auth cards.");
                return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CardBO> GetCardById(int id)
        {
            _logHelper.MethodName = "GetCardById(int id)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details
                                join cust in db.tbl_Payment_Customer_Details on r.Provider_Customer_ID equals cust.Provider_Customer_ID
                                where r.ID == id
                                select new { r, cust }).FirstOrDefault();
                    if (card == null)
                    {
                        return new Tuple<string, string, CardBO>("500", string.Format("Card with id {0} does not exists in DB.", id), null);
                    }

                    if (card.cust.Customer_Status != (int)CustomerStatus.Active)
                    {
                        return new Tuple<string, string, CardBO>("500", string.Format("Card with id {0} cannot be pull as the customer is blocked in DB.", id), null);
                    }


                    return new Tuple<string, string, CardBO>(
                    "200",
                    "Success",
                    new CardBO
                    {
                        CardType = card.r.Card_Type,
                        ExpiryMonth = card.r.Card_Exipry_DateTime.Value.Month,
                        ExpiryYear = card.r.Card_Exipry_DateTime.Value.Year,
                        ExpityDateTime = card.r.Card_Exipry_DateTime.Value,
                        FullName = card.r.Name_On_Card,
                        Issuer = card.r.Card_Issuer_Code,
                        Last4 = card.r.Last_Four_Digit,
                        Network = card.r.Network_Name,
                        PGCardTokenId = card.r.Token_ID,
                        PGCustomerId = card.r.Provider_Customer_ID,
                        PaymentGateway = (PaymentGatway)card.cust.Payment_Gateway_ID
                    });
                }


            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting card by Id.");
                return new Tuple<string, string, CardBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CardBO> GetCardDetail(string issurcode, string lastfour, string network, string cardtype, DateTime cardExpiryDateTime)
        {
            _logHelper.MethodName = "GetCardDetail(string bankcode, string lastfour, string network, string cardtype)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details
                                where r.Card_Issuer_Code == issurcode
                                && r.Last_Four_Digit == lastfour
                                && r.Network_Name == network
                                && r.Card_Type == cardtype
                                && r.Card_Exipry_DateTime == cardExpiryDateTime
                                select r).OrderByDescending(a => a.Record_Created_DateTime).FirstOrDefault();
                    if (card == null)
                    {
                        return new Tuple<string, string, CardBO>("500",
                            string.Format("Card with issurcode {0}, lastfour {1} network {2} cardtype {3} does not exists in DB.", issurcode, lastfour, network, cardtype),
                            null);
                    }

                    return new Tuple<string, string, CardBO>(
                        "200",
                        "Success",
                        new CardBO
                        {
                            CardType = card.Card_Type,
                            ExpiryMonth = card.Card_Exipry_DateTime.Value.Month,
                            ExpiryYear = card.Card_Exipry_DateTime.Value.Year,
                            ExpityDateTime = card.Card_Exipry_DateTime.Value,
                            FullName = card.Name_On_Card,
                            Issuer = card.Card_Issuer_Code,
                            Last4 = card.Last_Four_Digit,
                            Network = card.Network_Name,
                            PGCardTokenId = card.Token_ID,
                            PGCustomerId = card.Provider_Customer_ID,
                        });
                }
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

        public Tuple<string, string, CardBO> GetValidCardById_Mobile(int id, string mobile)
        {
            _logHelper.MethodName = "GetValidCardById_Mobile(int id, string mobile)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details
                                join cust in db.tbl_Payment_Customer_Details on r.Provider_Customer_ID equals cust.Provider_Customer_ID
                                where r.ID == id && cust.Customer_Mobile_No == mobile && r.IsDeleted == false && r.Card_Status == 1 // 1= active
                                select new { r, cust }).FirstOrDefault();
                    if (card == null)
                    {
                        return new Tuple<string, string, CardBO>("500",
                            string.Format("Card with id {0}, mobile {1} does not exists in DB.", id, mobile),
                            null);
                    }

                    if (card.cust.Customer_Status != (int)CustomerStatus.Active)
                    {
                        return new Tuple<string, string, CardBO>("500",
                            string.Format("Card with id {0} cannot be pulled as mobile {1} is not active in DB.", id, mobile),
                            null);
                    }

                    return new Tuple<string, string, CardBO>(
                    "200",
                    "Success",
                    new CardBO
                    {
                        CardType = card.r.Card_Type,
                        ExpiryMonth = card.r.Card_Exipry_DateTime.Value.Month,
                        ExpiryYear = card.r.Card_Exipry_DateTime.Value.Year,
                        ExpityDateTime = card.r.Card_Exipry_DateTime.Value,
                        FullName = card.r.Name_On_Card,
                        Issuer = card.r.Card_Issuer_Code,
                        Last4 = card.r.Last_Four_Digit,
                        Network = card.r.Network_Name,
                        PGCardTokenId = card.r.Token_ID,
                        PGCustomerId = card.r.Provider_Customer_ID,
                        PaymentGateway = (PaymentGatway)card.cust.Payment_Gateway_ID
                    });
                }


            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting valid card by Id.");
                return new Tuple<string, string, CardBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, CardBO> GetLatestValidCardByMobile(string mobile)
        {
            _logHelper.MethodName = "GetLatestValidCardByMobile(string mobile)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details
                                join cust in db.tbl_Payment_Customer_Details on r.Provider_Customer_ID equals cust.Provider_Customer_ID
                                where cust.Customer_Mobile_No == mobile && r.IsDeleted == false && r.Card_Status == 1 // 1= active
                                && cust.Customer_Status == (int)CustomerStatus.Active
                                select new { r, cust }).OrderByDescending(o => o.r.Record_Created_DateTime).FirstOrDefault();
                    if (card == null)
                    {
                        return new Tuple<string, string, CardBO>("500",
                            string.Format("No active Card available for mobile {0} in DB.", mobile),
                            null);
                    }

                    return new Tuple<string, string, CardBO>(
                    "200",
                    "Success",
                    new CardBO
                    {
                        CardType = card.r.Card_Type,
                        ExpiryMonth = card.r.Card_Exipry_DateTime.Value.Month,
                        ExpiryYear = card.r.Card_Exipry_DateTime.Value.Year,
                        ExpityDateTime = card.r.Card_Exipry_DateTime.Value,
                        FullName = card.r.Name_On_Card,
                        Issuer = card.r.Card_Issuer_Code,
                        Last4 = card.r.Last_Four_Digit,
                        Network = card.r.Network_Name,
                        PGCardTokenId = card.r.Token_ID,
                        PGCustomerId = card.r.Provider_Customer_ID,
                        PaymentGateway = (PaymentGatway)card.cust.Payment_Gateway_ID,
                        RecordCreatedDateTime = Convert.ToDateTime(card.r.Record_Created_DateTime)
                    });
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while getting latest valid card by Id. mobile:" + mobile);
                return new Tuple<string, string, CardBO>(
                    "500",
                    ex.Message,
                    null);
            }
        }

        public Tuple<string, string, bool> DeleteCard(CardDeleteBO cardDeleteBO)
        {
            _logHelper.MethodName = "DeleteCard(CardDeleteBO cardDeleteBO)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var card = (from r in db.tbl_Payment_Customer_Card_Details where r.ID == cardDeleteBO.CardId select r).FirstOrDefault();
                    if (card == null)
                    {
                        return new Tuple<string, string, bool>("400", "Card does not exists in DB.", false);
                    }
                    card.IsDeleted = true;
                    db.Entry(card).State = EntityState.Modified;
                    int result = db.SaveChanges();
                    if (result <= 0)
                    {
                        _logHelper.WriteInfo("No record set as deleted for card id " + cardDeleteBO.CardId.ToString());
                        return new Tuple<string, string, bool>(
                            "500",
                            "No record deleted",
                            false);
                    }
                }
                return new Tuple<string, string, bool>(
                    "200",
                    "Success",
                    true);

            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while deleting card.");
                return new Tuple<string, string, bool>(
                    "500",
                    ex.Message,
                    false);
            }
        }

        public Tuple<string, string, bool> DeleteAllCardByMobile(string Mobile)
        {
            _logHelper.MethodName = "DeleteAllCardByMobile(string Mobile)";
            try
            {
                using (CDSBusinessEntities db = new CDSBusinessEntities())
                {
                    var cards = (from card in db.tbl_Payment_Customer_Card_Details
                                 join cust in db.tbl_Payment_Customer_Details
                                     on card.Provider_Customer_ID equals cust.Provider_Customer_ID
                                 where cust.Customer_Mobile_No == Mobile && card.IsDeleted == false
                                 select card).ToList();

                    if (cards == null || cards.Count == 0)
                    {
                        return new Tuple<string, string, bool>("200", "Card does not exists in DB.", true);
                    }

                    cards.ForEach(r =>
                    {
                        r.IsDeleted = true;
                        r.Record_Update_DateTime = Convert.ToDateTime(DateTime.Now);
                    });

                    int result = db.SaveChanges();

                    if (result <= 0)
                    {
                        _logHelper.WriteInfo("Failed to delete auth cards for mobile" + Mobile);
                        return new Tuple<string, string, bool>(
                            "500",
                            "Failed to delete auth cards for mobile" + Mobile,
                            false);
                    }
                }

                return new Tuple<string, string, bool>(
                    "200",
                    "Success",
                    true);
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Error occured while deleting card for mobile " + Mobile);
                return new Tuple<string, string, bool>(
                    "500",
                    ex.Message,
                    false);
            }
        }

        public CardBO GetBlackListedCard(CardBO cardDetails)
        {
            _logHelper.MethodName = "GetBlackListedCard(CardBO cardDetails)";
            CardBO dbBlaclListCard = new CardBO();
            try
            {
                using (CDSBusinessEntities entities = new CDSBusinessEntities())
                {
                    dbBlaclListCard = entities.Tbl_Payment_BlackListedCards
                        .Where(x => x.IssuerCode == cardDetails.Issuer.ToLower() && x.CardType == cardDetails.CardType.ToLower() && x.NetworkName == cardDetails.Network.ToLower() && x.IsActive == true)
                        .Select(x =>
                        new CardBO
                        {
                            Network = x.NetworkName,
                            Issuer = x.IssuerCode,
                            CardType = x.CardType
                        }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logHelper.WriteError(ex, "Exception occured while fetching black listed card details");
            }
            return dbBlaclListCard;
        }

    }
}
