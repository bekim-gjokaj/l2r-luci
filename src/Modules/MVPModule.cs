using Discord.Commands;
using Luci.Models.Enums;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Luci.KillService;

namespace Luci.Modules
{

    [Name("MVP")]
    public class MVPModule : ModuleBase
    {
        private IConfiguration _config { get; set; }
        private KillService _kills { get; set; }

        public MVPModule(IConfiguration Config, KillService Kills)
        {
            _config = Config;
            _kills = Kills;
        }

        [Command("PzYcHO")]
        [Summary("My Kills")]
        public async Task KillCountforPzYcHOAsync()
        {
            string response = $"Hold my beer PzYcHO The Queen Bitch has {await _kills.GetCountAsync("PzYcHO", KillsType.Personal)} kills.";
            await ReplyAsync(response);
        }

        [Command("yamcha")]
        [Summary("My Kills")]
        public async Task KillCountforyamchaAsync()
        {
            string player = "yamcha";
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = string.Format("Eat shit and die. {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("EdgyAsHell")]
        [Summary("My Kills")]
        public async Task KillCountforedgyashellAsync()
        {
            string player = "EdgyAsHell";
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = string.Format("That asshole {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("JuggernuttZ")]
        [Summary("My Kills")]
        public async Task KillCountforJuggernuttZAsync()
        {
            string player = "JuggernuttZ";
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = string.Format("The Amazing {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("Tyranitar")]
        [Summary("My Kills")]
        public async Task KillCountforTyranitarAsync()
        {
            string player = "Tyranitar";
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = string.Format("The PvE King {0} has {1} kills.", player, result);

            await ReplyAsync(response);
        }

        [Command("MrDitch")]
        [Summary("My Kills")]
        public async Task KillCountforMrDitchAsync()
        {
            string player = "MrDitch";
            int result = await _kills.GetCountAsync(player, KillsType.Personal);
            string response = string.Format("The shemale spy MrBitch has {0} kills.", result);

            await ReplyAsync(response);
        }
    }

}