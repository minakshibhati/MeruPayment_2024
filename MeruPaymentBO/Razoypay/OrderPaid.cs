using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO.Razoypay
{
    public class OrderPaid
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("contains")]
        public string[] Contains { get; set; }

        [JsonProperty("created_at")]
        public dynamic CreatedAt { get; set; }

        [JsonProperty("payload")]
        public PayLoad PayLoad { get; set; }
    }

    public class PayLoad
    {
        [JsonProperty("payment")]
        public Payment Payment { get; set; }

        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }

    public class Payment
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Order
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Invoice
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Subscription
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("entity")]
        public string _Entity { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("amount_paid")]
        public int AmountPaid { get; set; }

        [JsonProperty("amount_due")]
        public int AmountDue { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("receipt")]
        public string Receipt { get; set; }

        [JsonProperty("offer_id")]
        public object OfferId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("attempts")]
        public int Attempts { get; set; }

        [JsonProperty("notes")]
        public dynamic Notes { get; set; }

        [JsonProperty("created_at")]
        public dynamic CreatedAt { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("invoice_id")]
        public object InvoiceId { get; set; }

        [JsonProperty("international")]
        public bool International { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("amount_refunded")]
        public int AmountRefunded { get; set; }

        [JsonProperty("refund_status")]
        public object RefundStatus { get; set; }

        [JsonProperty("captured")]
        public bool Captured { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("card_id")]
        public dynamic CardId { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("wallet")]
        public dynamic Wallet { get; set; }

        [JsonProperty("vpa")]
        public dynamic VPA { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("contact")]
        public string Contact { get; set; }

        [JsonProperty("fee")]
        public int? Fee { get; set; }

        [JsonProperty("tax")]
        public int? Tax { get; set; }

        [JsonProperty("error_code")]
        public dynamic ErrorCode { get; set; }

        [JsonProperty("error_description")]
        public dynamic ErrorDescription { get; set; }

    }
}