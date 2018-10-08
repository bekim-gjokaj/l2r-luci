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
            var builder = new ConfigurationBuilder()        // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory)      // Specify the default location for the config file
                .AddJsonFile("_configuration.json");        // Add this (json encoded) file to the configuration
            Configuration = builder.Build();                // Build the configuration
        }

        public static async Task RunAsync(string[] args)
        {
            Kamael.Globals.args = args;
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();             // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();     // Build the service provider
            provider.GetRequiredService<LoggingService>();      // Start the logging service
            provider.GetRequiredService<CommandHandler>();      // Start the command handler service

            await provider.GetRequiredService<StartupService>().StartAsync();       // Start the startup service

            await provider.GetRequiredService<L2RPacketService>().StartAsync(ConfigureDevice());       // Start the startup service

            await Task.Delay(-1);                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000             // Cache 1,000 messages per channel
            });

            var packetLogger = new L2RPacketService();
            var packetHandler = new PacketHandler(discord);

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
                        .AddSingleton(packetLogger)                 // Add random to the collection
                        .AddSingleton(packetHandler)                 // Add random to the collection
                        .AddSingleton(Configuration);           // Add the configuration to the collection
        }

        private ICaptureDevice ConfigureDevice()
        {
            /* Retrieve the device list  part of initialization*/
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            var iface = L2RPacketService.Initialization(Kamael.Globals.args);
            ICaptureDevice device = CaptureDeviceList.Instance[iface];
            //Register our handler function to the 'packet arrival' event
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(PacketHandler.PacketCapturer);
            return device;
        }
    }
}
