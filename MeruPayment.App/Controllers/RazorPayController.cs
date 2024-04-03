using AutoMapper;
using MeruCommonLibrary;
using MeruPaymentBAL;
using MeruPaymentBO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Configuration;
using MeruPaymentBO.Razoypay;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeruPayment.App.Controllers
{
    //public class SuccessCallBackResponse 
    //{
    //    public string Order_Id  { get; set; }
    //    public string razorpay_payment_id { get; set; }
    //    public string razorpay_order_id { get; set; }
    //    public string razorpay_signature { get; set; }

    //    public string status_code { get; set; }

    //    public string appsource { get; set; }
    //    public string CheckSum { get; set; }

    //    public string mobilenum { get; set; }
    //}

    public class RazorPayOrderDet
    {
        public int amount { get; set; }

        public string currency { get; set; }

        public string receipt  { get; set; }

        public string notes  { get; set; }
    }
    public class SuccessCallResponse
    {
        public SuccessCallResponse(int successCode, string message)
        {
            StatusCode = successCode;
            StatusMessage = message;
            
        }

        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }


    }

    public class Response
    {
        public Response(int successCode, string message, dynamic responsedetails)
        {
            StatusCode = successCode;
            StatusMessage = message;
            ResponseDetails = responsedetails;
        }
        
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public dynamic ResponseDetails { get; set; }

    }
    public class RazorPayController : ApiController
    {
        
        private SuccessCallBackResponse _successCallBackResponse = null;
        // GET: RazorPay

        
        [Route("CreateOrder")]
        [HttpPost] 
        /*API call after CreateOrder
        Added on 22-02-2023 for RazorPaySDK Integration*/
        public IHttpActionResult CreateOrder([FromBody] OrderBORazorPay det)
        {
            LogHelper objLogger = new LogHelper("RazorPaymentAPIController");
            try
            {
                //OrderBORazorPay det = new OrderBORazorPay();
               
                
                CreateOrder createOrder = new CreateOrder();
                Response res;                
                det.checksum = Convert.ToString(HttpContext.Current.Request.Headers["Checksum"]);
                objLogger.WriteInfo("Calling Create Order API - Request:" + JsonConvert.SerializeObject(det));

                Tuple<string, string, Dictionary<string, string>> returnValue = createOrder.ProcessRequest_DA(det);

                if (returnValue.Item1 == "200")
                {
                    objLogger.WriteInfo(string.Format(" Order Id created successfully"+returnValue.Item3));
                    return Content(HttpStatusCode.OK, new Response(0, " Order Id created successfully",returnValue.Item3));
                }
                else
                {
                    objLogger.WriteInfo(string.Format(" Order Id creation Failed", returnValue.Item2));
                    return Content(HttpStatusCode.OK, new Response(1, " Order Id creation Failed", returnValue.Item2));
                }
                   
            }
            catch(Exception ex)
            {
                return Content(HttpStatusCode.ExpectationFailed, "exception :" + ex.Message);
                objLogger.WriteError(ex,string.Format(" Order Id creation Failed"+det.id.ToString()));
                objLogger.WriteError(ex, "exception details : "+ ex.Message);
            }
        }
        #region oldsuccesscallback
        /*API for SuccessCallbak
        Added on 22-02-2023 for RazorPaySDK Integration*/

        [Route("SuccessCallBack")]
        [HttpPost]
        public IHttpActionResult SuccessCallBack(SuccessCallBackResponse resp)
        {
            RazorCheckoutResponseBAL objRazorCheckoutResponseBAL = new RazorCheckoutResponseBAL();
            LogHelper objLogger = new LogHelper("RazorPaymentAPIController");
            objLogger.WriteInfo("Success CallBack API - Request:"+ JsonConvert.SerializeObject(resp));

            string LogData = "";
            try
            {

                Response res;
                PaymentBO objPaymentBO = objRazorCheckoutResponseBAL.GetMeruPaymentDetail(resp.PaymentId);
                if (objPaymentBO == null)
                {

                    objLogger.WriteInfo(string.Format("Payment data is not present for Meru payment Id {0}", resp.PaymentId));

                }
                resp.CheckSum = Convert.ToString(HttpContext.Current.Request.Headers["Checksum"]);
                ValidatePaymentSignature validate = new ValidatePaymentSignature();
                bool isValidSignature = validate.verifyPayment(resp, ConfigurationManager.AppSettings["Razor_Key_Secret"]);
               
                //bool isValidSignature = objRazorCheckoutResponseBAL.ValidateResponseSignature(resp.razorpay_signature, resp.razorpay_order_id + "|" + resp.razorpay_payment_id, ConfigurationManager.AppSettings["Razor_Key_Secret"]);
                LogData = "Razorpaysignature:" + resp.razorpay_signature + ",OrderId:" + resp.razorpay_order_id + "PaymentID:" + resp.razorpay_payment_id;
                LogData = LogData + "Method Name:objRazorCheckoutResponseBAL.ValidateResponseSignature";

             
                #region GET SOURCE DETAIL

                PaymentRequestSystemMasterBO objPaymentRequestSystemMasterBO = objRazorCheckoutResponseBAL.GetSourceDetail(resp.appsource);
                if (objPaymentRequestSystemMasterBO == null)
                {
                    objLogger.WriteInfo(string.Format("Source is either unknown or null. Source: {0}", LogData.ToString()));

                }
                
                #endregion

                //update database
                Tuple<string, string, Boolean> returnValue = objRazorCheckoutResponseBAL.UpdatePaymentResponseDetail_DA(resp);
                
                objPaymentBO = objRazorCheckoutResponseBAL.GetMeruPaymentDetail(resp.PaymentId);

                if (!isValidSignature)
                {
                    objLogger.WriteInfo(string.Format("Razorpay API response signature mismatch {0}", LogData.ToString()));

                    return Content(HttpStatusCode.OK, new SuccessCallResponse(1, "Validation failed for OrderId:" + resp.razorpay_order_id));

                }
                else
                {
                    objLogger.WriteInfo(string.Format("Razorpay API response signature Matched {0}", LogData.ToString()));

                    //bool retvalue = objRazorCheckoutResponseBAL.UpdatePaymentResponseDetail_DA(resp.razorpay_signature, resp.Order_Id, resp.razorpay_payment_id, resp.razorpay_order_id, resp.status_code,resp);
                    if (returnValue.Item3)
                    {
                        #region PUSH RESPONSE TO QUEUE FOR SUCCESS

                        //if (objPaymentRequestSystemMasterBO.QueueName != null && objPaymentRequestSystemMasterBO.QueueName.Length > 0)
                        //{

                        //    if (!objRazorCheckoutResponseBAL.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, objPaymentBO))
                        //    {
                        //        objLogger.WriteInfo(string.Format("Failed to push transaction data to queue: {0} {1}", objPaymentRequestSystemMasterBO.QueueName, LogData.ToString()));
                        //    }
                        //}
                        pragatiQueue queueObject = new pragatiQueue();
                        try
                        {
                            // RazorCheckoutResponseBAL objRazorCheckoutResponseBAL = new RazorCheckoutResponseBAL();
                            //objPaymentRequestSystemMasterBO  dbSourceDetails = objRazorCheckoutResponseBAL.GetSourceDetail(requestResouce);
                            dynamic RequestReferenceVal = JObject.Parse(objPaymentBO.RequestReferenceVal);
                            queueObject.MeruPaymentId = objPaymentBO.PaymentTransactionId;
                            queueObject.CarNo = RequestReferenceVal.CarNo;
                            queueObject.SPId = RequestReferenceVal.SPId;
                            queueObject.Amount = objPaymentBO.Amount;
                            queueObject.PaymentMethod = objPaymentBO.PaymentMethod.ToString();
                            queueObject.PaymentSource = objPaymentBO.PaymentSource.ToString();
                            queueObject.PaymentId = objPaymentBO.PaymentReferenceData2;
                        }
                        catch (Exception ex)
                        {
                            objLogger.WriteError(ex, "Exception Occured While Binding Data to PragatiQueue Model.s");
                        }

                        #region Push Into Queue
                        if (!string.IsNullOrWhiteSpace(objPaymentRequestSystemMasterBO.QueueName))
                        {
                            try
                            {
                                CommonMethods commonMethods = new CommonMethods();
                                commonMethods.PushToQueue(objPaymentRequestSystemMasterBO.QueueName, Newtonsoft.Json.JsonConvert.SerializeObject(queueObject, Formatting.None));
                            }
                            catch (Exception ex)
                            {
                                objLogger.WriteError(ex, "Exception Occured during Send data to Queue" + ex.Message);
                            }
                        }
                        #endregion

                        #endregion


                        objLogger.WriteInfo(string.Format("Razorpay API responseUpdated in DB {0}", resp.ToString()));

                        return Content(HttpStatusCode.OK, new SuccessCallResponse(0, "Payment Successfull - " + returnValue.Item2));

                    }
                    else
                    {
                        objLogger.WriteInfo(string.Format("Failure in Razorpay API responseUpdation  in DB {0} ", resp.ToString()));

                        return Content(HttpStatusCode.OK, new SuccessCallResponse(0, "Payment update failed" + resp.razorpay_order_id));

                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.ExpectationFailed, "exception :" + ex.Message);
            }


        }
        #endregion
       

    }
}