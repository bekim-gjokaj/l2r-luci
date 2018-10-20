using Discord;
using Discord.WebSocket;
using Luci.Models.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class FortService
    {
        private IConfiguration _config { get; set; }
        //Fort response dictionaries
        private readonly SortedDictionary<string, string> dictFortRespYes = new SortedDictionary<string, string>();
        private readonly SortedDictionary<string, string> dictFortRespNo = new SortedDictionary<string, string>();
        private readonly SortedDictionary<string, string> dictFortRespMaybe = new SortedDictionary<string, string>();
        //private readonly SortedDictionary<string, SortedDictionary<string, string>> dictResponses = new SortedDictionary<string, SortedDictionary<string, string>>();

        public FortService(IConfiguration config)
        {
            _config = config;
        }
        
        public async Task<Embed> AttendanceList()
        {
            return await GetEmbedAsync();
        }

        public async Task<Embed> AttendanceAdd(string Player, AttendanceResponseType Response)
        {
            switch(Response)
            {
                case AttendanceResponseType.Yes:
                    dictFortRespYes.Add(Player, "yes");
                    break;

                case AttendanceResponseType.No:
                    dictFortRespYes.Add(Player, "no");
                    break;

                case AttendanceResponseType.Maybe:
                    dictFortRespYes.Add(Player, "maybe");
                    break;
            }
            return await GetEmbedAsync();
        }


        public static async Task<int> DaysRemainingTillFortSiege()
        {
            DateTime today = DateTime.Today;
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            return ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;

        }

        public static async Task<DateTime> DateOfNextFortSiege()
        {

            return DateTime.Now.AddDays(await DaysRemainingTillFortSiege());
        }



        public async Task<Embed> GetEmbedAsync()
        {

            //Setup variables
            string formatFort = _config["fort:formats:embed"];
            int daysRemaining = await DaysRemainingTillFortSiege();

            // Setup embeded card
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(0, 255, 33),
                Description = _config["fort:attendance:desc"],
                Title = $"{_config["fort:attendance:title"]} - ({daysRemaining} days)"
            };


            //LOOP through YES responses in dictionary
            foreach (var key in dictFortRespYes.Keys)
            {

                if (dictFortRespYes.Count != 0)
                {
                    //Build a string of names
                    string embedVal = "";
                    foreach (KeyValuePair<string, string> response in dictFortRespYes)
                    {
                        embedVal += $"{response.Key}\r\n";
                    }
                    //Add fields to embed card for the bounty
                    builder.AddField(x =>
                    {
                        x.Name = _config["fort:attendance:yestitle"];
                        x.Value = embedVal;
                        x.IsInline = false;
                    });
                    
                }

            }

            //LOOP through NO responses in dictionary
            foreach (var key in dictFortRespNo.Keys)
            {

                if (dictFortRespNo.Count != 0)
                {
                    //Build a string of names
                    string embedVal = "";
                    foreach (KeyValuePair<string, string> response in dictFortRespNo)
                    {
                        embedVal += $"{response.Key}\r\n";
                    }
                    //Add fields to embed card for the bounty
                    builder.AddField(x =>
                    {
                        x.Name = _config["fort:attendance:notitle"];
                        x.Value = embedVal;
                        x.IsInline = false;
                    });


                }

            }

            //LOOP through MAYBE responses in dictionary
            foreach (var key in dictFortRespMaybe.Keys)
            {

                if (dictFortRespMaybe.Count != 0)
                {
                    //Build a string of names
                    string embedVal = "";
                    foreach (KeyValuePair<string, string> response in dictFortRespMaybe)
                    {
                        embedVal += $"{response.Key}\r\n";
                    }
                    //Add fields to embed card for the bounty
                    builder.AddField(x =>
                    {
                        x.Name = _config["fort:attendance:yestitle"];
                        x.Value = embedVal;
                        x.IsInline = false;
                    });

                    
                }

            }

            return builder.Build();
        }

    }
}
