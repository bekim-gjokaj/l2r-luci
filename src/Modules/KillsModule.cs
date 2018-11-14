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
            string RecentKills = "```RECENT LIST```";

            List<KillsItem> KillLog = await _kills.GetKillLogAsync();
            if (KillLog.Count != 0)
            {

                var result = await _kills.GetRecentKillsAsync();

                //Return formatted string to Discord
                foreach (var item in result)
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
        public async Task KillsForSpecificPvP(string P1, string P2)
        {
            List<Discord.Embed> result = await _kills.KillsForSpecificPvP(P1, P2);
            //Return formatted string to Discord
            foreach (Discord.Embed item in result)
            {
                await ReplyAsync("", false, item);
            }
        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Kills for")]
        [Summary("My Kills")]
        public async Task KillCountByPlayerAsync(string player)
        {
            int dailyresult = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 1);
            int weeklyresult = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 7);
            int monthlyresult = await _kills.GetKillCountByPlayerAsync(player, KillsType.Personal, 30);
            string response = "";

            switch (player)
            {
                default:
                    response = string.Format(_config["kills:formats:killsfor"], player, dailyresult, weeklyresult, monthlyresult);
                    break;
            }

            await ReplyAsync(response);
        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("kills for clan")]
        [Summary("My Kills")]
        public async Task KillCountByClanAsync(string clan)
        {
            int dailyresult = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 1);
            int weeklyresult = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 7);
            int monthlyresult = await _kills.GetKillCountByClanAsync(clan, KillsType.Clan, 30);
            string response = "";

            switch (clan)
            {
                default:
                    response = string.Format(_config["kills:formats:killsforclan"], clan, dailyresult, weeklyresult, monthlyresult);
                    break;
            }

            await ReplyAsync(response);
        }
    }
}