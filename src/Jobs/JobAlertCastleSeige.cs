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
    public class JobAlertCastleSeige : IJob
    {
        public JobAlertCastleSeige(){ }
        public async Task Execute(IJobExecutionContext context)
        {
            string builder = string.Format("Castle siege in 2 hours!! Get online!!");
            await UtilService.SendMessage(builder, "clan-announcements");
        }


    }
}
