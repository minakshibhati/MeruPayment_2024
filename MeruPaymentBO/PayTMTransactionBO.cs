using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PayTMTransactionBO
    {
        public string MerchantId { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string BankTransactionId { get; set; }
        public string TransactionAmount { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string TransactionDate { get; set; }
        public string GatewayName { get; set; }
        public string BankName { get; set; }
        public string PaymentMode { get; set; }
        public string TransactionType { get; set; }
        public string RefundAmount { get; set; }
        public string CustomerId { get; set; }
    }
}
