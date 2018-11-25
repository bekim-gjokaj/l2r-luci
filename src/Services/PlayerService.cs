using Discord;
using Discord.WebSocket;
using Kamael.Packets.Clan;
using Luci.Models;
using Luci.Models.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class PlayerService
    {
        private IConfiguration _config { get; set; }

        private DiscordSocketClient _discord { get; set; }
        //Player dictionaries
        private Dictionary<string, Player> PlayerList = new Dictionary<string, Player>();

        public PlayerService(IConfiguration Config, DiscordSocketClient Discord)
        {
            _config = Config;
            _discord = Discord;
            LoadFileAsync().Wait();
        }

        public async Task NotifyClanMembersAsync(PacketClanMemberListReadResult clanmembers)
        {
            try
            {

                ulong guildId = 0;
                ulong channelId = 0;
                ulong.TryParse(_config["fort:attendance:guildId"], out guildId);
                ulong.TryParse(_config["kills:channelId"], out channelId);
                uint totalCP = 0;

                string msg = $"Clan ***{clanmembers.ClanID}*** - {clanmembers.MemberCount} Members\r\n\r\n";
                foreach (var item in clanmembers.Members)
                {
                    totalCP += item.PlayerCP;
                    msg += $"***{item.PlayerName}*** - CP: {item.PlayerCP} Offline Time: {item.Offline}\r\n";
                }
                msg += $"\r\nTotal Clan CP: {totalCP}";
                await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(msg);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public async Task NotifyClanInfoAsync(PacketClanInfoReadResult claninfo)
        {
            try
            {

                ulong guildId = 0;
                ulong channelId = 0;
                ulong.TryParse(_config["kills:guildid"], out guildId);
                ulong.TryParse(_config["testchannel"], out channelId);
                uint totalCP = 0;

                string msg = $"Clan ***{claninfo.Name}*** - {claninfo.Members} Members\r\n\r\n";
                
                    msg += $"***{claninfo.Adena}/{claninfo.Adena2}*** Adena- CP: {claninfo.CombatPower}/{claninfo.CombatPower2}\r\n";
                
                msg += $"\r\nTotal Clan CP: {totalCP}";
                await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(msg);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task<Dictionary<string, Player>> ListAsync()
        {
            try
            {
                return PlayerList;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<Player> FindPlayerByNameAsync(string Name)
        {
            try
            {
                if (PlayerList.ContainsKey(Name))
                {
                    return PlayerList[Name];
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

        public async Task<List<Embed>> ClearAsync()
        {
            try
            {
                

                PlayerList.Clear();

                bool saveresult = await SaveFileAsync();
                List<Embed> result = await GetEmbedAsync();
                return result;
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

                Player player = new Player
                {
                    BountyID = System.Guid.NewGuid(),
                    PlayerName = Player,
                    Description = Description,
                    Reward = Reward,
                    Log = new Dictionary<string, int>(),
                    Type = tmpType,
                    Expiration = tmpExp
                };

                PlayerList.Add(Player, player);

                bool saveresult = await SaveFileAsync();
                List<Embed> result = await GetEmbedAsync();
                return result;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<Player> IncrementKillAsync(Player Player, string PlayerName)
        {
            try
            {
                // currentCount will be zero if the key id doesn't exist..
                Player.Log.TryGetValue(PlayerName, out int currentCount);

                Player.Log[PlayerName] = currentCount + 1;
                await SaveFileAsync();
                return Player;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<List<Embed>> GetEmbedAsync()
        {
            try
            {
                //Create the object to return
                List<Embed> embeds = new List<Embed>();

                //Load Player format
                string formatPlayer = _config["kills:Player:formats:embed"];

                // Setup embeded card
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Color = new Color(255, 0, 0),
                    Description = _config["kills:Player:desc"],
                    Title = $":gift:   **Player LIST**   :gift:"
                };

                if ((PlayerList != null) && PlayerList.Count > 0)
                {
                    foreach (KeyValuePair<string, Player> item in PlayerList)
                    {
                        string Playerleaders = "\r\n";
                        string Playerplace = "";
                        int counter = 0;
                        // Reverse for loop (forr + tab)
                        Dictionary<string, int> Playerlog = item.Value.Log;
                        IEnumerable<KeyValuePair<string, int>> leaders = Playerlog.OrderByDescending(x => x.Value).Take(3);
                        foreach (KeyValuePair<string, int> leader in leaders)
                        {
                            counter++;
                            switch(counter)
                            {
                                case 1:
                                    Playerplace = ":first_place:";
                                    break;
                                case 2:
                                    Playerplace = ":second_place:";
                                    break;
                                case 3:
                                    Playerplace = ":third_place:";
                                    break;
                            }
                            Playerleaders += string.Format("{0} {1}:   {2}\r\n",Playerplace, leader.Key, leader.Value);
                        }


                        Playerleaders += "";
                        //Add fields to embed card for the Player
                        builder.AddField(x =>
                        {
                            x.Name = $":gift:   {item.Value.PlayerName}";
                            x.Value = string.Format(formatPlayer, item.Value.Type, item.Value.Description, item.Value.Expiration, item.Value.Reward, Playerleaders);
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
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                return null;
            }
        }

        public async Task<string> GetStringAsync()
        {
            //Initialize string
            string strBounties = "";

            if ((PlayerList != null) && PlayerList.Count > 0)
            {
                foreach (KeyValuePair<string, Player> item in PlayerList)
                {
                    //Choose the Kills format for victory or defeat
                    string PlayerListFormat = _config["kills:Player:formats:list"];

                    //Create formatted string for return
                    strBounties += string.Format(PlayerListFormat, item.Value.PlayerName, item.Value.Description, item.Value.Reward, item.Value.Type.ToString(), item.Value.Expiration);
                }
                strBounties = $"```Player LIST\r\n\r\n{strBounties}\r\n\r\n```";
                return strBounties;
            }
            else
            {
                return "```Player LIST EMPTY```";
            }
        }

        public async Task<bool> SaveFileAsync()
        {
            string filename = _config["data:Playerfile"];
            string filedir = _config["data:datadir"];
            bool success = false;

            try
            {
                string json = JsonConvert.SerializeObject(PlayerList);

                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(filename))               // Create Playerlist file if it doesn't exist
                {
                    File.Create(filename).Dispose();
                }

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, PlayerList);
                }

                success = true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR SAVING Player LIST JSON\r\n" + ex.ToString());
            }
            return success;
        }

        public async Task LoadFileAsync()
        {
            string filename = _config["data:Playerfile"];
            string filedir = _config["data:datadir"];
            Dictionary<string, Player> PlayerList = new Dictionary<string, Player>();
            try
            {
                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                {
                    Directory.CreateDirectory(filedir);
                }

                if (!File.Exists(filename))               // Create Playerlist file if it doesn't exist
                {
                    File.Create(filename).Dispose();
                }

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    PlayerList = serializer.Deserialize<Dictionary<string, Player>>(reader);
                }
                await Console.Out.WriteLineAsync($"*** LOADED PlayerFILE from {filedir}\\{filename}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING Player LIST JSON\r\n" + ex.ToString());
            }
        }
    }
}