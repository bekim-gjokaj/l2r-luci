using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets;
using Luci.Jobs;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public PacketService PacketSvc;

        public Startup(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();        // Create a new instance of the config builder
            System.Collections.IDictionary env = Environment.GetEnvironmentVariables();
            builder.SetBasePath(AppContext.BaseDirectory);      // Specify the default location for the config file
            SelectConfiguFiles(builder, env);
            Configuration = builder.Build();                // Build the configuration
            builder.AddEnvironmentVariables();
            ConfigService._configuration = Configuration;
        }
        public static async Task RunAsync(string[] args)
        {
            Kamael.Globals.args = args;
            Startup startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            ServiceCollection services = new ServiceCollection();             // Create a new instance of a service collection
            ConfigureServicesAsync(services);
            ServiceProvider provider = services.BuildServiceProvider();     // Build the service provider
            provider.GetRequiredService<LoggingService>();      // Start the logging service
            provider.GetRequiredService<CommandHandler>();      // Start the command handler service
            await provider.GetRequiredService<KillListService>().StartAsync();       // Start the startup service
            await provider.GetRequiredService<StartupService>().StartAsync();       // Start the startup service
            await provider.GetRequiredService<L2RPacketService>().StartAsync(ConfigureDevice());       // Start the packet service

            await Task.Delay(-1);                               // Keep the program alive
        }


        private static void SelectConfiguFiles(IConfigurationBuilder builder, System.Collections.IDictionary env)
        {
            string hostingEnv = (string)env["Hosting:Environment"];

            if (hostingEnv == "Bekim")
                builder.AddJsonFile("_configuration.Bekim.json", optional: false, reloadOnChange: true);        // Add this (json encoded) file to the configuration
            else if (hostingEnv == "Tiffany")
                builder.AddJsonFile("_configuration.Tiffany.json", optional: false, reloadOnChange: true);        // Add this (json encoded) file to the configuration
            else
                builder.AddJsonFile("_configuration.json", optional: false, reloadOnChange: true);        // Add this (json encoded) file to the configuration

        }

        private async void ConfigureServicesAsync(IServiceCollection services)
        {
            DiscordSocketClient discord = new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000             // Cache 1,000 messages per channel
            });

            L2RPacketService packetLogger = new L2RPacketService();
            KillListService killList = new KillListService();
            PacketService packetService = new PacketService(discord, killList, Configuration);
            PacketSvc = packetService;
            await UtilService.StartAsync(discord, killList, Configuration);

            services.AddSingleton(discord)
                        .AddSingleton(new CommandService(new CommandServiceConfig
                        {                                       // Add the command service to the collection
                            LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                            DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
                            CaseSensitiveCommands = false       // Ignore case when executing commands
                        }))
                        .AddSingleton<StartupService>()         // Add startupservice to the collection
                        .AddSingleton<LoggingService>()         // Add loggingservice to the collection
                        .AddSingleton<CommandHandler>()         // Add loggingservice to the collection
                        .AddSingleton<Random>()                 // Add random to the collection
                        .AddSingleton(packetLogger)             // Add packetLogger to the collection
                        .AddSingleton(packetService)            // Add packetService to the collection
                        .AddSingleton(Configuration)            // Add the configuration to the collection
                        .AddSingleton(killList);                // Add the killList to the collection

            

            await InitializeScheduler();
        }

        private static async Task InitializeScheduler()
        {
            
            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = await schedFact.GetScheduler();
            await sched.Start();


            Dictionary<string, object> JobList = new Dictionary<string, object>();
            JobList.Add("JobAlertServerReset", new JobAlertServerReset());
            JobList.Add("JobAlertCastleSeige", new JobAlertCastleSeige());
            JobList.Add("JobAlertFortSiege", new JobAlertFortSiege());
            int counter = 1;

            foreach(var key in JobList.Keys)
            {

                /*********************************************************
                // SERVER RESET ALERT JOB
                *********************************************************/

                IJobDetail jobAlert = JobBuilder.Create(JobList[key].GetType())
                        .WithIdentity("Job" + counter, "group1")
                        .Build();

                ITrigger triggerAlert = TriggerBuilder.Create()
                    .WithIdentity("Trigger" + counter, "group1")
                    .WithCronSchedule("30 12 * * * ?")
                    .ForJob("Job" + counter, "group1")
                    .Build();

                // Schedule the job using the job and trigger
                await sched.ScheduleJob(jobAlert, triggerAlert);
                counter++;
            }
            
        }

        public delegate int MyDelegate (string s);

        private ICaptureDevice ConfigureDevice()
        {
            /* Retrieve the device list  part of initialization*/
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            int iface = L2RPacketService.Initialization(Kamael.Globals.args);
            ICaptureDevice device = CaptureDeviceList.Instance[Convert.ToInt32(Configuration["packets:interface"])];
            //Register our handler function to the 'packet arrival' event
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(PacketSvc.PacketCapturer);
            return device;
        }
    }
}