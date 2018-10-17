using Discord.Commands;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Modules
{
    [Name("Fort"), Group("Fort")]
    [Summary("Fort Actions")]
    public class FortModule : ModuleBase<SocketCommandContext>
    {

        public SortedDictionary<string, string> dictFortRespYes = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> dictFortRespNo = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> dictFortRespMaybe = new SortedDictionary<string, string>();


        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Ask Attendance")]
        [Summary("Prompt Luci to start asking who's coming")]
        public async Task Attendance()
        {
            IConfiguration _config = ConfigService._configuration;
            await UtilService.SendMessage(_config["fort:attendancemsg"], _config["fort:attendancechannel"]);
        }

        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Attendance")]
        [Summary("Prompt Luci to start asking who's coming")]
        public async Task<string> AttendanceList()
        {
            IConfiguration _config = ConfigService._configuration;
            DateTime today = DateTime.Today;
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            DateTime nextFriday = today.AddDays(daysUntilFriday);


            string output = string.Format("```Attendance for Fort Siege ({0})\r\n***YES:***", nextFriday.ToShortDateString());
            foreach (var item in dictFortRespYes)
            {
                output += String.Format("* {0}\r\n", item.Key);
            }

            output += "\r\n***NO:***";

            foreach (var item in dictFortRespNo)
            {
                output += String.Format("* {0}\r\n", item.Key);
            }
            output += "\r\n***MAYBE:***";

            foreach (var item in dictFortRespMaybe)
            {
                output += String.Format("* {0}\r\n", item.Key);
            }
            output += "\r\n```";


            return output;
        }



        /// <summary>
        /// RECENT
        /// </summary>
        /// <returns></returns>
        [Command("Answer")]
        [Summary("Respond to fort attendance")]
        public async Task AttendanceResponse(string Response)
        {
            IConfiguration _config = ConfigService._configuration;


            switch (Response.ToLower())
            {
                case "yes":
                    await UtilService.SendMessage(
                        String.Format(_config["fort:attendanceresponsemsg"], "yes"), _config["fort:attendancechannel"]);
                    break;

                case "no":
                    await UtilService.SendMessage(
                        String.Format(_config["fort:attendanceresponsemsg"], "no"), _config["fort:attendancechannel"]);
                    break;
                case "maybe":
                    await UtilService.SendMessage(
                        String.Format(_config["fort:attendanceresponsemsg"], "maybe"), _config["fort:attendancechannel"]);
                    break;
                default:
                    await UtilService.SendMessage("Sorry! I didn't catch your response properly. Your answer will not be recorded. Please try again later.", _config["fort:attendancechannel"]);
                    break;
            }
            //await UtilService.SendMessage(_config["fort:attendancemsg"], _config["fort:attendancechannel"]);
        }

    }

}
