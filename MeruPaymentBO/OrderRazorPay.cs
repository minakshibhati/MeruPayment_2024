using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBO
{
    public class OrderBORazorPay
    {
        public string id { get; set; }

        public int amount { get; set; }

        public bool partial_payment { get; set; }

        public int amount_paid { get; set; }
        public int amount_due { get; set; }

        public string currency{ get; set; }

        public int receipt { get; set; }
        public string status{ get; set; }

        public int attempts { get; set; }
        public int OfferId { get; set; }

        public dynamic notes { get; set; }

        public string created_at { get; set; }

        public string mobile_number { get; set; }

        public string appsource { get; set; }

        public string  checksum { get; set; }

        public string email { get; set; }

        public string name { get; set; }

    }
}
