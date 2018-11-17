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
    public class SurveyService
    {
        private IConfiguration _config { get; set; }
        //Fort response dictionaries
        private List<Survey> _surveys = new List<Survey>();
        private SortedDictionary<string, string> dictResponses = new SortedDictionary<string, string>();

        public SurveyService(IConfiguration config)
        {
            _config = config;
            LoadAsync().Wait();
        }

        public async Task LoadAsync()
        {
            await LoadFileAsync();

            IConfigurationSection configSurveys = _config.GetSection("surveys");
            

            foreach (var configSurvey in configSurveys.GetChildren())
            {
                Survey survey = new Survey();
                survey.ChannelId = Convert.ToInt64(_config[$"{configSurvey.Path}:channelId"]);
                survey.Color1 = Convert.ToInt32(_config[$"{configSurvey.Path}:color1"]);
                survey.Color2 = Convert.ToInt32(_config[$"{configSurvey.Path}:color2"]);
                survey.Color3 = Convert.ToInt32(_config[$"{configSurvey.Path}:color3"]);
                survey.Count = Convert.ToString(_config[$"{configSurvey.Path}:count"]);
                survey.Desc = Convert.ToString(_config[$"{configSurvey.Path}:desc"]);
                survey.Enabled = Convert.ToBoolean(_config[$"{configSurvey.Path}:enabled"]);
                survey.GuildId = Convert.ToInt64(_config[$"{configSurvey.Path}:guildId"]);
                survey.Msg = Convert.ToString(_config[$"{configSurvey.Path}:msg"]);
                survey.Name = configSurvey.Key;
                survey.ResponseMsg = Convert.ToString(_config[$"{configSurvey.Path}:responsemsg"]);
                survey.Title = Convert.ToString(_config[$"{configSurvey.Path}:title"]);
                survey.TitleYes = Convert.ToString(_config[$"{configSurvey.Path}:yestitle"]);
                survey.TitleNo = Convert.ToString(_config[$"{configSurvey.Path}:notitle"]);
                survey.TitleMaybe = Convert.ToString(_config[$"{configSurvey.Path}:maybetitle"]);
                survey.TitleTotal = Convert.ToString(_config[$"{configSurvey.Path}:totaltitle"]);

                if(!_surveys.Contains(survey))
                {
                    _surveys.Add(survey);
                }
            }
        }

        public async Task<List<Embed>> GetAsync(string Survey)
        {
            List<Embed> embeds = new List<Embed>();
            Embed embed = await GetEmbedAsync(Survey);
            embeds.Add(embed);
            return embeds;
        }


        public async Task<List<Embed>> AddResponseAsync(string Survey, string Player, SurveyResponseType Response)
        {

            List<Embed> embeds = new List<Embed>();
            Survey survey = null;

            //find the survey
            foreach ( var _survey in _surveys)
            {
                if(Survey.ToLower() == _survey.Name)
                {
                    survey = _survey;
                }
            }

            string prevResponse = await CheckIfPlayerResponded(Player, survey.Responses);

            //No previous response - add new response
            if (prevResponse == "")
            {
                switch (Response)
                {
                    case SurveyResponseType.Yes:
                        survey.Responses.Add(Player, "yes");
                        break;

                    case SurveyResponseType.No:
                        survey.Responses.Add(Player, "no");
                        break;

                    case SurveyResponseType.Maybe:
                        survey.Responses.Add(Player, "maybe");
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
                        survey.Responses.Remove(Player);
                        break;

                    case "no":
                        survey.Responses.Remove(Player);
                        break;

                    case "maybe":
                        survey.Responses.Remove(Player);
                        break;
                }

                switch (Response)
                {
                    case SurveyResponseType.Yes:
                        survey.Responses.Add(Player, "yes");
                        break;

                    case SurveyResponseType.No:
                        survey.Responses.Add(Player, "no");
                        break;

                    case SurveyResponseType.Maybe:
                        survey.Responses.Add(Player, "maybe");
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
            Embed embed = await GetEmbedAsync(survey.Name);

            embeds.Add(embed);

            return embeds;
        }


        public async Task<List<Embed>> ClearAsync(string Survey)
        {
            Survey survey = new Models.Survey();
            //find the survey
            foreach (var _survey in _surveys)
            {
                if (Survey.ToLower() == _survey.Name)
                {
                    _survey.Responses.Clear();
                    survey = survey;
                }
            }


            

            List<Embed> embeds = new List<Embed>();

            // Setup embeded alert
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(255, 0, 0),
                Title = $"\r\n *** CLEARED LIST ***\r\n"
            };
            Embed alert = builder.Build();

            embeds.Add(alert);


            Embed embed = await GetEmbedAsync(survey.Name);
            embeds.Add(embed);


            bool result = await SaveFileAsync();
            return embeds;
        }


        public async Task<string> CheckIfPlayerResponded(string Player, SortedDictionary<string, string> Responses)
        {
            string response = "";

            if (Responses.ContainsKey(Player))
            {
                response = Responses[Player];
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



        public async Task<Embed> GetEmbedAsync(string Survey)
        {
            try
            {
                Survey survey = new Models.Survey();
                //find the survey
                foreach (var _survey in _surveys)
                {
                    if (Survey.ToLower() == _survey.Name)
                    {
                        survey = _survey;
                    }
                }

                //Setup variables
                int daysRemaining = await DaysRemainingTillFortSiege();
                int responseCounter = 0;

                // Setup embeded card
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Color = new Color(survey.Color1, survey.Color2, survey.Color3),
                    Description = survey.Desc,
                    Title = $"{survey.Title} - ({daysRemaining} days)"
                };


                //LOOP through YES responses in dictionary
                foreach (string key in survey.Responses.Keys)
                {
                    if (survey.Responses.Count != 0 && survey.Responses[key] == "yes")
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in survey.Responses)
                        {
                            responseCounter++;
                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = survey.TitleYes;
                            x.Value = $"\r\n{embedVal}\r\n";
                            x.IsInline = false;
                        });

                        break;

                    }

                }


                //LOOP through YES responses in dictionary
                foreach (string key in survey.Responses.Keys)
                {
                    if (survey.Responses.Count != 0 && survey.Responses[key] == "no")
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in survey.Responses)
                        {
                            responseCounter++;
                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = survey.TitleNo;
                            x.Value = $"\r\n{embedVal}\r\n";
                            x.IsInline = false;
                        });

                        break;

                    }

                }


                //LOOP through YES responses in dictionary
                foreach (string key in survey.Responses.Keys)
                {
                    if (survey.Responses.Count != 0 && survey.Responses[key] == "Maybe")
                    {
                        //Build a string of names
                        string embedVal = "";
                        foreach (KeyValuePair<string, string> response in survey.Responses)
                        {
                            responseCounter++;
                            embedVal += $"{response.Key}\r\n";
                        }
                        //Add fields to embed card for the bounty
                        builder.AddField(x =>
                        {
                            x.Name = survey.TitleMaybe;
                            x.Value = $"\r\n{embedVal}\r\n";
                            x.IsInline = false;
                        });

                        break;

                    }

                }



                //Add fields to embed card for the bounty
                builder.AddField(x =>
                {
                    x.Name = survey.TitleTotal;
                    x.Value = string.Format(survey.Count, responseCounter);
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
            
            string filename = _config["data:surveyfile"];
            string filedir = _config["data:datadir"];
            bool success = false;

            try
            {

                string jsonSurvey = JsonConvert.SerializeObject(_surveys);
                
                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(filename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(filename).Dispose();
                }
                

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, _surveys);
                }

                success = true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR SAVING SURVEY LIST JSON\r\n" + ex.ToString());
            }
            return success;
        }


        public async Task LoadFileAsync()
        {
            string filename = _config["data:surveyfile"];
            string filedir = _config["data:datadir"];

            Dictionary<string, Bounty> bountyList = new Dictionary<string, Bounty>();
            try
            {


                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(filename))               // Create bountylist file if it doesn't exist
                {
                    File.Create(filename).Dispose();
                }
                

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    _surveys = serializer.Deserialize<List<Survey>>(reader);
                }

                await Console.Out.WriteLineAsync($"*** LOADED FORTFILE from {filedir}\\{filename}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING BOUNTY LIST JSON\r\n" + ex.ToString());
            }

        }

    }
}
