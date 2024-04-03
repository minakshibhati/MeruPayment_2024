using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class PaymentLinkBO
    {
        public string Payment_Transaction_ID { get; set; }
        public string Request_RefId { get; set; }
        public long Payment_Amount_Paise { get; set; }
        public string PG_OrderId { get; set; }
        public string PG_PaymentId { get; set; }
        public string PG_InvoiceId { get; set; }
        public string PG_ReceiptNo { get; set; }
        public string PG_Ref_Data { get; set; }
        public int? Payment_Source { get; set; }
        public int? Payment_Method { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}
