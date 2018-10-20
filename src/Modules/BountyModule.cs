using Discord;
using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillService;

namespace Luci.Modules
{
    [Name("Bounty"), Group("Bounty")]
    [Summary("Bounty management")]
    public class BountyModule : ModuleBase<SocketCommandContext>
    {
        private BountyService _bounty;
        private IConfiguration _config;

        public BountyModule(BountyService bounty, IConfiguration config)
        {
            _bounty = bounty;
            _config = config;
        }

        [Command("List")]
        [Summary("Get a list of current bounties")]
        public async Task List()
        {
            try
            {
                List<Embed> result = await _bounty.GetEmbedAsync();

                
                foreach (Embed item in result)
                {
                    await ReplyAsync("", false, item);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Command("Add")]
        [Summary("Add Bounties")]
        public async Task Add( string _player, string _desc, string _reward, string _type, int _days )
        {
            try
            {
                //Parse Params[]
                //string []Params = new string[4];
                //string Player = Params[0];
                //string Description = Params[1];
                //string Reward = Params[2];
                //string Type = Params[3];
                //int Days = Convert.ToInt32(Params[4]);

                DateTime Expiration = DateTime.Now.AddDays(Convert.ToInt32(_days));

                //Add new bounty and return new bounty list
                List<Embed> bountylist = await _bounty.AddBountyAsync(_player, _desc, _reward, Expiration, _type);


                foreach (var embed in bountylist)
                {
                    //Return formatted output to Discord
                    await ReplyAsync("", false, embed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}