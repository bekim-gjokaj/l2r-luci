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
    public class BountyService
    {
        private IConfiguration _config { get; set; }
        //Bounty dictionaries
        private Dictionary<string, Bounty> BountyList = new Dictionary<string, Bounty>();

        public BountyService(IConfiguration Config)
        {
            _config = Config;
            LoadFileAsync().Wait();
        }
        


        public async Task<Dictionary<string, Bounty>> ListAsync()
        {
            try
            {
                return BountyList;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public async Task<Bounty> FindBountyByNameAsync(string Name)
        {
            try
            {
                if(BountyList.ContainsKey(Name))
                {
                    return BountyList[Name];
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<List<Embed>> AddAsync(string Player, string Description, string Reward, DateTime Expiration, string Type)
        {
            try
            {
                DateTime tmpExp = DateTime.Today.AddDays(7);
                if (Expiration != null)
                {
                    tmpExp = Expiration;
                }

                KillsType tmpType;
                if (Type == "Clan")
                {
                    tmpType = KillsType.Clan;
                }
                else
                {
                    tmpType = KillsType.Personal;
                }

                
                Bounty bounty = new Bounty
                {
                    BountyID = System.Guid.NewGuid(),
                    PlayerName = Player,
                    Description = Description,
                    Reward = Reward,
                    Log = new Dictionary<string, int>(),
                    Type = tmpType,
                    Expiration = tmpExp
                };

                BountyList.Add(Player, bounty);

                var saveresult = await SaveFileAsync();
                var result = await GetEmbedAsync();
                return result;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        private async Task<Bounty> IncrementKillAsync(Bounty bounty, string Player)
        {
            try
            {

                // currentCount will be zero if the key id doesn't exist..
                bounty.Log.TryGetValue(Player, out int currentCount);

                bounty.Log[Player] = currentCount + 1;
                return bounty;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }



        public async Task<List<Embed>> GetEmbedAsync()
        {
            //Create the object to return
            List<Embed> embeds = new List<Embed>();
            
            //Load bounty format
            string formatBounty = _config["kills:bounty:formats:embed"];

            // Setup embeded card
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(255, 0, 0),
                Description = _config["kills:bounty:desc"],
                Title = $":gift:   **BOUNTY LIST**   :gift:"
            };

            if ((BountyList != null) && BountyList.Count > 0)
            {
                foreach (KeyValuePair<string, Bounty> item in BountyList)
                {

                    //Add fields to embed card for the bounty
                    builder.AddField(x =>
                    {
                        x.Name = $":gift:   {item.Value.PlayerName}";
                        x.Value = string.Format(formatBounty, item.Value.Type, item.Value.Description, item.Value.Expiration, item.Value.Reward);
                        x.IsInline = false;
                    });

                }
                embeds.Add(builder.Build());
                return embeds;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> GetStringAsync()
        {
            
            //Initialize string
            string strBounties = "";

            if ((BountyList != null) && BountyList.Count > 0)
            {
                foreach (KeyValuePair<string, Bounty> item in BountyList)
                {
                    //Choose the Kills format for victory or defeat
                    string BountyListFormat = _config["kills:bounty:formats:list"];

                    //Create formatted string for return
                    strBounties += string.Format(BountyListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);

                }
                strBounties = $"```BOUNTY LIST\r\n\r\n{strBounties}\r\n\r\n```";
                return strBounties;
            }
            else
            {
                return "```BOUNTY LIST EMPTY```";
            }
        }


        public async Task<bool> SaveFileAsync()
        {

            string filename = _config["data:bountyfile"];
            string filedir = _config["data:datadir"];
            bool success = false;

            try
            {

                string json = JsonConvert.SerializeObject(BountyList);

                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);
                if (!File.Exists(filename))               // Create bountylist file if it doesn't exist
                    File.Create(filename).Dispose();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, BountyList);
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

            string filename = _config["data:bountyfile"];
            string filedir = _config["data:datadir"];
            Dictionary<string, Bounty> bountyList = new Dictionary<string, Bounty>();
            try
            {
                

                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);
                if (!File.Exists(filename))               // Create bountylist file if it doesn't exist
                    File.Create(filename).Dispose();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    BountyList = serializer.Deserialize<Dictionary<string, Bounty>>(reader);
                }
                
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING BOUNTY LIST JSON\r\n" + ex.ToString());
            }
        }

    }
}
