using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Luci.Models;

namespace Luci.Services
{
    public static class UtilService
    {
        private static DiscordSocketClient _discord;
        private static IConfiguration _config;

        // DiscordSocketClient, CommandService, and IConfiguration are injected automatically from the IServiceProvider
        public static async Task StartAsync(DiscordSocketClient discord, IConfiguration config)
        {
            _config = config;
            _discord = discord;
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

        public static async Task SendMessage(Embed message, string channel)
        {

            foreach (SocketGuild guild in _discord.Guilds)
            {
                foreach (SocketTextChannel textchan in guild.TextChannels)
                {
                    if (textchan.Name == channel)
                    {
                        await textchan.SendMessageAsync("", false, message);
                    }
                }
            }
        }


        public static async Task<SocketUser> FindUser(string Name)
        {
            try
            {
                foreach (SocketGuild guild in _discord.Guilds)
                {
                    foreach (SocketUser user in guild.Users)
                    {
                        if (user.Username.ToLower().Contains(Name.ToLower()))
                        {
                            return user;

                        }



                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task NotifyClanChat(PacketChatGuildListReadResult chat)
        {

            string builder = string.Format(":speech_balloon: ***{0}:\t\t*** **{1}** _\t@ {2}_", chat.PlayerName, chat.Message, DateTime.Now);
            //dictRecentKills.Add(builder);

            foreach (SocketGuild guild in _discord.Guilds)
            {
                foreach (SocketTextChannel textchan in guild.TextChannels)
                {
                    if (textchan.Name == "in-game-clan-chat")
                    {
                        await textchan.SendMessageAsync(builder);
                    }
                }
            }
        }

    }
}
