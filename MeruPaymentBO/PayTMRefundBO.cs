using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PayTMRefundBO
    {
        public string MerchandId { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string TransactionAmount { get; set; }
        public string RefundAmount { get; set; }
        public string TotalRefundAmount { get; set; }
        public string TransactionDate { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string RefundMeruRefId { get; set; }
        public string CardIssuer { get; set; }
        public string BankTransactionId { get; set; }
        public string PaymentMode { get; set; }
        public string RefundDate { get; set; }
        public string RefundType { get; set; }
        public string RefundPaytmRefId { get; set; }
    }
}
