using MeruCommonLibrary;
using MeruPaymentBAL;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PaymentLinkIssueService
{
    public partial class PaymentLinkIssueService : ServiceBase
    {
        private LogHelper logHelper = null;
        private IScheduler scheduler;
        private int RuningFrequencyMinutes = 1;

        public PaymentLinkIssueService()
        {
            InitializeComponent();
            logHelper = new LogHelper("PaymentLinkIssueService");
            RuningFrequencyMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["RuningFrequencyInMinutes"]);
        }

        protected override void OnStart(string[] args)
        {
            logHelper.MethodName = "OnStart(string[] args)";

            try
            {
                CreateAndSend_PaymentLink().GetAwaiter().GetResult();
            }
            catch (OperationCanceledException ocx)
            {
                logHelper.WriteError(ocx, "");
            }
            catch (Exception ex)
            {
                logHelper.WriteError(ex, "");
            }
        }

        protected override void OnStop()
        {
            logHelper.MethodName = "OnStop()";
            try
            {
                logHelper.WriteInfo("------- Shutting Down ---------------------");

                scheduler.Shutdown();

                logHelper.WriteInfo("------- Shutdown Complete -----------------");
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

        protected async Task CreateAndSend_PaymentLink()
        {
            logHelper.MethodName = "CreateAndSend_PaymentLink()";

            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.scheduler.instanceName", "PaymentLinkIssueJob" },
                    { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                    { "quartz.threadPool.threadCount", "1" }
                };


                logHelper.WriteInfo("------- Getting Scheduler Instance -------------------");
                ISchedulerFactory factory = new StdSchedulerFactory(props);
                scheduler = await factory.GetScheduler();

                logHelper.WriteInfo("------- Starting Scheduler ----------------");
                await scheduler.Start();

                // define the job and tie it to our DisablePaymentInstrumentJob class
                IJobDetail job = JobBuilder.Create<PaymentLinkIssueJob>()
                    .WithIdentity("PaymentLinkIssueJob", "Payment_Background_Service")
                    .Build();

                //// Trigger the job to run now, and then repeat every 10 seconds
                //ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                //.WithIdentity("Payment_Instrument_Disable_Service_Trigger", "Payment_Background_Service")
                //.StartNow()
                //.WithCronSchedule("0 1 * * *")
                //.Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("PaymentLinkIssueJob", "Payment_Background_Service")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithInterval(new TimeSpan(0, 0, RuningFrequencyMinutes, 0))
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

        internal class PaymentLinkIssueJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.Run(() => { new PaymentLinkIssue().ProcessLink(); });
            }
        }
    }
}
