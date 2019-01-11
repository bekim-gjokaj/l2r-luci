using Discord.WebSocket;
using Luci.Jobs;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class SchedulerService
    {
        private IConfiguration _config { get; set; }
        private DiscordSocketClient _discord { get; set; }
        private IScheduler _scheduler; // after Start, and until shutdown completes, references the scheduler object
        private readonly IServiceProvider container;
        

        public SchedulerService(IConfiguration Config, DiscordSocketClient Discord, IServiceProvider container)
        {
            _config = Config;
            _discord = Discord;
            this.container = container;
        }

        public async Task StartAsync()
        {

            Dictionary<string, string> JobList = new Dictionary<string, string>
            {
                //{ "0 0/1 * * * ?", "Testing scheduler" },
                { "0 45 23 * * ?", "Daily quests reset in 15 minutes." },
                { "0 30 18 ? * 7", "Castle Siege in 30 minutes." },
                { "0 30 16 ? * 5", "Fort Siege in 30 minutes." },
                { "0 45 17 * * ?", "Guillotine spawns in 15 minutes." },
                { "0 45 15 * * ?", "Zaken spawns in 15 minutes." },
                { "0 45 6 * * ?", "Event Marsha spawns in 15 minutes." },
                { "0 45 10 * * ?", "Event Marsha spawns in 15 minutes." },
                { "0 45 18 * * ?", "Event Marsha spawns in 15 minutes." }
            };
            int counter = 1;

            // construct a scheduler factory
            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.JobFactory = new JobFactory(container);
            _scheduler.Start().Wait();


            foreach (string key in JobList.Keys)
            {
                /*********************************************************
                // ALERT JOBS
                *********************************************************/

                IJobDetail jobAlert = JobBuilder.Create<JobAlertMessage>()
                        .WithIdentity("Job" + counter, "group1")
                        .UsingJobData("jobSays", (string)JobList[key])
                        .Build();

                ITrigger triggerAlert = TriggerBuilder.Create()
                    .WithIdentity("Trigger" + counter, "group1")
                    .WithCronSchedule(key)
                    .ForJob("Job" + counter, "group1")
                    .Build();

                // Schedule the job using the job and trigger
                await _scheduler.ScheduleJob(jobAlert, triggerAlert);
                Console.WriteLine("*** Started Job - " + JobList[key]);
                counter++;
            }
        }
    }
}