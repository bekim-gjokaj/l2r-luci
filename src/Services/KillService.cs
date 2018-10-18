using Luci.Models;
using Luci.Models.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci
{
    public class KillService
    {
        public static List<KillsItem> KillLog { get; set; }
        public static IDictionary KillCountPersonal { get; set; }
        public static IDictionary KillCountClan { get; set; }
        public static IDictionary KillCountAlliance { get; set; }
        public static IDictionary BountyList { get; set; }

        public KillService(List<KillsItem> killLog, IDictionary bountyList, IDictionary killCountPersonal, IDictionary killCountClan, IDictionary killCountAlliance)
        {
            KillLog = killLog;
            KillCountPersonal = killCountPersonal;
            KillCountClan = killCountClan;
            KillCountAlliance = killCountAlliance;
            BountyList = bountyList;
        }

        public async Task StartAsync()
        {
            KillLog = new List<KillsItem>();
            Bounty bounty = new Bounty
            {
                Log = new Dictionary<string, int>()
            };
            BountyList = new Dictionary<string, Bounty>();
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
        public static async Task<int> GetCountAsync(string Name, KillsType KillType)
        {
            try
            {
                IDictionary dictionary = null;
                switch (KillType)
                {
                    case KillsType.Personal:
                        dictionary = KillCountPersonal;
                        break;

                    case KillsType.Clan:
                        dictionary = KillCountClan;
                        break;

                    case KillsType.Alliance:
                        dictionary = KillCountAlliance;
                        break;
                }

                //If personal, check for name
                if (dictionary.Contains(Name))
                {
                    return (int)dictionary[Name];
                }
                else
                {
                    dictionary.Add(Name, 0);
                    return (int)dictionary[Name];
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        public static async Task<List<KillsItem>> GetKillLogAsync()
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

        public static async Task<Dictionary<string, Bounty>> GetBountyListAsync()
        {
            try
            {
                return (Dictionary<string, Bounty>)BountyList;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<Bounty> FindBountyByNameAsync(string Name)
        {
            try
            {
                Bounty bounty = (Bounty)BountyList[Name];
                if (bounty != null)
                {
                    return bounty;
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

        public static async Task<Dictionary<string, Bounty>> AddBountyAsync(string Player, string Description, string Reward, DateTime Expiration, string Type)
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
                    BountyID = new Guid(),
                    PlayerName = Player,
                    Description = Description,
                    Reward = Reward,
                    Log = new Dictionary<string, int>(),
                    Type = tmpType,
                    Expiration = tmpExp
                };

                BountyList.Add(Player, bounty);
                return (Dictionary<string, Bounty>)BountyList;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async Task<Bounty> AddBountyKillAsync(Bounty bounty, string Player)
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

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public static async Task<KillsItem> ProcessKillAsync(string Name1, string Clan1, string Name2, string Clan2)
        {
            try
            {
                ProcessCounts(Name1, Name2, KillCountPersonal);
                ProcessCounts(Clan1, Clan2, KillCountClan);

                KillsItem killItem = new KillsItem
                {
                    P1 = Name1,
                    P2 = Name2,
                    Clan1 = Clan1,
                    Clan2 = Clan2,
                    Date = DateTime.Now
                };
                killItem.P1KillCount = await GetCountAsync(killItem.P1, KillsType.Personal);
                killItem.P2KillCount = await GetCountAsync(killItem.P2, KillsType.Personal);
                killItem.Clan1KillCount = await GetCountAsync(killItem.Clan1, KillsType.Clan);
                killItem.Clan2KillCount = await GetCountAsync(killItem.Clan2, KillsType.Clan);
                Bounty bounty = await FindBountyByNameAsync(killItem.P2);
                if (bounty != null)
                {
                    killItem.BountyID = bounty.BountyID;
                    Bounty result = await AddBountyKillAsync(bounty, killItem.P1);
                }

                KillLog.Add(killItem);

                return killItem;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public static async void ProcessCounts(string Name1, string Name2, IDictionary dictionary)
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
    }
}