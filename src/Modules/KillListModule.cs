using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Luci.Modules
{
    [Name("KillList")]
    [Summary("Clan Kill List")]
    public class KillListModule : ModuleBase<SocketCommandContext>
    {

        [Command("Recent")]
        [Summary("Recent Clan Kill List")]
        public async Task Say()
        {
            //string RecentKillList = string.Join("\r\n", this.dictRecentKills.ToArray());
            //await ReplyAsync(RecentKillList);
        }

      
    }








 }
