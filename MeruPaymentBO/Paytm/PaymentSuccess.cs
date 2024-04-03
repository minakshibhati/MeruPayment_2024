using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO.Paytm
{
    public class PaymentSuccess
    {
        public string Currency { get; set; }

        public string GatewayName { get; set; }

        public string ResponseMessage { get; set; }

        public string ResponseCode { get; set; }

        public string BankName { get; set; }

        public string PaymentMode { get; set; }

        public string CustomerId { get; set; }

        public string Status { get; set; }

        public string MerchantId { get; set; }

        public string MerchantUniqueValue { get; set; }

        public string OrderId { get; set; }

        public string TransactionId { get; set; }

        public string TransactionAmount { get; set; }

        public string BankTransactionId { get; set; }

        public string TransactionDateTime { get; set; }

        public string TransactionDate { get; set; }

    }
}