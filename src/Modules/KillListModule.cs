using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Modules
{
    [Name("Log")]
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
            string RecentKillList = "```RECENT LIST```";

            List<KillListItem> KillLog = await KillListService.GetKillLogAsync();
            if (KillLog.Count != 0)
            {

                foreach (KillListItem killItem in KillLog)
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
            }
            else
            {
                await ReplyAsync("Kill List Currently Empty.");
            }
        }
    }


    [Group("Bounty"), Name("Bounty")]
    [Summary("Bounty management")]
    public class BountyModule : ModuleBase<SocketCommandContext>
    {   /// <summary>
        /// GET BOUNTIES
        /// </summary>
        /// <returns></returns>
        [Command("List")]
        [Summary("Bounties")]
        public async Task GetBounties()
        {
            try
            {
                IConfiguration _config = ConfigHelper._configuration;
                string RecentKillList = "```BOUNTY LIST```\r\n";

                Dictionary<string, Bounty> bountylist = await KillListService.GetBountyListAsync();
                if (bountylist.Count != 0)
                {

                    foreach (KeyValuePair<string, Bounty> item in bountylist)
                    {

                        //Choose the KillList format for victory or defeat
                        string BountyListFormat = _config["killlist:bountylistformat"];

                        //Create formatted string for return
                        RecentKillList += string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration, item.Value.Type);
                        RecentKillList += "```\r\n" + item.Value.PlayerName + "'s Bounty Log:\r\n";

                        foreach (KeyValuePair<string, int> log in item.Value.Log)
                        {
                            RecentKillList += "* " + log.Key + " - " + log.Value + "\r\n";
                        }
                        RecentKillList += "\r\n```";

                    }
                    //Return formatted string to Discord
                    await ReplyAsync(RecentKillList);
                }
                else
                {
                    await ReplyAsync("Bounty List Currently Empty.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        [Command("Add")]
        [Summary("Add Bounties")]
        public async Task AddBounty(string Player, string Description, string Options)
        {
            string[] arrTemp = Options.Split("|");
            string Reward = arrTemp[0];
            string Type = arrTemp[1];
            int Days = Convert.ToInt32(arrTemp[2]);


            DateTime Exp = DateTime.Now.AddDays(Convert.ToInt32(Days));


            IConfiguration _config = ConfigHelper._configuration;
            string RecentKillList = "```BOUNTY LIST```";

            Dictionary<string, Bounty> bountylist = await KillListService.AddBountyAsync(Player, Description, Reward, Exp, Type);
            if ((bountylist != null) && bountylist.Count > 0)
            {

                foreach (KeyValuePair<string, Bounty> item in bountylist)
                {

                    //Choose the KillList format for victory or defeat
                    string BountyListFormat = _config["killlist:bountylistformat"] + "\r\n";

                    //Create formatted string for return
                    RecentKillList += string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);


                }


                //Return formatted string to Discord
                await ReplyAsync(RecentKillList);
            }
            else
            {
                await ReplyAsync("Bounty List Currently Empty.");
            }
        }
    }

    [Name("MVP")]
    public class KillListMVPModule : ModuleBase
    {

        [Command("PzYcHO")]
        [Summary("My Kills")]
        public async Task KillCountforPzYcHOAsync()
        {
            string player = "PzYcHO";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("Hold my beer {0} The Queen Bitch has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("yamcha")]
        [Summary("My Kills")]
        public async Task KillCountforyamchaAsync()
        {
            string player = "yamcha";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("Eat shit and die. {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("EdgyAsHell")]
        [Summary("My Kills")]
        public async Task KillCountforedgyashellAsync()
        {
            string player = "EdgyAsHell";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("That asshole {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("JuggernuttZ")]
        [Summary("My Kills")]
        public async Task KillCountforJuggernuttZAsync()
        {
            string player = "JuggernuttZ";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("The Amazing {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("Tyranitar")]
        [Summary("My Kills")]
        public async Task KillCountforTyranitarAsync()
        {
            string player = "Tyranitar";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("The PvE King {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("MrDitch")]
        [Summary("My Kills")]
        public async Task KillCountforMrDitchAsync()
        {
            string player = "MrDitch";
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
            string response = string.Format("The shemale spy MrBitch has {0} kills.", result);

            await ReplyAsync(response);
        }
    }

    /// <summary>
    /// RECENT
    /// </summary>
    /// <returns></returns>
    [Group("kills"), Name("Kills")]
    [Summary("vs")]
    public class Kills : ModuleBase
    {
        [Command("vs")]
        public async Task KillsForSpecificPvP(string P1, string P2)
        {
            IConfiguration _config = ConfigHelper._configuration;
            string RecentKillList = "";
            string RecentKillListFormat = "Player {0} has {1} kill(s) Vs. Player {2} has {3} kill(s).\r\n";
            int P1Count = 0;
            int P2Count = 0;

            List<KillListItem> KillLog = await KillListService.GetKillLogAsync();

            foreach (KillListItem killItem in KillLog)
            {

                if (killItem.P1.ToLower() == P1.ToLower() && killItem.P2.ToLower() == P2.ToLower())
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


                }


            }
            //Create formatted string for return
            RecentKillList += string.Format(RecentKillListFormat, P1, P1Count, P2, P2Count);

            //Return formatted string to Discord
            await ReplyAsync(RecentKillList);

        }


        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("for")]
        [Summary("My Kills")]
        public async Task KillCountByPlayerAsync(string player)
        {
            int result = await KillListService.GetCountAsync(player, KillListType.Personal);
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
        [Command("for clan")]
        [Summary("My Kills")]
        public async Task KillCountByClanAsync(string clan)
        {
            IConfiguration _config = ConfigHelper._configuration;
            int result = await KillListService.GetCountAsync(clan, KillListType.Clan);
            string response = "";
            bool UseRandom = true;

            string configmsg = _config["killlist:clanmessages:" + clan];
            if (configmsg != null)
            {
                response = string.Format(_config["killlist:clanmessages:" + clan], clan, result);
            }

            //if (UseConfigMessages)
            //{
            //    response = string.Format(_config["killlist:clanmessages:" + clan], clan, result);
            //}
            //else
            //{ 
            //    switch (clan)
            //    {
            //        case "RogueSquad":
            //            response = string.Format("Those pussies {0} have {1} kills... but they go down like a MOTHERFUCKER!", clan, result);
            //            break;

            //        case "Ascension":
            //            response = string.Format("Those bitches {0} have {1} kills... but they got good dick.", clan, result);
            //            break;

            //        case "Legacy":
            //            response = string.Format("Those crazy fuckers {0} have {1} kills...Oh look! Something shiney!!", clan, result);
            //            break;
            //    } 
            //}

            if (UseRandom && response == "")
            {
                Random Rnd = new Random();
                int selection = Rnd.Next(4);

                switch (selection)
                {
                    case 0:
                        response = string.Format("Those dipshits {0} have {1} kills.", clan, result);
                        break;
                    case 1:
                        response = string.Format("Those losers {0} have {1} kills.", clan, result);
                        break;
                    case 2:
                        response = string.Format("Those dicks {0} have {1} kills.", clan, result);
                        break;
                    case 3:
                        response = string.Format("Those crybabies {0} have {1} kills.", clan, result);
                        break;
                    case 4:
                        response = string.Format("Those asshats {0} have {1} kills.", clan, result);
                        break;
                }
            }


            await ReplyAsync(response);
        }
    }

}
