using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Timers;
using System.Configuration;
using MeruPaymentBAL;

namespace MeruPaymentRefund
{
    public partial class RefundQueueListener : ServiceBase
    {
        #region RESOURCE_DECLARE
        private static Logger objLogger;
        private Timer timer = null;
        bool isProcessingAllRefundsCompleted = false;
        Task ProcessRefundsFromQueue;
        #endregion

        public RefundQueueListener()
        {
            InitializeComponent();
            objLogger = LogManager.GetCurrentClassLogger();
        }

        protected override void OnStart(string[] args)
        {
            isProcessingAllRefundsCompleted = true;
            try
            {
                timer = new Timer();
                this.timer.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMinutes"]);//60 mins interval
                this.timer.Elapsed += new ElapsedEventHandler(this.Timer_tick);
                timer.Enabled = true;
                objLogger.Info("Meru Payment Refund Service Started.");
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured At on Start Event." );
            }
        }

        private void Timer_tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (isProcessingAllRefundsCompleted)
                {
                    ProcessRefundsFromQueue = new Task(ProcessRefund);
                    isProcessingAllRefundsCompleted = false;
                    ProcessRefundsFromQueue.Start();
                }
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured Error");
            }
            finally
            {
                ProcessRefundsFromQueue = null;
            }
        }

        public void OnDebug()
        {
            //// TODO: Add code here to perform any debug.
            //MeruPaymentRefundBAL bal = new MeruPaymentRefundBAL();
            //try
            //{
            //    bal.ProcessRefund();
            //}
            //catch (Exception ex)
            //{
            //    objLogger.Error(ex,":Exception Occured on debug. Error:" + ex.Message);
            //}
            //finally
            //{
                
            //}
            ProcessRefund();
        }

        public void ProcessRefund()
        {
            objLogger.Info("Reading from meruPaymentRefundQ started and Processing Refund");

            try
            {
                MeruPaymentRefundBAL bal = new MeruPaymentRefundBAL();
                bal.ProcessRefund();
            }
            catch (Exception ex)
            {
                objLogger.Error(ex, "Exception Occured on Process Refund Service");
            }
            finally
            {
                isProcessingAllRefundsCompleted = true;
                objLogger.Info("Meru Payment Refund Service completed");
            }
        }

        protected override void OnStop()
        {
            objLogger.Info("Meru Payment Refund Service Stopped.");
        }
    }
}
