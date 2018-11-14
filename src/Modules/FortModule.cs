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


        [Command("Fort"), Priority(2)]
        [Summary("Get Luci's current status on the fortress siege")]
        public async Task Fort()
        {
            //Get attendance list
            var result = await _fort.GetEmbedAsync();

            await ReplyAsync("", false, result);

        }

        [Command("Fort"), Priority(1)]
        [Summary("Respond to fort attendance")]
        public async Task Fort(string Response)
        {
            //Initialize variables
            List<Embed> embeds = new List<Embed>();
            string response = string.Format(_config["fort:attendance:response"], Response);

            //Check response
            switch (Response.ToLower())
            {
                case "yes":
                    embeds = await _fort.AddAttendanceAsync(Context.User.Username, AttendanceResponseType.Yes);
                    break;
                case "no":
                    embeds = await _fort.AddAttendanceAsync(Context.User.Username, AttendanceResponseType.No);
                    break;
                case "maybe":
                    embeds = await _fort.AddAttendanceAsync(Context.User.Username, AttendanceResponseType.Maybe);
                    break;
                case "clear":
                    embeds = await _fort.ClearAsync();
                    break;
                default:
                    await ReplyAsync("Sorry! I didn't catch your response properly. Your answer will be added as a maybe.");
                    embeds = await _fort.AddAttendanceAsync(Context.User.Username, AttendanceResponseType.Maybe);
                    break;
            }

            foreach (var embed in embeds)
            {
                await ReplyAsync("", false, embed);
            }
        }


        [Command("Fort"), Priority(0)]
        [Summary("Respond to fort attendance")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task Fort(string Response, SocketGuildUser user)
        {
            //Initialize variables
            List<Embed> embeds = new List<Embed>();
            string response = string.Format(_config["fort:attendance:response"], Response);

            //Check response
            switch (Response.ToLower())
            {
                case "yes":
                    embeds = await _fort.AddAttendanceAsync(user.Username, AttendanceResponseType.Yes);
                    break;
                case "no":
                    embeds = await _fort.AddAttendanceAsync(user.Username, AttendanceResponseType.No);
                    break;
                case "maybe":
                    embeds = await _fort.AddAttendanceAsync(user.Username, AttendanceResponseType.Maybe);
                    break;
                case "clear":
                    embeds = await _fort.ClearAsync();
                    break;
                default:
                    await ReplyAsync("Sorry! I didn't catch your response properly. Your answer will be added as a maybe.");
                    embeds = await _fort.AddAttendanceAsync(user.Username, AttendanceResponseType.Maybe);
                    break;
            }

            foreach (var embed in embeds)
            {
                await ReplyAsync("", false, embed);
            }
        }

        [Command("FortClear")]
        [Summary("Clear the fortress siege list")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Clear()
        {
            //Get attendance list
            var result = await _fort.ClearAsync();

            foreach (var embed in result)
            {
                await ReplyAsync("", false, embed); 
            }

        }


    }

}
