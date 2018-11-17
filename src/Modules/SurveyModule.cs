using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Luci.Models.Enums;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci.Modules
{
    [Name("Survey")]
    [Summary("Commands for maintaining asking surveys and logging responses.")]
    public class SurveyModule : ModuleBase<SocketCommandContext>
    {
        private IConfiguration _config { get; set; }
        private SurveyService _survey { get; set; }
        private DiscordSocketClient _discord { get; set; }

        public SurveyModule(IConfiguration config, SurveyService survey, DiscordSocketClient discord)
        {
            _config = config;
            _survey = survey;
            _discord = discord;
        }

        [Command("Survey List")]
        [Summary("Ask Luci to list the surveys:\r\n Example: !survey list")]
        public async Task List()
        {
            string list = await _survey.GetSurveyStringList();
            await ReplyAsync("Survey list:\r\n" + list);
        }

        public async Task<IConfigurationSection> FindSurveyByName(string SurveyName)
        {
            IConfigurationSection configSurveys = _config.GetSection("surveys");

            foreach (IConfigurationSection configSurvey in configSurveys.GetChildren())
            {
                if (SurveyName.ToLower() == configSurvey.Key)
                {
                    return configSurvey;
                }
            }

            return null;
        }

        [Command("Ask")]
        [Summary("Prompt Luci to ask a survey:\r\n Example: !ask {surveyname}")]
        public async Task Ask(string Survey)
        {
            ulong guildId = 0;
            ulong channelId = 0;
            string surveymsg = "";

            IConfigurationSection configSurvey = await FindSurveyByName(Survey.ToLower());
            if (Survey.ToLower() == configSurvey.Key)
            {
                ulong.TryParse(_config[$"{configSurvey.Path}:guildId"], out guildId);
                ulong.TryParse(_config[$"{configSurvey.Path}:channelId"], out channelId);
                surveymsg = _config[$"{configSurvey.Path}:msg"];
            }

            await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(surveymsg);

            //Luci will send message asking for roll call
            //ReplyAsync(_config["survey:attendance:msg"], _config["survey:attendance:channel"]);
        }

        [Command("Survey"), Priority(2)]
        [Summary("Get Luci's current status on the survey siege")]
        public async Task Survey(string Survey)
        {
            //Get attendance list
            Embed result = await _survey.GetEmbedAsync(Survey);

            await ReplyAsync("", false, result);
        }

        [Command("Respond"), Priority(1), Alias("R")]
        [Summary("Respond to survey attendance")]
        public async Task Survey(string Survey, string Response)
        {
            //Initialize variables
            List<Embed> embeds = new List<Embed>();

            //Check response
            switch (Response.ToLower())
            {
                case "yes":
                    embeds = await _survey.AddResponseAsync(Survey, Context.User.Username, SurveyResponseType.Yes);
                    break;

                case "no":
                    embeds = await _survey.AddResponseAsync(Survey, Context.User.Username, SurveyResponseType.No);
                    break;

                case "maybe":
                    embeds = await _survey.AddResponseAsync(Survey, Context.User.Username, SurveyResponseType.Maybe);
                    break;

                case "clear":
                    embeds = await _survey.ClearAsync(Survey);
                    break;

                default:
                    await ReplyAsync("Sorry! I didn't catch your response properly. Your answer will be added as a maybe.");
                    embeds = await _survey.AddResponseAsync(Survey, Context.User.Username, SurveyResponseType.Maybe);
                    break;
            }

            foreach (Embed embed in embeds)
            {
                await ReplyAsync("", false, embed);
            }
        }

        [Command("Survey"), Priority(0), Alias("R")]
        [Summary("Respond to survey attendance")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Survey(string Survey, string Response, SocketGuildUser user)
        {
            //Initialize variables
            List<Embed> embeds = new List<Embed>();

            IConfigurationSection configSurvey = await FindSurveyByName(Survey.ToLower());
            string response = string.Format(_config[$"{configSurvey.Path}:responsemsg"], Response);

            //Check response
            switch (Response.ToLower())
            {
                case "yes":
                    embeds = await _survey.AddResponseAsync(Survey, user.Username, SurveyResponseType.Yes);
                    break;

                case "no":
                    embeds = await _survey.AddResponseAsync(Survey, user.Username, SurveyResponseType.No);
                    break;

                case "maybe":
                    embeds = await _survey.AddResponseAsync(Survey, user.Username, SurveyResponseType.Maybe);
                    break;

                case "clear":
                    embeds = await _survey.ClearAsync(Survey);
                    break;

                default:
                    await ReplyAsync("Sorry! I didn't catch your response properly. Your answer will be added as a maybe.");
                    embeds = await _survey.AddResponseAsync(Survey, user.Username, SurveyResponseType.Maybe);
                    break;
            }

            foreach (Embed embed in embeds)
            {
                await ReplyAsync("", false, embed);
            }
        }

        [Command("SurveyClear")]
        [Summary("Clear the survey siege list")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Clear(string Survey)
        {
            //Get attendance list
            List<Embed> result = await _survey.ClearAsync(Survey);

            foreach (Embed embed in result)
            {
                await ReplyAsync("", false, embed);
            }
        }
    }
}