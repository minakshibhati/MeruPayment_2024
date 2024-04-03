using MeruCommonLibrary;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MeruPaymentBAL;
using System.Collections.Specialized;

namespace PaymentInstrumentDisableService
{
    public partial class PaymentInstrumentDisableService : ServiceBase
    {
        private LogHelper logHelper = null;
        //private CancellationTokenSource _cancellationTokenSource;
        private IScheduler scheduler;

        public PaymentInstrumentDisableService()
        {
            InitializeComponent();
            logHelper = new LogHelper("PaymentInstrumentDisableService");
        }

        protected override void OnStart(string[] args)
        {
            logHelper.MethodName = "OnStart(string[] args)";
            try
            {
                Disable_Unused_InActiveUser_PaymentInstrument().GetAwaiter().GetResult();
                //_cancellationTokenSource = new CancellationTokenSource();
                //await Task.Run(() => Disable_Unused_InActiveUser_PaymentInstrument(_cancellationTokenSource.Token));
            }
            catch (OperationCanceledException ocx)
            {
                logHelper.WriteError(ocx, "");
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "");
            }
            //_cancellationTokenSource = null;
        }

        protected override void OnStop()
        {
            logHelper.MethodName = "OnStop()";
            try
            {
                logHelper.WriteInfo("------- Shutting Down ---------------------");

                scheduler.Shutdown();//true);

                logHelper.WriteInfo("------- Shutdown Complete -----------------");
                //if (_cancellationTokenSource != null)
                //{
                //    logHelper.WriteInfo("------- Cancelling Task ----------------------");
                //    _cancellationTokenSource.Cancel();
                //    logHelper.WriteInfo("------- Task Cancelled ----------------------");
                //}
            }
            catch (SchedulerException se)
            {
                logHelper.WriteError(se, "");
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "");
            }
        }

        protected async Task Disable_Unused_InActiveUser_PaymentInstrument()//CancellationToken ct)
        {
            logHelper.MethodName = "Disable_Unused_InActiveUser_PaymentInstrument(CancellationToken ct)";

            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.scheduler.instanceName", "Payment_Instrument_Disable_Job" },
                    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                    { "quartz.threadPool.threadCount", "1" }
                };


                logHelper.WriteInfo("------- Getting Scheduler Instance -------------------");
                ISchedulerFactory factory = new StdSchedulerFactory(props);
                scheduler = await factory.GetScheduler();

                logHelper.WriteInfo("------- Starting Scheduler ----------------");
                await scheduler.Start();

                // define the job and tie it to our DisablePaymentInstrumentJob class
                IJobDetail job = JobBuilder.Create<DisablePaymentInstrumentJob>()
                    .WithIdentity("Payment_Instrument_Disable_Job", "Payment_Background_Service")
                    .Build();

                //// Trigger the job to run now, and then repeat every 10 seconds
                //ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                //.WithIdentity("Payment_Instrument_Disable_Service_Trigger", "Payment_Background_Service")
                //.StartNow()
                //.WithCronSchedule("0 1 * * *")
                //.Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("Payment_Instrument_Disable_Service_Trigger", "Payment_Background_Service")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithInterval(new TimeSpan(0, 0, 1, 0))
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                DateTimeOffset ft = await scheduler.ScheduleJob(job, trigger);
                logHelper.WriteInfo(string.Format("{0} will run at: {1}", job.Key, ft));
            }
            catch (SchedulerException se)
            {
                logHelper.WriteError(se, "");
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "");
            }
        }
    }

    internal class DisablePaymentInstrumentJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() => { new DisablePaymentInstrument().Process(); });
        }
    }
}
