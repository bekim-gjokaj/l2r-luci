using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpPcap;
using System;
using System.Threading.Tasks;

namespace Luci
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();        // Create a new instance of the config builder
            var env = Environment.GetEnvironmentVariables();
            string hostingEnv = (string)env["Hosting:Environment"];
            if (hostingEnv == "Bekim")
            {
                builder.SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddJsonFile("_configuration.Bekim.json");        // Add this (json encoded) file to the configuration
            }
            else if (hostingEnv == "Tiffany")
            {
                builder.SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddJsonFile("_configuration.Tiffany.json");        // Add this (json encoded) file to the configuration
            }
            else 
            {
                builder.SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddJsonFile("_configuration.json");        // Add this (json encoded) file to the configuration
            }

            Configuration = builder.Build();                // Build the configuration

            builder.AddEnvironmentVariables();
            ConfigHelper._configuration = Configuration;
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
            ConfigureServices(services);

            ServiceProvider provider = services.BuildServiceProvider();     // Build the service provider
            provider.GetRequiredService<LoggingService>();      // Start the logging service
            provider.GetRequiredService<CommandHandler>();      // Start the command handler service
            await provider.GetRequiredService<KillListService>().StartAsync();       // Start the startup service
            await provider.GetRequiredService<StartupService>().StartAsync();       // Start the startup service
            await provider.GetRequiredService<L2RPacketService>().StartAsync(ConfigureDevice());       // Start the packet service

            await Task.Delay(-1);                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            DiscordSocketClient discord = new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000             // Cache 1,000 messages per channel
            });

            L2RPacketService packetLogger = new L2RPacketService();
            KillListService killList = new KillListService();
            PacketHandlerService packetHandler = new PacketHandlerService(discord, killList, Configuration);

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
                        .AddSingleton(packetHandler)            // Add packetHandler to the collection
                        .AddSingleton(Configuration)            // Add the configuration to the collection
                        .AddSingleton(killList);                // Add the killList to the collection
        }

        private ICaptureDevice ConfigureDevice()
        {
            /* Retrieve the device list  part of initialization*/
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            int iface = L2RPacketService.Initialization(Kamael.Globals.args);
            ICaptureDevice device = CaptureDeviceList.Instance[Convert.ToInt32(Configuration["packets:interface"])];
            //Register our handler function to the 'packet arrival' event
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(PacketHandlerService.PacketCapturer);
            return device;
        }
    }
}