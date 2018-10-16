using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Luci.Modules
{
    [Name("KillList")]
    [Summary("Clan Kill List")]
    public class KillListModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Recent")]
        [Summary("Recent Kills")]
        public async Task Say()
        {
            IConfiguration _config = ConfigHelper._configuration;
            string RecentKillList = "";

            foreach (KillListItem killItem in KillListService.KillLog)
            {
                if (killItem.P1 != "DocHoliday" || killItem.P2 == "DocHoliday")
                {
                    RecentKillList = "";
                    if (killItem.Clan1 == _config["killlist:clanname"])
                    {
                        RecentKillList += string.Format(_config["killlist:victoryformat"] + "\r\n",
                                killItem.P1,
                                (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                killItem.Clan1,
                                (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                killItem.P2,
                                (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                killItem.Clan2,
                                (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                DateTime.Now);

                        await ReplyAsync(RecentKillList);
                    }
                    else
                    {
                        RecentKillList += string.Format(_config["killlist:defeatformat"] + "\r\n",
                                killItem.P1,
                                (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                killItem.Clan1,
                                (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                killItem.P2,
                                (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                killItem.Clan2,
                                (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                DateTime.Now);

                        await ReplyAsync(RecentKillList);
                    }
                }

            }
            //
        }
    }
}