using Discord.Commands;
using Luci.Models;
using Luci.Models.Enums;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
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

                foreach (KillsItem killItem in KillLog)
                {
                    if (killItem.P1 != _config["kills:specialname"] || killItem.P2 != _config["kills:specialname"])
                    {

                        //Choose the Kills format for victory or defeat
                        string RecentKillsFormat = "";
                        if (killItem.Clan1 == _config["kills:clanname"])
                        {
                            RecentKillsFormat += _config["kills:victoryformat"] + "\r\n";
                        }
                        else
                        {
                            RecentKillsFormat += _config["kills:defeatformat"] + "\r\n";
                        }

                        //Create formatted string for return
                        RecentKills += string.Format(RecentKillsFormat,
                                    killItem.P1,
                                    (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                    killItem.Clan1,
                                    (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                    killItem.P2,
                                    (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                    killItem.Clan2,
                                    (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                    DateTime.Now);

                        //Return formatted string to Discord
                        await ReplyAsync(RecentKills);
                    }

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

            var result = await _kills.KillsForSpecificPvP(P1, P2);
            //Return formatted string to Discord
            foreach (var item in result)
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
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = "";

            switch (player)
            {
                default:
                    response = string.Format("That asshole {0} has {1} kills.", player, result);
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

            int result = await _kills.GetCountAsync(clan, KillsType.Clan);
            string response = "";

            switch (clan)
            {
                default:
                    response = string.Format("Those assholes {0} have {1} kills.", clan, result);
                    break;
            }

            await ReplyAsync(response);
        }

    }
}
