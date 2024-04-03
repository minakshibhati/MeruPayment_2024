using MeruCommonLibrary;
using MeruPaymentBO;
using MeruPaymentCore;
using MeruPaymentDAL.DAL;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPaymentBAL
{
    public class RefundPayment
    {
        private static Logger loggerInfo;
        private PaymentDAL dal = null;
        private Razorpay razorPayManager = null;
        private Paytm objPaytm = null;
        private RabbitMQ queueManager = null;
        private string QueueName = "";

        public RefundPayment()
        {
            loggerInfo = LogManager.GetCurrentClassLogger();
            dal = new PaymentDAL();
            razorPayManager = new Razorpay();
            objPaytm = new Paytm();
            queueManager = new RabbitMQ();
            QueueName = ConfigurationManager.AppSettings["RefundProcessingQueue"];
        }

        internal Tuple<string, string, Dictionary<string, string>> ProcessRefund(string paymentId, Int64 amount, string note)
        {
            try
            {
                PaymentBO paymentDetails = dal.GetMeruPaymentDetail(paymentId);
                long totalAmountRefunded = paymentDetails.RefundAmount + amount;

                if (totalAmountRefunded > paymentDetails.Amount)
                {
                    loggerInfo.Warn("Cannot process refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + " as Requested Amount : " + amount + " is greater than total amount paid : " + paymentDetails.Amount);
                    return new Tuple<string, string, Dictionary<string, string>>(
                    "500",
                    "Cannot process refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + " as Requested Amount : " + amount + " is greater than total amount paid : " + paymentDetails.Amount,
                    null);
                }

                switch (paymentDetails.PaymentSource)
                {
                    case PaymentGatway.Razorpay:
                        RazorpayRefundBO refundResponse = new RazorpayRefundBO();

                        loggerInfo.Info("Processing Refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + "Refund Amount : " + amount);
                        refundResponse = razorPayManager.RefundPayment(paymentDetails.PaymentReferenceData2, amount, (paymentDetails.Amount > amount), note);

                        if (refundResponse == null)
                        {
                            loggerInfo.Info("Unable to process refund for payment id:" + paymentDetails.PaymentReferenceData2);

                            return new Tuple<string, string, Dictionary<string, string>>(
                            "500",
                            "Unable to process refund for payment id:" + paymentDetails.PaymentReferenceData2,
                            null);
                        }

                        if (refundResponse.ErrorCode != null && refundResponse.ErrorCode.Length > 0)
                        {
                            loggerInfo.Error("Error occurred in Processing Refund Payment Id: " + paymentDetails.PaymentReferenceData2 + "Queueing data again. " + "Error Code: " + refundResponse.ErrorCode + "Error Desc:" + refundResponse.ErrorDescription);

                            bool ispublished = queueManager.Publish(QueueName, JsonConvert.SerializeObject(new RefundRequestBO { Action = "REFUND", Amount = amount, MId = paymentId, Note = note }, Formatting.None));
                        }

                        loggerInfo.Info("Refund Processed for Payment Id : " + paymentDetails.PaymentReferenceData2 + "Refund Id : " + refundResponse.RefundId);
                        dal.RefundSuccess(paymentDetails.PaymentTransactionId.ToString(), refundResponse.RefundAmount.ToString(), refundResponse.RefundId);

                        Dictionary<string, string> returnValue = new Dictionary<string, string>();
                        returnValue.Add("RazorpayRefundId", refundResponse.RefundId);

                        return new Tuple<string, string, Dictionary<string, string>>(
                        "200",
                        "Success",
                        returnValue);

                    case PaymentGatway.PayTM:
                        //PayTMRefundBO objPayTMRefundBO = new PayTMRefundBO();
                        //objPaytm.RefundTransaction("", "", refundObject.Amount);
                        //TODO: Paytm refund

                        return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Not implemented.",
                        null);

                }
                return new Tuple<string, string, Dictionary<string, string>>(
                        "500",
                        "Unknown payment source.",
                        null);
            }
            catch (Exception ex)
            {
                loggerInfo.Error(ex, "Error while processing refund for payment id:" + paymentId);

                return new Tuple<string, string, Dictionary<string, string>>(
                "500",
                "Error while processing refund for payment id:" + paymentId,
                null);
            }
        }
    }
}
