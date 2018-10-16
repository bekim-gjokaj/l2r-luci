using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Modules
{
    [Name("Kills")]
    [Summary("Kill List")]
    public class KillListModule : ModuleBase<SocketCommandContext>
    {   /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Recent")]
        [Summary("Recent Kills")]
        public async Task Recent()
        {
            IConfiguration _config = ConfigHelper._configuration;
            string RecentKillList = "";

            foreach (KillListItem killItem in KillListService.KillLog)
            {
                if (killItem.P1 != _config["killlist:specialname"] || killItem.P2 != _config["killlist:specialname"])
                {

                    //Choose the KillList format for victory or defeat
                    string RecentKillListFormat = "";
                    if (killItem.Clan1 == _config["killlist:clanname"])
                    {
                        RecentKillListFormat += _config["killlist:victoryformat"] + "\r\n";
                    }
                    else
                    {
                        RecentKillListFormat += _config["killlist:defeatformat"] + "\r\n";
                    }

                    //Create formatted string for return
                    RecentKillList += string.Format(RecentKillListFormat,
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
                    await ReplyAsync(RecentKillList);
                }

            }
            //
        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Group("kills"), Name("Kills")]
        [Summary("Recent Kills")]
        public class Kills : ModuleBase
        {
            [Command("Kills"), Priority(0)]
            public async Task KillsForSepcificPvP(string P1, string P2)
            {
                IConfiguration _config = ConfigHelper._configuration;
                string RecentKillList = "";
                string RecentKillListFormat = "Player {0} has {1} kill(S). Player {2} has {3} kill(s).\r\n";
                int P1Count = 0;
                int P2Count = 0;

                foreach (KillListItem killItem in KillListService.KillLog)
                {

                    if (killItem.P1 == P1 && killItem.P2 == P2)
                    {

                        if (killItem.Clan1 == _config["killlist:clanname"])
                        {
                            P1Count++;
                            P2Count--;
                        }
                        else
                        {
                            P2Count++;
                            P1Count--;
                        }

                        //Create formatted string for return
                        RecentKillList += string.Format(RecentKillListFormat, P1, P1Count, P2, P2Count);

                        //Return formatted string to Discord
                        await ReplyAsync(RecentKillList);
                    }


                }

            }


            /// <summary>
            /// RECENT
            /// </summary>
            /// <returns></returns>
            [Command("for"), Priority(1)]
            [Summary("My Kills")]
            public async Task KillCountByPlayerAsync(string player)
            {
                int result = await KillListService.GetCountAsync(player, KillListType.Personal);
                string response = "";

                switch (player)
                {
                    case "yamcha":
                        response = string.Format("Eat shit and die. {0} has {1} kills.", player, result);
                        break;

                    case "Tyranitar":
                        response = string.Format("The PvE King {0} has {1} kills.", player, result);
                        break;

                    case "PzYcHO":
                        response = string.Format("Hold my beer {0} The Queen Bitch has {1} kills.", player, result);
                        break;

                    case "JuggernuttZ":
                        response = string.Format("The Amazing {0} has {1} kills.", player, result);
                        break;

                    case "EdgyAsHell":
                        response = string.Format("That asshole {0} has {1} kills.", player, result);
                        break;

                    case "GOOD":
                    case "EVIL":
                    case "SCERogue":
                        response = string.Format("That asshole {0} has {1} kills.", player, result);
                        break;

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
            [Command("for clan")]
            [Summary("My Kills")]
            public async Task KillCountByClanAsync(string clan)
            {
                int result = await KillListService.GetCountAsync(clan, KillListType.Clan);
                string response = "";

                switch (clan)
                {
                    case "RogueSquad":
                        response = string.Format("Those pussies {0} have {1} kills... but they go down like a MOTHERFUCKER!", clan, result);
                        break;

                    case "Ascension":
                        response = string.Format("Those bitches {0} have {1} kills... but they got good dick.", clan, result);
                        break;

                    case "SakuraMoonblade":
                    case "TheMainEvent":
                        response = string.Format("Those dicks {0} have {1} kills.", clan, result);
                        break;

                    case "Legacy":
                        response = string.Format("Those crazy fuckers {0} have {1} kills...Oh look! Something shiney!!", clan, result);
                        break;

                    default:
                        response = string.Format("Those asshats {0} have {1} kills.", clan, result);
                        break;
                }

                await ReplyAsync(response);
            }
        }
        //
    }
}
