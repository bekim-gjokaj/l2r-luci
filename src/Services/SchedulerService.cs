using Luci.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class SchedulerService
    {
        public SchedulerService()
        {
            
        }

        public async Task StartAsync()
        {
            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = await schedFact.GetScheduler();
            await sched.Start();

            Dictionary<string, object> JobList = new Dictionary<string, object>
            {
                { "23 30 0 * * ?", new JobAlertServerReset() },
                { "0 30 18 ? * 7", new JobAlertCastleSeige() },
                { "0 30 16 ? * 5", new JobAlertFortSiege() }
            };
            int counter = 1;

            foreach (string key in JobList.Keys)
            {
                /*********************************************************
                // ALERT JOBS
                *********************************************************/

                IJobDetail jobAlert = JobBuilder.Create(JobList[key].GetType())
                        .WithIdentity("Job" + counter, "group1")
                        .Build();

                ITrigger triggerAlert = TriggerBuilder.Create()
                    .WithIdentity("Trigger" + counter, "group1")
                    .WithCronSchedule(key)
                    .ForJob("Job" + counter, "group1")
                    .Build();

                // Schedule the job using the job and trigger
                await sched.ScheduleJob(jobAlert, triggerAlert);
                Console.WriteLine("*** Started Job - Job" + counter);
                counter++;
            }
        }
    }
}
