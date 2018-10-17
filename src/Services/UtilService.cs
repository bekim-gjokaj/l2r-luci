using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Luci.Services
{
    public static class UtilService
    {
        public static DiscordSocketClient _discord;
        public static KillListService _killList;
        public static IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public static async Task StartAsync(DiscordSocketClient discord, KillListService killList, IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
            _killList = killList;
        }
        

        public static async Task SendMessage(string message, string channel)
        {
            
            foreach (SocketGuild guild in _discord.Guilds)
            {
                foreach (SocketTextChannel textchan in guild.TextChannels)
                {
                    if (textchan.Name == channel)
                    {
                        await textchan.SendMessageAsync(message);
                    }
                }
            }
        }
    }
}
