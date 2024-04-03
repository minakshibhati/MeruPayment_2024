using Newtonsoft.Json;

namespace MeruPaymentBO.Razoypay
{
    public class SucbscriptionCharged
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
}
