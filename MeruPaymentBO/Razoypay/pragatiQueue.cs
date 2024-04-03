

using Newtonsoft.Json;
namespace MeruPaymentBO.Razoypay
{
    public class pragatiQueue
    {
        [JsonProperty("MeruPaymentId")]
        public string MeruPaymentId { get; set; }

        [JsonProperty("CarNo")]
        public string CarNo { get; set; }

        [JsonProperty("SPId")]
        public string SPId { get; set; }

        [JsonProperty("Amount")]
        public long Amount { get; set; }

        [JsonProperty("PaymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty("PaymentSource")]
        public string PaymentSource { get; set; }

        [JsonProperty("PaymentId")]
        public string PaymentId { get; set; }
    }
}
