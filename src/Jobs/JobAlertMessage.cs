using Discord.WebSocket;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luci.Jobs
{

    /// <summary>
    /// SimpleJOb is just a class that implements IJOB interface. It implements just one method, Execute method
    /// </summary>
    public class JobAlertMessage : IJob
    {
        private IConfiguration _config { get; set; }
        private DiscordSocketClient _discord { get; set; }

        public JobAlertMessage()
        {
        }

        public JobAlertMessage(IConfiguration Config, DiscordSocketClient Discord)
        {
            _config = Config;
            _discord = Discord;
        }
        public async Task Execute(IJobExecutionContext context)
        {

            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string jobSays = dataMap.GetString("jobSays");
            ulong guildId = Convert.ToUInt64(_config["scheduler:guildid"]);
            ulong channelId = Convert.ToUInt64(_config["scheduler:channelid"]);
            await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(jobSays);
        }


    }
}
