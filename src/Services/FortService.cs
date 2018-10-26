using Discord;
using Discord.WebSocket;
using Luci.Models;
using Luci.Models.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class FortService
    {
        private IConfiguration _config { get; set; }
        //Fort response dictionaries
        private SortedDictionary<string, string> dictFortRespYes = new SortedDictionary<string, string>();
        private SortedDictionary<string, string> dictFortRespNo = new SortedDictionary<string, string>();
        private SortedDictionary<string, string> dictFortRespMaybe = new SortedDictionary<string, string>();
        //private readonly SortedDictionary<string, SortedDictionary<string, string>> dictResponses = new SortedDictionary<string, SortedDictionary<string, string>>();

        public FortService(IConfiguration config)
        {
            _config = config;
            LoadFileAsync().Wait();
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

            var result = await SaveFileAsync();
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


        public async Task<bool> SaveFileAsync()
        {


            string yesfilename = _config["data:fortyesfile"];
            string nofilename = _config["data:fortnofile"];
            string maybefilename = _config["data:fortmaybefile"];
            string filedir = _config["data:datadir"];
            bool success = false;

            try
            {

                string jsonyes = JsonConvert.SerializeObject(dictFortRespYes);
                string jsonno = JsonConvert.SerializeObject(dictFortRespNo);
                string jsonmaybe = JsonConvert.SerializeObject(dictFortRespMaybe);

                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);
                if (!File.Exists(yesfilename))               // Create bountylist file if it doesn't exist
                    File.Create(yesfilename).Dispose();
                if (!File.Exists(nofilename))               // Create bountylist file if it doesn't exist
                    File.Create(nofilename).Dispose();
                if (!File.Exists(maybefilename))               // Create bountylist file if it doesn't exist
                    File.Create(maybefilename).Dispose();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{yesfilename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, dictFortRespYes);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{nofilename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, dictFortRespNo);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{maybefilename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, dictFortRespMaybe);
                }

                success = true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR SAVING BOUNTY LIST JSON\r\n" + ex.ToString());
            }
            return success;
        }


        public async Task LoadFileAsync()
        {
            string yesfilename = _config["data:fortyesfile"];
            string nofilename = _config["data:fortnofile"];
            string maybefilename = _config["data:fortmaybefile"];
            string filedir = _config["data:datadir"];

            Dictionary<string, Bounty> bountyList = new Dictionary<string, Bounty>();
            try
            {


                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);
                if (!File.Exists(yesfilename))               // Create bountylist file if it doesn't exist
                    File.Create(yesfilename).Dispose();
                if (!File.Exists(nofilename))               // Create bountylist file if it doesn't exist
                    File.Create(nofilename).Dispose();
                if (!File.Exists(maybefilename))               // Create bountylist file if it doesn't exist
                    File.Create(maybefilename).Dispose();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{yesfilename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    dictFortRespYes = serializer.Deserialize<SortedDictionary<string, string>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{nofilename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    dictFortRespNo = serializer.Deserialize<SortedDictionary<string, string>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{maybefilename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    dictFortRespMaybe = serializer.Deserialize<SortedDictionary<string, string>>(reader);
                }

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING BOUNTY LIST JSON\r\n" + ex.ToString());
            }

        }

    }
}
