using Newtonsoft.Json;
using System.Collections.Generic;

namespace MeruPaymentBO.Paytm
{
    public class AcceptRefund
    {
        [JsonProperty("head")]
        public Head Head { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }
    }

    public class Head
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public class Body
    {
        [JsonProperty("txnId")]
        public string TxnId { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("txnAmount")]
        public string TxnAmount { get; set; }

        [JsonProperty("refundAmount")]
        public string RefundAmount { get; set; }

        [JsonProperty("txnTimestamp")]
        public string TxnTimeStamp { get; set; }

        [JsonProperty("acceptRefundTimestamp")]
        public string AcceptRefundTimeStamp { get; set; }

        [JsonProperty("refId")]
        public string RefId { get; set; }

        [JsonProperty("merchantRefundRequestTimestamp")]
        public string MerchantRefundRequestTimeStamp { get; set; }

        [JsonProperty("refundId")]
        public string RefundId { get; set; }


        /* This property is used only for Success Refund Event*/
        [JsonProperty("userCreditInitiateTimestamp")]
        public string UserCreditInitiateTimestamp { get; set; }

        [JsonProperty("refundDetailInfoList")]
        public List<RefundDetailsInfoList> RefundInfoList { get; set; }
        /* This property is used only for Success Refund Event*/
    }

    public class RefundDetailsInfoList
    {
        [JsonProperty("userMobileNo")]
        public string UserMobileNo { get; set; }

        [JsonProperty("refundType")]
        public string RefundType { get; set; }

        [JsonProperty("payMethod")]
        public string PayMethod { get; set; }

        [JsonProperty("refundAmount")]
        public string RefundAmout { get; set; }

        [JsonProperty("rrn")]
        public string RRN { get; set; }

        [JsonProperty("issuingBankName")]
        public string IssuingBankName { get; set; }

        [JsonProperty("maskedVpa")]
        public string MaskedVpa { get; set; }

        [JsonProperty("maskedCardNumber")]
        public string MaskedCardNumber { get; set; }

        [JsonProperty("maskedBankAccountNumber")]
        public string MaskedBankAccountNumber { get; set; }

        [JsonProperty("cardScheme")]
        public string CardScheme { get; set; }

        [JsonProperty("userCreditExpectedDate")]
        public string UserCreditExpectedDate { get; set; }
    }
}
