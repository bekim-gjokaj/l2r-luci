using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Luci.Models.Enums;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillService;

namespace Luci.Modules
{
    [Name("Fort")]
    [Summary("Commands for maintaining fort attendance, teams, and reminders")]
    public class FortModule : ModuleBase<SocketCommandContext>
    {

        private IConfiguration _config { get; set; }
        private FortService _fort { get; set; }
        private DiscordSocketClient _discord { get; set; }

        public FortModule (IConfiguration config, FortService fort, DiscordSocketClient discord)
        {
            _config = config;
            _fort = fort;
            _discord = discord;
        }
       
        [Command("Ask Attendance")]
        [Summary("Prompt Luci to ask who's coming")]
        public async Task Attendance()
        {
            var guildId = Context.Guild.Id;
            var channels = Context.Guild.Channels;
            ulong channelId = new ulong();
            foreach(var item in channels)
            {
                if(item.Name == _config["fort:attendance:channel"])
                {
                    channelId = item.Id;
                }
            }
            await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(_config["fort:attendance:msg"]);
            //Luci will send message asking for roll call
            //ReplyAsync(_config["fort:attendance:msg"], _config["fort:attendance:channel"]);
        }

        
        [Command("Fort")]
        [Summary("Get Luci's current status on the fortress siege")]
        public async Task Fort()
        {
            //Get attendance list
            var result = await _fort.GetEmbedAsync();
            
            await ReplyAsync("", false, result);

        }

        [Command("Fort")]
        [Summary("Respond to fort attendance")]
        public async Task Fort(string Response)
        {
            //Initialize variables
            Embed embed;
            string response = string.Format(_config["fort:attendance:response"], Response);

            //Check response
            switch (Response.ToLower())
            {
                case "yes":
                    embed = await _fort.AttendanceAdd(Context.User.Username, AttendanceResponseType.Yes);
                    await ReplyAsync(response, false, embed);
                    break;

                case "no":
                    embed = await _fort.AttendanceAdd(Context.User.Username, AttendanceResponseType.No);
                    await ReplyAsync(response, false, embed);
                    break;
                case "maybe":
                    embed = await _fort.AttendanceAdd(Context.User.Username, AttendanceResponseType.Maybe);
                    await ReplyAsync(response, false, embed);
                    break;
                default:
                    await ReplyAsync("Sorry! I didn't catch your response properly. Your answer will be addes as a maybe.");
                    break;
            }
        }

    }

}
