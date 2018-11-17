using Discord;
using Discord.Commands;
using Kamael.Packets;
using Kamael.Packets.Clan;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillService;

namespace Luci.Modules
{
    [Name("Player"), Group("Player")]
    [Summary("Player management")]
    public class PlayerModule : ModuleBase<SocketCommandContext>
    {
        private PlayerService _Player;
        private IConfiguration _config;

        public PlayerModule(PlayerService Player, IConfiguration config)
        {
            _Player = Player;
            _config = config;
        }

        [Command("List")]
        [Summary("Get a list of current players")]
        public async Task List()
        {
            try
            {
                List<Embed> result = await _Player.GetEmbedAsync();


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

        [Command("Test")]
        [Summary("Get a list of current players")]
        public async Task Test()
        {
            try
            {
                
                PacketClanMemberListReadResult members = new PacketClanMemberListReadResult();
                members.ClanID = 1234;
                members.MemberCount = 2;

                PacketClanMemberItem juggz = new PacketClanMemberItem();
                juggz.PlayerName = "JuggernuttZ";
                juggz.PlayerCP = 2000000;
                juggz.Offline = "11/14/2018 00:01:02";

                PacketClanMemberItem edgy = new PacketClanMemberItem();
                edgy.PlayerName = "EdgyAsHell";
                edgy.PlayerCP = 3000000;
                edgy.Offline = "11/14/2018 00:01:03";

                List<PacketClanMemberItem> membs = new List<PacketClanMemberItem>();
                membs.Add(juggz);
                membs.Add(edgy);

                members.Members = membs;

                await _Player.NotifyClanMembersAsync(members);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Command("Clear")]
        [Summary("Clear the list of current bounties")]
        public async Task Clear()
        {
            try
            {
                List<Embed> result = await _Player.ClearAsync();


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
        public async Task Add( string Player, string options)
        {
            try
            {
                //Parse Params[]
                string[] Params = options.Split("|");
                //string Player = Params[0];
                string Description = Params[0];
                string Reward = Params[1];
                string Type = Params[2];
                int Days = Convert.ToInt32(Params[3]);

                DateTime Expiration = DateTime.Now.AddDays(Convert.ToInt32(Days));

                //Add new Player and return new Player list
                List<Embed> Playerlist = await _Player.AddAsync(Player, Description, Reward, Expiration, Type);


                foreach (var embed in Playerlist)
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