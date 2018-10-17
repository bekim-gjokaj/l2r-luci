using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static Luci.KillListService;

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



        public static async Task NotifyKill(PacketClanMemberKillNotify kill)
        {
            KillListItem killItem = await KillListService.ProcessKillAsync(kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name);

            //dictRecentKills.Add(builder);
            if (killItem != null)
            {

                foreach (SocketGuild guild in _discord.Guilds)
                {
                    foreach (SocketTextChannel textchan in guild.TextChannels)
                    {
                        if (textchan.Name == _config["killlist:clanchannel"] && (kill.Clan2Name == _config["killlist:clanname"] || kill.ClanName == _config["killlist:clanname"]))
                        {
                            string strFormat = "";
                            Bounty bounty = await KillListService.FindBountyByNameAsync(kill.Player2Name);
                            if (bounty != null)
                            {
                                strFormat = _config["killlist:bountyformat"];
                            }
                            else if (kill.ClanName == _config["killlist:clanname"])
                            {
                                strFormat = _config["killlist:victoryformat"];
                            }

                            else
                            {
                                strFormat = _config["killlist:defeatformat"];
                            }

                            SocketUser P1User = null;//await FindUser(killItem.P1);
                            SocketUser P2User = null;//await FindUser(killItem.P2);

                            string result = string.Format(strFormat,
                                    (P1User == null) ? killItem.P1 : P1User.Mention,
                                    (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                    killItem.Clan1,
                                    (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                    (P2User == null) ? killItem.P2 : P2User.Mention,
                                    (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                    killItem.Clan2,
                                    (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                    DateTime.Now);
                            await textchan.SendMessageAsync(result);

                        }



                        //SPECIAL PLAYER
                        if (kill.PlayerName == _config["killlist:specialname"] && textchan.Name == _config["killlist:specialchannel"])
                        {


                            SocketUser P1User = await UtilService.FindUser(killItem.P1);
                            SocketUser P2User = await UtilService.FindUser(killItem.P2);


                            string docbuilder = string.Format(_config["killlist:specialformat"],
                                    (P1User == null) ? killItem.P1 : P1User.Mention,
                                    (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                    killItem.Clan1,
                                    (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                    (P2User == null) ? killItem.P2 : P2User.Mention,
                                    (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                    killItem.Clan2,
                                    (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                    DateTime.Now);
                            await textchan.SendMessageAsync(docbuilder);
                        }

                    }
                }
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
