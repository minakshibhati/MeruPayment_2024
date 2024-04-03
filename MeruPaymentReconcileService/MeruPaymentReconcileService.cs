
using MeruCommonLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace MeruPaymentReconcileService
{
    public partial class MeruPaymentReconcileService : ServiceBase
    {
        private LogHelper objLogger = null;
        private Timer timer = null;
        bool isProcessingCompleted = false;
        Task ProcessReconciliationTask;

        public MeruPaymentReconcileService()
        {
            InitializeComponent();
            objLogger = new LogHelper("MeruPaymentReconcileService");
        }

        protected override void OnStart(string[] args)
        {
            isProcessingCompleted = true;
            try
            {
                timer = new Timer();
                this.timer.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["TimerIntervalInMinutes"]);//60 mins interval
                this.timer.Elapsed += new ElapsedEventHandler(this.Timer_tick);
                this.timer.Enabled = true;
                objLogger.WriteInfo("Meru Payment Reconciliation Service Started.");
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, DateTime.Now.ToString("dd-MM-yyyy HH:mmm:ss:fff") + ":Exception Occured At on Start Event.");
            }
        }

        private void Timer_tick(object sender, ElapsedEventArgs e)
        {
            //objLogger.WriteInfo("Meru Payment Reconciliation Timer Event Started ." + isProcessingCompleted.ToString());
            try
            {
                if (isProcessingCompleted)
                {
                    objLogger.WriteInfo("Meru Payment Reconciliation Timer Event Started.");
                    ProcessReconciliationTask = new Task(ProcessReconciliation);
                    isProcessingCompleted = false;
                    ProcessReconciliationTask.Start();
                }
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, DateTime.Now.ToString("dd-MM-yyyy HH:mmm:ss:fff") + ":Exception Occured Error:" + ex.Message);
            }
            finally
            {
                ProcessReconciliationTask = null;
            }
        }

        public void ProcessReconciliation()
        {
            //objLogger.WriteInfo("Reading from DB and Payment Service Provider for Reconciliation");

            try
            {
                //DateTime StartDate;
                //DateTime EndDate;
                //GetDate(out StartDate, out EndDate);

                MeruPaymentBAL.ReconcileBAL bal = new MeruPaymentBAL.ReconcileBAL();
                bal.Process();
                //bal.NewProcess(StartDate, EndDate);
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Exception Occured on Process Reconciliation Service");
            }
            finally
            {
                isProcessingCompleted = true;
                
            }
        }

        //private static void GetDate(out DateTime StartDate, out DateTime EndDate)
        //{
        //    bool UseCustomDate = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCustomDate"]);
        //    if (UseCustomDate)
        //    {
        //        StartDate = Convert.ToDateTime(ConfigurationManager.AppSettings["CustomStartDate"]);
        //        EndDate = Convert.ToDateTime(ConfigurationManager.AppSettings["CustomEndDate"]);
        //    }
        //    else
        //    {
        //        int hours = Convert.ToInt32(ConfigurationManager.AppSettings[""]);
        //        hours = -hours;
        //        StartDate = DateTime.Today.AddDays(-1);
        //        EndDate = DateTime.Today.AddMilliseconds(-1);
        //    }
        //}

        public void OnDebug()
        {
            // TODO: Add code here to perform any debug.
            try
            {
                //DateTime StartDate;
                //DateTime EndDate;
                //GetDate(out StartDate, out EndDate);

                MeruPaymentBAL.ReconcileBAL bal = new MeruPaymentBAL.ReconcileBAL();
                bal.Process();
            }
            catch (Exception ex)
            {
                objLogger.WriteError(ex, "Exception Occured on debug");
            }
            finally
            {

            }
        }

        protected override void OnStop()
        {
            objLogger.WriteInfo("Meru Payment Reconciliation Service Stopped.");
        }
    }
}
