using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Modules
{
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
                IConfiguration _config = ConfigService._configuration;
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

            IConfiguration _config = ConfigService._configuration;
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
}