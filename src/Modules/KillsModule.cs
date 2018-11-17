using Discord.Commands;
using Luci.Models;
using Luci.Models.Enums;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci.Modules
{
    [Name("Kills")]
    [Summary("Kill List")]
    public class KillsModule : ModuleBase<SocketCommandContext>
    {

        private IConfiguration _config { get; set; }
        private KillService _kills { get; set; }

        public KillsModule(IConfiguration Configuration, KillService Kills)
        {
            _config = Configuration;
            _kills = Kills;
        }

        [Command("Recent")]
        [Summary("Recent Kills")]
        public async Task Recent()
        {
            List<KillsItem> KillLog = await _kills.GetKillLogAsync();
            if (KillLog.Count != 0)
            {

                List<Discord.Embed> result = await _kills.GetRecentKillsAsync();

                //Return formatted string to Discord
                foreach (Discord.Embed item in result)
                {
                    await ReplyAsync("", false, item);
                }
            }
            else
            {
                await ReplyAsync("Kill List Currently Empty.");
            }
        }

        [Command("kills vs")]
        public async Task KillCountByVersusAsync(string P1, string P2)
        {
            List<Discord.Embed> result = await _kills.KillCountByVersusAsync(P1, P2);
            //Return formatted string to Discord
            foreach (Discord.Embed item in result)
            {
                await ReplyAsync("", false, item);
            }
        }

        [Command("kills tag me")]
        public async Task KillsTagMeAsync(string PlayerName)
        {
            await _kills.AddPlayerTagging(Context.User, PlayerName);
            await ReplyAsync("Ok, I'll tag you from now on.");

        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Kills for")]
        [Summary("My Kills")]
        public async Task KillCountByPlayerAsync(string player)
        {
            int killdaily = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 1);
            int killweekly = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 7);
            int killmonthly = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 30);
            int deathdaily = await _kills.GetDeathCountByPlayerAsync(player, KillsType.Personal, 1);
            int deathweekly = await _kills.GetDeathCountByPlayerAsync(player, KillsType.Personal, 7);
            int deathmonthly = await _kills.GetDeathCountByPlayerAsync(player, KillsType.Personal, 30);
            string response = "";

            switch (player)
            {
                default:
                    response = string.Format(_config["kills:formats:killsfor"],
                                                player,
                                                killdaily,
                                                deathdaily,
                                                killweekly,
                                                deathweekly,
                                                killmonthly,
                                                deathmonthly);
                    break;
            }

            await ReplyAsync(response);


            await ReplyAsync("```\r\nRECENT KILLS\r\n```");
            List<Discord.Embed> recentkills = await _kills.GetRecentKillsByPlayerAsync(player);
            foreach (Discord.Embed kill in recentkills)
            {
                await ReplyAsync("", false, kill);
            }


            await ReplyAsync("```\r\nRECENT DEATHS\r\n```");
            List<Discord.Embed> recentdeaths = await _kills.GetRecentDeathsByPlayerAsync(player);
            foreach (Discord.Embed kill in recentdeaths)
            {
                await ReplyAsync("", false, kill);
            }
        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("kills for clan")]
        [Summary("My Kills")]
        public async Task KillCountByClanAsync(string clan)
        {
            int killdaily = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 1);
            int killweekly = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 7);
            int killmonthly = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 30);
            int deathdaily = await _kills.GetDeathCountByClanAsync(clan, KillsType.Clan, 1);
            int deathweekly = await _kills.GetDeathCountByClanAsync(clan, KillsType.Clan, 7);
            int deathmonthly = await _kills.GetDeathCountByClanAsync(clan, KillsType.Clan, 30);
            string response = "";

            switch (clan)
            {
                default:
                    response = string.Format(_config["kills:formats:killsforclan"],
                                                clan,
                                                killdaily,
                                                deathdaily,
                                                killweekly,
                                                deathweekly,
                                                killmonthly,
                                                deathmonthly);
                    break;
            }

            await ReplyAsync(response);


            await ReplyAsync("```\r\nRECENT KILLS\r\n```");
            List<Discord.Embed> recentkills = await _kills.GetRecentKillsByClanAsync(clan);
            foreach (Discord.Embed kill in recentkills)
            {
                await ReplyAsync("", false, kill);
            }


            await ReplyAsync("```\r\nRECENT DEATHS\r\n```");
            List<Discord.Embed> recentdeaths = await _kills.GetRecentDeathsByClanAsync(clan);
            foreach (Discord.Embed kill in recentdeaths)
            {
                await ReplyAsync("", false, kill);
            }
        }
    }
}