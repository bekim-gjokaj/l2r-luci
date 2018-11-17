using Discord;
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
        private readonly SortedDictionary<string, SortedDictionary<string, string>> dictFortResponses = new SortedDictionary<string, SortedDictionary<string, string>>();

        public FortService(IConfiguration config)
        {
            _config = config;
            LoadFileAsync().Wait();
        }

        public async Task<List<Embed>> GetAsync()
        {
            List<Embed> embeds = new List<Embed>();
            Embed embed = await GetEmbedAsync();
            embeds.Add(embed);
            return embeds;
        }


        public async Task<List<Embed>> AddAttendanceAsync(string Player, AttendanceResponseType Response)
        {

            List<Embed> embeds = new List<Embed>();

            string prevResponse = await CheckIfPlayerResponded(Player);

            //No previous response - add new response
            if (prevResponse == "")
            {
                switch (Response)
                {
                    case AttendanceResponseType.Yes:
                        dictFortRespYes.Add(Player, "yes");
                        break;

                    case AttendanceResponseType.No:
                        dictFortRespNo.Add(Player, "no");
                        break;

                    case AttendanceResponseType.Maybe:
                        dictFortRespMaybe.Add(Player, "maybe");
                        break;
                }
            }
            else if (Response.ToString().ToLower() == prevResponse.ToLower())
            {

                // Setup embeded alert
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Color = new Color(255, 255, 0),
                    Title = $"\r\n *** Same response as last time dummy. Are you drunk right now? ***\r\n"
                };
                Embed alert = builder.Build();

                embeds.Add(alert);
            }
            else //There was a previous response - changing answer
            {
                string outputVal;
                switch (prevResponse)
                {
                    case "yes":
                        dictFortRespYes.Remove(Player);
                        break;

                    case "no":
                        dictFortRespNo.Remove(Player);
                        break;

                    case "maybe":
                        dictFortRespMaybe.Remove(Player);
                        break;
                }

                switch (Response)
                {
                    case AttendanceResponseType.Yes:
                        dictFortRespYes.Add(Player, "yes");
                        break;

                    case AttendanceResponseType.No:
                        dictFortRespNo.Add(Player, "no");
                        break;

                    case AttendanceResponseType.Maybe:
                        dictFortRespMaybe.Add(Player, "maybe");
                        break;
                }

                // Setup embeded alert
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Color = new Color(255, 255, 0),
                    Title = $"\r\n *** CHANGE OF RESPONSE ***\r\n"
                };
                Embed alert = builder.Build();

                embeds.Add(alert);

            }

            bool result = await SaveFileAsync();
            Embed embed = await GetEmbedAsync();

            embeds.Add(embed);

            return embeds;
        }


        public async Task<List<Embed>> ClearAsync()
        {
            dictFortRespMaybe.Clear();
            dictFortRespNo.Clear();
            dictFortRespYes.Clear();

            List<Embed> embeds = new List<Embed>();

            // Setup embeded alert
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(255, 0, 0),
                Title = $"\r\n *** CLEARED LIST ***\r\n"
            };
            Embed alert = builder.Build();

            embeds.Add(alert);


            Embed embed = await GetEmbedAsync();
            embeds.Add(embed);


            bool result = await SaveFileAsync();
            return embeds;
        }


        public async Task<string> CheckIfPlayerResponded(string Player)
        {
            string response = "";

            if (dictFortRespYes.ContainsKey(Player))
            {
                response = dictFortRespYes[Player];
            }
            else if (dictFortRespNo.ContainsKey(Player))
            {
                response = dictFortRespNo[Player];
            }
            else if (dictFortRespMaybe.ContainsKey(Player))
            {
                response = dictFortRespMaybe[Player];
            }

            return response;

        }


        private static async Task<int> DaysRemainingTillFortSiege()
        {
            DateTime today = DateTime.Today;
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            return ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;

        }

        private static async Task<DateTime> DateOfNextFortSiege()
        {

            return DateTime.Now.AddDays(await DaysRemainingTillFortSiege());
        }



        public async Task<Embed> GetEmbedAsync()
        {
            try
            {

                //Setup variables
                string formatFort = _config["fort:formats:embed"];
                int daysRemaining = await DaysRemainingTillFortSiege();
                int responseCounter = 0;

                // Setup embeded card
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Color = new Color(0, 255, 33),
                    Description = _config["fort:attendance:desc"],
                    Title = $"{_config["fort:attendance:title"]} - ({daysRemaining} days)"
                };


                //LOOP through YES responses in dictionary
                foreach (string key in dictFortRespYes.Keys)
                {
                    if (dictFortRespYes.Count != 0)
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in dictFortRespYes)
                        {
                            responseCounter++;
                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = _config["fort:attendance:yestitle"];
                            x.Value = $"\r\n{embedVal}\r\n";
                            x.IsInline = false;
                        });

                        break;

                    }

                }

                //LOOP through NO responses in dictionary
                foreach (string key in dictFortRespNo.Keys)
                {
                    if (dictFortRespNo.Count != 0)
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in dictFortRespNo)
                        {
                            responseCounter++;

                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = _config["fort:attendance:notitle"];
                            x.Value = embedVal;
                            x.IsInline = false;
                        });

                        break;
                    }

                }

                //LOOP through MAYBE responses in dictionary
                foreach (string key in dictFortRespMaybe.Keys)
                {

                    if (dictFortRespMaybe.Count != 0)
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in dictFortRespMaybe)
                        {
                            responseCounter++;
                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = _config["fort:attendance:maybetitle"];
                            x.Value = embedVal;
                            x.IsInline = false;
                        });

                        break;
                    }

                }


                //Add fields to embed card for the bounty
                builder.AddField(x =>
                {
                    x.Name = string.Format(_config["fort:attendance:totaltitle"]);
                    x.Value = string.Format(_config["fort:attendance:count"], responseCounter);
                    x.IsInline = false;
                });
                return builder.Build();
            }
            catch (Exception ex)
            {

                await Console.Out.WriteLineAsync(ex.ToString());
                return null;
            }
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
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(yesfilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(yesfilename).Dispose();
                }

                if (!File.Exists(nofilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(nofilename).Dispose();
                }

                if (!File.Exists(maybefilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(maybefilename).Dispose();
                }

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
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(yesfilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(yesfilename).Dispose();
                }

                if (!File.Exists(nofilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(nofilename).Dispose();
                }

                if (!File.Exists(maybefilename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(maybefilename).Dispose();
                }

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

                await Console.Out.WriteLineAsync($"*** LOADED FORTFILE from {filedir}\\{yesfilename}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING BOUNTY LIST JSON\r\n" + ex.ToString());
            }

        }

    }
}
