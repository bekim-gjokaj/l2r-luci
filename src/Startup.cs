using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets;
using Luci.Jobs;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Luci
{
    public class Startup
    {
        public IConfiguration _config { get; set; }

        public Startup(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();                     // Create a new instance of the config builder
            System.Collections.IDictionary env = Environment.GetEnvironmentVariables();     // Get the environment variables
            builder.SetBasePath(AppContext.BaseDirectory);                                  // Specify the default location for the config file
            builder = SelectConfigureFilesAsync(builder, env);                                               // Select custom config.json files for Bekim or Tiff
            _config = builder.Build();                                                // Build the configuration
        }

        private IConfigurationBuilder SelectConfigureFilesAsync(IConfigurationBuilder builder, System.Collections.IDictionary env)
        {
            string hostingEnv = (string)env["Hosting:Environment"];

            if (hostingEnv == "Bekim")
            {
                builder.AddJsonFile("_configuration.Bekim.json", optional: true, reloadOnChange: true);        // Add this (json encoded) file to the configuration
            }
            else if (hostingEnv == "Tiffany")
            {
                builder.AddJsonFile("_configuration.Tiffany.json", optional: true, reloadOnChange: true);        // Add this (json encoded) file to the configuration
            }
            else
            {
                builder.AddJsonFile("_configuration.json", optional: false, reloadOnChange: true);        // Add this (json encoded) file to the configuration
            }

            return builder;
        }

        
        public static async Task RunAsync(string[] args) //Wrapper function for grabbing the args
        {
            Kamael.Globals.args = args;
            Startup startup = new Startup(args);
            //Continue to RunAsync below
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            //Create a new instance of a service collection
            //This is the main dependency injection container
            ServiceCollection services = new ServiceCollection();
            await ConfigureServicesAsync(services);

            //Start the services using dependency injection
            ServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<KillService>();
            provider.GetRequiredService<BountyService>();
            provider.GetRequiredService<PlayerService>();
            provider.GetRequiredService<SurveyService>();
            provider.GetRequiredService<L2RPacketService>();    //The packet logger dll
            await provider.GetRequiredService<SchedulerService>().StartAsync();
            //.StartAsync(); at the end is what kicks off the console app to start
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<PacketService>();       //The packet service for interpretting the packets
            // Keep the program alive
            await Task.Delay(-1);
        }

        private async Task ConfigureServicesAsync(IServiceCollection services)
        {
            DiscordSocketClient discord = new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000             // Cache 1,000 messages per channel
            });

            //L2RPacketService packetLogger = new L2RPacketService();
            //BountyService Bounty = new BountyService(Configuration);
            //FortService Fort = new FortService(Configuration);
            //KillService Kills = new KillService(Configuration, Bounty);

            //services.AddSingleton(packetLogger);             // Add packetLogger to the collection
            //services.AddSingleton(packetService);            // Add packetService to the collection
            //services.AddSingleton(Configuration);            // Add the configuration to the collection
            //services.AddSingleton(Kills);                    // Add the Kills to the collection

            services.AddSingleton(discord)
                        .AddSingleton(new CommandService(new CommandServiceConfig
                        {                                       // Add the command service to the collection
                            LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                            DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
                            CaseSensitiveCommands = false       // Ignore case when executing commands
                        }))
                        .AddSingleton(new JobAlertMessage(_config, discord))
                        .AddSingleton<StartupService>()
                        .AddSingleton<IConfiguration>(_config)
                        .AddSingleton<LoggingService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<Random>()
                        .AddSingleton<BountyService>()
                        .AddSingleton<PlayerService>()
                        .AddSingleton<SurveyService>()
                        .AddSingleton<KillService>()
                        .AddSingleton<SchedulerService>()
                        .AddSingleton<L2RPacketService>()
                        .AddSingleton<PacketService>();
            
        }
    }
    
}