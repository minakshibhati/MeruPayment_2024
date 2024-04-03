using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using MeruCommonLibrary;
using MeruPaymentCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MeruPaymentDAL.DAL;
using MeruPaymentDAL;

namespace MeruPaymentBAL
{
    public class CommonMethods
    {
        private static Logger objLogger;
        private PaymentDAL paymentDAL = null;

        public CommonMethods()
        {
            objLogger = LogManager.GetCurrentClassLogger();
            paymentDAL = new PaymentDAL();
        }

        public PaymentMethod GetPaymentMethod(string objPaymentMethod)
        {
            if (objPaymentMethod == null)
            {
                objPaymentMethod = "";
            }

            try
            {
                switch (objPaymentMethod.ToLower())
                {
                    case "card":
                        return PaymentMethod.card;
                    case "debit":
                        return PaymentMethod.debit;
                    case "credit":
                        return PaymentMethod.credit;
                    case "netbanking":
                        return PaymentMethod.netbanking;
                    case "wallet":
                        return PaymentMethod.wallet;
                    case "emi":
                        return PaymentMethod.emi;
                    case "upi":
                        return PaymentMethod.upi;
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return PaymentMethod.Unknown;
        }

        public bool ValidateData_HMACSHAH256(string Checksum, string Data, string Secret)
        {
            bool returnValue = false;

            try
            {
                using (HMACSHA256Hash objHash = new HMACSHA256Hash(Secret))
                {
                    returnValue = objHash.ValidateData(Checksum, Data);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }

            return returnValue;
        }

        public bool PushToQueue(string QueueName, string Value)
        {
            bool returnValue = false;
            try
            {
                using ( RabbitMQ objRabbitMQ = new RabbitMQ())
                {
                    returnValue = objRabbitMQ.Publish(QueueName, Value);
                    if (!returnValue)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            returnValue = objRabbitMQ.Publish(QueueName, Value);
                            if (returnValue)
                            {
                                break;
                            }
                        }
                    }
                }
                if (returnValue)
                {
                    objLogger.Info(string.Format("Data: {0} pushed to queue :{1}", Value, QueueName));
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnValue;
        }

        public bool UpdateRazorPostPaymentDetail(string mpid, string RazorPaymentId)
        {
            bool returnStatus = false;
            try
            {
                RazorpayCardBO objRazorpayCardBO = null;
                Razorpay objRazorpay = new Razorpay();
                RazorpayPaymentBO objRazorPaymentBO = objRazorpay.GetPaymentDetail(RazorPaymentId);
                JObject objDetail = new JObject();
                if (objRazorPaymentBO.PaymentMethod == PaymentMethod.card)
                {
                    objRazorpayCardBO = objRazorpay.GetCardDetail(objRazorPaymentBO.PaymentMethodDetail);

                    if (objRazorpayCardBO.CardType.ToLower() == "credit")
                    {
                        objRazorPaymentBO.PaymentMethod = PaymentMethod.credit;
                    }
                    else if (objRazorpayCardBO.CardType.ToLower() == "debit")
                    {
                        objRazorPaymentBO.PaymentMethod = PaymentMethod.debit;
                    }

                    objDetail.Add(new JProperty("Name", objRazorpayCardBO.FullName));
                    objDetail.Add(new JProperty("Last4", objRazorpayCardBO.Last4));
                    objDetail.Add(new JProperty("Issuer", objRazorpayCardBO.Issuer));
                    objDetail.Add(new JProperty("International", objRazorpayCardBO.IsInternational));
                    objDetail.Add(new JProperty("Emi", objRazorpayCardBO.IsEMI));
                }
                else
                    objDetail.Add(new JProperty("Issuer", objRazorPaymentBO.PaymentMethodDetail));
                PaymentDAL objPaymentDAL = new PaymentDAL();
                returnStatus = objPaymentDAL.UpdatePostTransactionSuccess(mpid, objDetail.ToString(Formatting.None), PaymentGatway.Razorpay, objRazorPaymentBO.PaymentMethod);
            }
            catch (Exception ex)
            {
                objLogger.Error(ex);
            }
            return returnStatus;
        }

        public void ExecuteProcedure(PaymentBO dbPaymentDetails, string Action)
        {
            try
            {
                if (dbPaymentDetails != null)
                    objLogger.Info("Data Received in Execute Procedure method : " + Newtonsoft.Json.JsonConvert.SerializeObject(dbPaymentDetails,Formatting.None));

                using (UpdatePaymentDetailsWebhookDAL updater = new UpdatePaymentDetailsWebhookDAL())
                {
                    updater.UpdatePostPaymentDetails(dbPaymentDetails, Action);
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured in method ExecuteProcedure(PaymentBO dbPaymentDetails, string Action)");
            }
        }
    }
}
