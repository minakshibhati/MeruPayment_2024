using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeruCommonLibrary;
using MeruPaymentBO;
using Newtonsoft.Json;
using MeruPaymentDAL.DAL;
using MeruPaymentCore;
using System.Configuration;
using NLog;

namespace MeruPaymentBAL
{
    public class MeruPaymentRefundBAL
    {
        private static Logger loggerInfo;
        PaymentDAL dal = null;
        Razorpay razorPayManager = null;
        Paytm objPaytm = null;
        RabbitMQ queueManager = null;
        int maxRecordsToBeRead = 0;
        string dataFromQueue = "";
        string QueueName = "";

        public MeruPaymentRefundBAL()
        {
            loggerInfo = LogManager.GetCurrentClassLogger();
            dal = new PaymentDAL();
            razorPayManager = new Razorpay();
            objPaytm = new Paytm();
            queueManager = new RabbitMQ();
            maxRecordsToBeRead = Convert.ToInt32(ConfigurationManager.AppSettings["MaximumRecordsToBeProcessed"]);
            QueueName = ConfigurationManager.AppSettings["RefundProcessingQueue"];
        }

        public void ProcessRefund()
        {
            int counter = 0;
            try
            {
                RefundPayment refundPayment = new RefundPayment();
                Tuple<string, string, Dictionary<string, string>> returnRefundValue = null;

                while (counter < maxRecordsToBeRead)
                {
                    loggerInfo.Info("Reading " + counter + 1 + " element from Queue");
                    dataFromQueue = queueManager.Consume(QueueName);

                    RefundRequestBO refundObject = new RefundRequestBO();
                    if (dataFromQueue == null || dataFromQueue.Length == 0)
                    {
                        loggerInfo.Info("No data in merupaymentQ for refund");
                        break;
                    }
                    loggerInfo.Info("Data Retrieved from merupaymentQ " + dataFromQueue);
                    refundObject = JsonConvert.DeserializeObject<RefundRequestBO>(dataFromQueue);

                    returnRefundValue = refundPayment.ProcessRefund(refundObject.MId, refundObject.Amount, refundObject.Note);
                    if (returnRefundValue.Item1 != "200")
                    {
                        continue;
                    }

                    //PaymentBO paymentDetails = dal.GetMeruPaymentDetail(refundObject.MId);
                    //long totalAmountRefunded = paymentDetails.RefundAmount + refundObject.Amount;

                    //if (totalAmountRefunded > paymentDetails.Amount)
                    //{
                    //    loggerInfo.Warn("Cannot process refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + " as Requested Amount : " + refundObject.Amount + " is greater than total amount paid : " + paymentDetails.Amount);
                    //    continue;
                    //}

                    //switch (paymentDetails.PaymentSource)
                    //{
                    //    case PaymentGatway.Razorpay:
                    //        RazorpayRefundBO refundResponse = new RazorpayRefundBO();
                    //        if (paymentDetails.Amount == refundObject.Amount)
                    //        {
                    //            loggerInfo.Info("Processing Refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + "Refund Amount : " + refundObject.Amount);
                    //            refundResponse = razorPayManager.RefundPayment(paymentDetails.PaymentReferenceData2, refundObject.Amount, false, refundObject.Note);
                    //        }

                    //        if (paymentDetails.Amount > refundObject.Amount)
                    //        {
                    //            loggerInfo.Info("Processing Refund for Payment Id : " + paymentDetails.PaymentReferenceData2 + "Refund Amount : " + refundObject.Amount);
                    //            refundResponse = razorPayManager.RefundPayment(paymentDetails.PaymentReferenceData2, refundObject.Amount, true, refundObject.Note);
                    //        }
                    //        if (refundResponse == null)
                    //        {
                    //            loggerInfo.Info("Unable to process refund for payment id:" + paymentDetails.PaymentReferenceData2);
                    //        }
                    //        else
                    //        {
                    //            if (refundResponse.ErrorCode != null && refundResponse.ErrorCode.Length > 0)
                    //            {
                    //                loggerInfo.Error("Error occurred in Processing Refund Payment Id: " + paymentDetails.PaymentReferenceData2 + "Queueing data again. " + "Error Code: " + refundResponse.ErrorCode + "Error Desc:" + refundResponse.ErrorDescription);
                    //                bool ispublished = queueManager.Publish(QueueName, dataFromQueue);
                    //            }

                    //            loggerInfo.Info("Refund Processed for Payment Id : " + paymentDetails.PaymentReferenceData2 + "Refund Id : " + refundResponse.RefundId);
                    //            dal.RefundSuccess(paymentDetails.PaymentTransactionId.ToString(), refundResponse.RefundAmount.ToString(), refundResponse.RefundId);
                    //        }
                    //        break;
                    //    case PaymentGatway.PayTM:
                    //        //PayTMRefundBO objPayTMRefundBO = new PayTMRefundBO();
                    //        //objPaytm.RefundTransaction("", "", refundObject.Amount);
                    //        //TODO: Paytm refund
                    //        break;
                    //}
                    counter++;
                }
            }
            catch (Exception ex)
            {
                loggerInfo.Error(ex);
            }
            finally
            {
                //LogManager.Flush();
            }
        }
    }
}
