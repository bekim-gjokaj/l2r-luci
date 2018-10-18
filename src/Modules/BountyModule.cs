using Discord;
using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Modules
{
    [Name("Bounty"), Group("Bounty")]
    [Summary("Bounty management")]
    public class BountyModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public BountyModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }


        [Command("List")]
        [Summary("Bounties")]
        /// <summary>
        /// GET BOUNTIES
        /// </summary>
        /// <returns></returns>
        public async Task List()
        {
            try
            {
                var result = await BountyEmbedFormatterAsync();
                await ReplyAsync("", false, result);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Command("Add")]
        [Summary("Add Bounties")]
        public async Task Add(string Player, string Description, string Options)
        {
            try
            {
                string[] arrTemp = Options.Split("|");
                string Reward = arrTemp[0];
                string Type = arrTemp[1];
                int Days = Convert.ToInt32(arrTemp[2]);
                DateTime Exp = DateTime.Now.AddDays(Convert.ToInt32(Days));


                Dictionary<string, Bounty> bountylist = await KillListService.AddBountyAsync(Player, Description, Reward, Exp, Type);
                if ((bountylist != null) && bountylist.Count > 0)
                {
                    var result = await BountyEmbedFormatterAsync();
                    //Return formatted string to Discord
                    await ReplyAsync("", false, result);
                    //await ReplyAsync("", false, builder.Build());
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

        public async Task<string> BountyStringFormatterAsync()
        {
            Dictionary<string, Bounty> bountylist = await KillListService.GetBountyListAsync();


            string strBounties = "BOUNTY LIST\r\n";

            // Setup embeded card
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"**BOUNTY LIST**"
            };

            if ((bountylist != null) && bountylist.Count > 0)
            {
                foreach (KeyValuePair<string, Bounty> item in bountylist)
                {
                    //Choose the KillList format for victory or defeat
                    string BountyListFormat = _config["killlist:bountylistformat"] + "\r\n";

                    //Create formatted string for return
                    strBounties += string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);
                    string strTmp = string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);
                    builder.AddField(x =>
                    {
                        x.Name = item.Value.PlayerName;
                        x.Value = strTmp;
                        x.IsInline = false;
                    });
                }

                return strBounties;
            }
            else
            {
                return "";
            }
        }
        public async Task<Embed> BountyEmbedFormatterAsync()
        {
            Dictionary<string, Bounty> bountylist = await KillListService.GetBountyListAsync();

            
            // Setup embeded card
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(255, 0, 0),
                Description = $"**BOUNTY LIST**\r\n \r\n"
            };

            if ((bountylist != null) && bountylist.Count > 0)
            {
                foreach (KeyValuePair<string, Bounty> item in bountylist)
                {
                    //Choose the KillList format for victory or defeat
                    string BountyListFormat = _config["killlist:bountylistformat"] + "\r\n";

                    //Create formatted string for return
                    string strTmp = string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);
                    builder.AddField(x =>
                    {
                        x.Name = $":gift:   **{item.Value.PlayerName}** ";
                        x.Value = strTmp;
                        x.IsInline = false;
                    });
                }

                return builder.Build();
            }
            else
            {
                return null;
            }
        }
    }
}