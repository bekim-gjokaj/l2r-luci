using Discord;
using Discord.WebSocket;
using Kamael.Packets.Character;
using Kamael.Packets.Clan;
using Luci.Models;
using Luci.Models.Enums;
using Luci.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Luci
{
    public class KillService
    {
        private IConfiguration _config { get; set; }
        private DiscordSocketClient _discord { get; set; }
        private BountyService _bountyService { get; set; }
        private IDictionary KillCountPersonal { get; set; }
        private IDictionary KillCountClan { get; set; }
        private IDictionary KillCountAlliance { get; set; }
        private IDictionary KillTagList { get; set; }
        private List<KillsItem> KillLog;

        public KillService(IConfiguration Configuration, BountyService BountyService, DiscordSocketClient Discord)
        {
            _config = Configuration;
            _bountyService = BountyService;
            _discord = Discord;

            KillLog = new List<KillsItem>();
            KillCountPersonal = new Dictionary<string, int>();
            KillCountClan = new Dictionary<string, int>();
            KillCountAlliance = new Dictionary<string, int>();
            KillTagList = new Dictionary<string, SocketUser>();

            LoadFileAsync().Wait();
        }

        public async Task StartAsync(IConfiguration Configuration, BountyService BountyService)
        {
            _config = Configuration;
            _bountyService = BountyService;

            KillLog = new List<KillsItem>();
            KillTagList = new Dictionary<string, SocketUser>();
            KillCountPersonal = new Dictionary<string, int>();
            KillCountClan = new Dictionary<string, int>();
            KillCountAlliance = new Dictionary<string, int>();
        }


        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<int> GetKillCountByPlayerAsync(string Name, KillsType KillType, int Days)
        {
            try
            {
                var filtered = KillLog
                 .Where(t => t.Date <= DateTime.Now &&
                           t.Date >= DateTime.Now.Subtract(TimeSpan.FromDays(Days)) &&
                           t.P1 == Name)
                           .Count();

                return filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<int> GetKillCountByClanAsync(string Name, KillsType KillType, int Days)
        {
            try
            {
                var filtered = KillLog
                 .Where(t => t.Date <= DateTime.Now &&
                           t.Date >= DateTime.Now.Subtract(TimeSpan.FromDays(Days)) &&
                           t.Clan1 == Name)
                           .Count();

                return filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }



        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<int> GetDeathCountByPlayerAsync(string Name, KillsType KillType, int Days)
        {
            try
            {
                var filtered = KillLog
                 .Where(t => t.Date <= DateTime.Now &&
                           t.Date >= DateTime.Now.Subtract(TimeSpan.FromDays(Days)) &&
                           t.P2 == Name)
                           .Count();

                return filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<int> GetDeathCountByClanAsync(string Name, KillsType KillType, int Days)
        {
            try
            {
                var filtered = KillLog
                 .Where(t => t.Date <= DateTime.Now &&
                           t.Date >= DateTime.Now.Subtract(TimeSpan.FromDays(Days)) &&
                           t.Clan2 == Name)
                           .Count();

                return filtered;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<List<Embed>> GetRecentKillsAsync()
        {

            List<Embed> embeds = null;
            try
            {
                var filtered = KillLog
                 .OrderBy(x => x.Date)
                 .Take(5)
                 .ToList();

                embeds = await GetEmbedAsync(filtered);

                return embeds;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return embeds;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<List<Embed>> GetRecentKillsByPlayerAsync(string Name)
        {

            List<Embed> embeds = null;
            try
            {
                var filtered = KillLog
                 .Where(x => x.P1 == Name)
                 .OrderBy(x => x.Date)
                 .Take(5)
                 .ToList();

                embeds = await GetEmbedAsync(filtered);

                return embeds;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return embeds;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<List<Embed>> GetRecentDeathsByPlayerAsync(string Name)
        {

            List<Embed> embeds = null;
            try
            {
                var filtered = KillLog
                 .Where(x => x.P2 == Name)
                 .OrderBy(x => x.Date)
                 .Take(5)
                 .ToList();

                embeds = await GetEmbedAsync(filtered);

                return embeds;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return embeds;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<List<Embed>> GetRecentKillsByClanAsync(string Name)
        {

            List<Embed> embeds = null;
            try
            {
                var filtered = KillLog
                 .Where(x => x.Clan1 == Name)
                 .OrderBy(x => x.Date)
                 .Take(5)
                 .ToList();

                embeds = await GetEmbedAsync(filtered);

                return embeds;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return embeds;
            }
        }

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<List<Embed>> GetRecentDeathsByClanAsync(string Name)
        {

            List<Embed> embeds = null;
            try
            {
                var filtered = KillLog
                 .Where(x => x.Clan2 == Name)
                 .OrderBy(x => x.Date)
                 .Take(5)
                 .ToList();

                embeds = await GetEmbedAsync(filtered);

                return embeds;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return embeds;
            }
        }

        public async Task<List<KillsItem>> GetKillLogAsync()
        {
            try
            {
                return KillLog;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task AddPlayerTagging(SocketUser User, string PlayerName)
        {
            try
            {
                if (KillTagList is null)
                    KillTagList = new Dictionary<string, SocketUser>();

                KillTagList.Add(PlayerName, User);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<KillsItem> ProcessKillAsync(string Name1, string Clan1, string Name2, string Clan2, string Region, int Channel)
        {
            try
            {

                var result = await SaveFileAsync();

                //Process Kill Counts
                await ProcessCounts(Name1, Name2, KillCountPersonal);
                await ProcessCounts(Clan1, Clan2, KillCountClan);

                //Check bounty status
                bool isBountyKill = false;
                Bounty bounty = await _bountyService.FindBountyByNameAsync(Name2);
                Bounty clanbounty = await _bountyService.FindBountyByNameAsync(Clan2);

                if (bounty != null)
                {
                    bounty = await _bountyService.IncrementKillAsync(bounty, Name1);
                }
                else if (clanbounty != null)
                {
                    
                    if (clanbounty != null)
                    {
                        clanbounty = await _bountyService.IncrementKillAsync(clanbounty, Name1);
                    }
                }


                var p1killcount = await GetKillCountByPlayerAsync(Name1, KillsType.Personal, 1);
                var p2killcount = await GetKillCountByPlayerAsync(Name2, KillsType.Personal, 1);
                var clan1killcount = await GetKillCountByPlayerAsync(Clan1, KillsType.Clan, 1);
                var clan2killcount = await GetKillCountByPlayerAsync(Clan2, KillsType.Clan, 1);

                var p1deathcount = await GetDeathCountByPlayerAsync(Name1, KillsType.Personal, 1);
                var p2deathcount = await GetDeathCountByPlayerAsync(Name2, KillsType.Personal, 1);
                var clan1deathcount = await GetDeathCountByPlayerAsync(Clan1, KillsType.Clan, 1);
                var clan2deathcount = await GetDeathCountByPlayerAsync(Clan2, KillsType.Clan, 1);

                //Create KillsItem object
                KillsItem killItem = new KillsItem();

                killItem.P1 = Name1;
                killItem.P2 = Name2;
                killItem.Clan1 = Clan1;
                killItem.Clan2 = Clan2;
                killItem.Date = DateTime.Now;
                killItem.P1KillCount = p1killcount;
                killItem.P2KillCount = p2killcount;
                killItem.Clan1KillCount = clan1killcount;
                killItem.Clan2KillCount = clan2killcount;
                killItem.P1DeathCount = p1deathcount;
                killItem.P2DeathCount = p2deathcount;
                killItem.Clan1DeathCount = clan1deathcount;
                killItem.Clan2DeathCount = clan2deathcount;
                killItem.PlayerBounty = bounty;
                killItem.Region = Region;
                killItem.Channel = Channel;

                KillLog.Add(killItem);

                result = await SaveFileAsync();

                return killItem;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task ProcessCounts(string Name1, string Name2, IDictionary dictionary)
        {
            //PROCESS KILLER
            if (dictionary.Contains(Name1))
            {
                dictionary[Name1] = (int)dictionary[Name1] + 1;
            }
            else
            {
                dictionary.Add(Name1, 1);
            }

            //PROCESS KILLER
            if (dictionary.Contains(Name2))
            {
                dictionary[Name2] = (int)dictionary[Name2] - 1;
            }
            else
            {
                dictionary.Add(Name2, -1);
            }
        }

        public async Task<List<Embed>> KillCountByVersusAsync(string P1, string P2)
        {
            string RecentKills = "";
            string RecentKillsFormat = "Player {0} has {1} kill(s) Vs. Player {2} has {3} kill(s).\r\n";
            int P1Count = 0;
            int P2Count = 0;

            List<KillsItem> KillLog = await GetKillLogAsync();

            foreach (KillsItem killItem in KillLog)
            {
                if (killItem.P1.ToLower() == P1.ToLower() && killItem.P2.ToLower() == P2.ToLower())
                {
                    if (killItem.Clan1 == _config["kills:clanname"])
                    {
                        P1Count++;
                        P2Count--;
                    }
                    else
                    {
                        P2Count++;
                        P1Count--;
                    }
                }
            }
            //Create formatted string for return
            RecentKills += string.Format(RecentKillsFormat, P1, P1Count, P2, P2Count);

            return new List<Embed>();
        }

        public async Task<SocketUser> CheckTagPlayer(string PlayerName)
        {
            SocketUser user = null;

            if (KillTagList != null)
            {
                if (KillTagList.Contains(PlayerName))
                {
                    user = (SocketUser)KillTagList[PlayerName];
                } 
            }

            var save = await SaveFileAsync();
            return user;
            
        }

        public async Task<List<Embed>> GetEmbedAsync(List<KillsItem> kills)
        {
            try
            {

                //Create the object to return
                List<Embed> embeds = new List<Embed>();

                if ((kills != null) && kills.Count > 0)
                {
                    //LOOP through kills
                    foreach (KillsItem kill in kills)
                    {
                        // Setup embeded card
                        EmbedBuilder builder = new EmbedBuilder();


                        string formatKill = "";


                        //Load kill format
                        if (kill.PlayerBounty != null)
                        {
                            builder.Color = new Color(244, 244, 66);
                            string title = string.Format(_config["kills:formats:bountytitle"],
                                                                    kill.P1,
                                                                    kill.P1KillCount,
                                                                    kill.P1DeathCount,
                                                                    kill.Clan1,
                                                                    kill.Clan1KillCount,
                                                                    kill.Clan1DeathCount,
                                                                    kill.P2,
                                                                    kill.P2KillCount,
                                                                    kill.P2DeathCount,
                                                                    kill.Clan2,
                                                                    kill.Clan2KillCount,
                                                                    kill.Clan2DeathCount
                                                                    );
                            builder.Title = title;

                            builder.Description = string.Format(_config["kills:formats:bountydesc"], 
                                                                    kill.Region, 
                                                                    kill.Channel, 
                                                                    kill.Date.ToString());


                            var user1 = await CheckTagPlayer(kill.P1);
                            var user2 = await CheckTagPlayer(kill.P2);

                            if (user1 != null)
                                builder.Description += $"\r\n( {user1.Mention} )";
                            if (user2 != null)
                                builder.Description += $"\r\n( {user2.Mention} )";
                        }
                        else if (kill.Clan1 == _config["kills:clanname"])
                        {
                            builder.Color = new Color(0, 255, 33);
                            string title = string.Format(_config["kills:formats:victorytitle"],
                                                                    kill.P1,
                                                                    kill.P1KillCount,
                                                                    kill.P1DeathCount,
                                                                    kill.Clan1,
                                                                    kill.Clan1KillCount,
                                                                    kill.Clan1DeathCount,
                                                                    kill.P2,
                                                                    kill.P2KillCount,
                                                                    kill.P2DeathCount,
                                                                    kill.Clan2,
                                                                    kill.Clan2KillCount,
                                                                    kill.Clan2DeathCount
                                                                    );
                            builder.Title = title;
                            builder.Description = string.Format(_config["kills:formats:victorydesc"], kill.Region, kill.Channel, kill.Date.ToString());


                            var user1 = await CheckTagPlayer(kill.P1);
                            var user2 = await CheckTagPlayer(kill.P2);

                            if (user1 != null)
                                builder.Description += $"\r\n( {user1.Mention} )";
                            if (user2 != null)
                                builder.Description += $"\r\n( {user2.Mention} )";
                        }
                        else
                        {
                            builder.Color = new Color(255, 0, 0);
                            string title = string.Format(_config["kills:formats:defeattitle"].ToString(),
                                                                    kill.P1,
                                                                    kill.P1KillCount,
                                                                    kill.P1DeathCount,
                                                                    kill.Clan1,
                                                                    kill.Clan1KillCount,
                                                                    kill.Clan1DeathCount,
                                                                    kill.P2,
                                                                    kill.P2KillCount,
                                                                    kill.P2DeathCount,
                                                                    kill.Clan2,
                                                                    kill.Clan2KillCount,
                                                                    kill.Clan2DeathCount
                                                                    );
                            builder.Title = title;
                            builder.Description = string.Format(_config["kills:formats:defeatdesc"], kill.Region, kill.Channel, kill.Date.ToString());


                            var user1 = await CheckTagPlayer(kill.P1);
                            var user2 = await CheckTagPlayer(kill.P2);

                            if (user1 != null)
                                builder.Description += $"\r\n( {user1.Mention} )";
                            if (user2 != null)
                                builder.Description += $"\r\n( {user2.Mention} )";
                        }
                        ////Add fields to embed card for the bounty
                        //builder.AddField(x =>
                        //{
                        //    x.Name = $":gift:   {kill.P1}";
                        //    x.Value = "";
                        //    x.IsInline = false;
                        //});

                        embeds.Add(builder.Build());
                    }

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

        public async Task NotifyKill(PacketClanMemberKillNotify kill)
        {
            KillsItem killItem = await ProcessKillAsync(kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name, Convert.ToString(kill.Region), Convert.ToInt32(kill.Channel));

            //dictRecentKills.Add(builder);
            if (killItem != null)
            {

                List<KillsItem> killList = new List<KillsItem>();
                killList.Add(killItem);

                List<Embed> embeds = await GetEmbedAsync(killList);


                ulong guildId = Convert.ToUInt64(_config["fort:attendance:guildid"]);
                ulong channelId = Convert.ToUInt64(_config["kills:channelid"]);
                await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync("", false, embeds[0]);
            }

            //SPECIAL PLAYER
            //var tmpName = "";
            //if (kill.PlayerName == _config["kills:specialname"] && tmpName == _config["kills:specialchannel"])
            //{
            //    SocketUser P1User = await UtilService.FindUser(killItem.P1);
            //    SocketUser P2User = await UtilService.FindUser(killItem.P2);

            //    string docbuilder = string.Format(_config["kills:specialformat"],
            //            (P1User == null) ? killItem.P1 : P1User.Mention,
            //            (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
            //            killItem.Clan1,
            //            (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
            //            (P2User == null) ? killItem.P2 : P2User.Mention,
            //            (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
            //            killItem.Clan2,
            //            (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
            //            DateTime.Now);
            //    _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(_config["fort:attendance:msg"]);
            //}
        }
        public async Task NotifyKill(PacketPlayerKillNotify kill)
        {
            KillsItem killItem = await ProcessKillAsync(kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name, Convert.ToString(kill.Region), Convert.ToInt32(kill.Channel) + 1);

            //dictRecentKills.Add(builder);
            if (killItem != null)
            {

                List<KillsItem> killList = new List<KillsItem>();
                killList.Add(killItem);

                List<Embed> embeds = await GetEmbedAsync(killList);


                ulong guildId = Convert.ToUInt64(_config["fort:attendance:guildid"]);
                ulong channelId = Convert.ToUInt64(_config["kills:channelid"]);
                await _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync("", false, embeds[0]);
            }

            //SPECIAL PLAYER
            //var tmpName = "";
            //if (kill.PlayerName == _config["kills:specialname"] && tmpName == _config["kills:specialchannel"])
            //{
            //    SocketUser P1User = await UtilService.FindUser(killItem.P1);
            //    SocketUser P2User = await UtilService.FindUser(killItem.P2);

            //    string docbuilder = string.Format(_config["kills:specialformat"],
            //            (P1User == null) ? killItem.P1 : P1User.Mention,
            //            (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
            //            killItem.Clan1,
            //            (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
            //            (P2User == null) ? killItem.P2 : P2User.Mention,
            //            (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
            //            killItem.Clan2,
            //            (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
            //            DateTime.Now);
            //    _discord.GetGuild(guildId).GetTextChannel(channelId).SendMessageAsync(_config["fort:attendance:msg"]);
            //}
        }



        public async Task<bool> SaveFileAsync()
        {

            string filename = _config["data:killfile"];
            string filedir = _config["data:datadir"];
            bool success = false;

            try
            {

                string jsonLog = JsonConvert.SerializeObject(KillLog);
                string jsonTagList = JsonConvert.SerializeObject(KillTagList);
                string jsonPersonal = JsonConvert.SerializeObject(KillCountPersonal);
                string jsonClan = JsonConvert.SerializeObject(KillCountClan);
                string jsonAlliance = JsonConvert.SerializeObject(KillCountAlliance);

                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);


                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json").Dispose();

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, KillLog);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, KillLog);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, KillLog);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, KillLog);
                }
                using (StreamWriter sw = new StreamWriter($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, KillLog);
                }

                await Console.Out.WriteLineAsync("*** SAVED KILL LIST FILE\r\n");
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

            string filename = _config["data:killfile"];
            string filedir = _config["data:datadir"];

            try
            {


                if (!Directory.Exists(filedir))     // Create the data directory if it doesn't exist
                    Directory.CreateDirectory(filedir);

                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json").Dispose();
                if (!File.Exists($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json"))
                    File.Create($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json").Dispose();
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;


                KillLog = new List<KillsItem>();
                KillCountPersonal = new Dictionary<string, int>();
                KillCountClan = new Dictionary<string, int>();
                KillCountAlliance = new Dictionary<string, int>();
                KillTagList = new Dictionary<string, SocketUser>();

                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_log.json"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    KillLog = serializer.Deserialize<List<KillsItem>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_taglist.json"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    KillTagList = serializer.Deserialize<Dictionary<string, SocketUser>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_personal.json"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    KillCountPersonal = serializer.Deserialize<Dictionary<string, int>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_clan.json"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    KillCountClan = serializer.Deserialize<Dictionary<string, int>>(reader);
                }
                using (StreamReader sw = new StreamReader($"{AppContext.BaseDirectory}\\{filedir}\\{filename}_alliance.json"))
                using (JsonReader reader = new JsonTextReader(sw))
                {
                    KillCountAlliance = serializer.Deserialize<Dictionary<string, int>>(reader);
                }

                await Console.Out.WriteLineAsync($"*** LOADED KILLFILE from {filedir}\\{filename}");

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("****************** ERROR LOADING KILL LIST JSON\r\n" + ex.ToString());
            }

        }

    }
}
