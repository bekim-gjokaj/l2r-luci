using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Luci
{

    public class KillListItem
    {
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string Clan1 { get; set; }
        public string Clan2 { get; set; }
        public DateTime Date { get; set; }
        public int P1KillCount { get; set; }
        public int P2KillCount { get; set; }
        public int Clan1KillCount { get; set; }
        public int Clan2KillCount { get; set; }
        public bool IsBountyKill { get; set; }
        public Guid BountyID { get; set; }

    }

    public class KillListService
    {
        public static List<KillListItem> KillLog { get; set; }
        public static IDictionary KillCountPersonal { get; set; }
        public static IDictionary KillCountClan { get; set; }
        public static IDictionary KillCountAlliance { get; set; }


        public enum KillListType
        {
            Personal = 0,
            Clan = 1,
            Alliance = 2
        }

        public async Task StartAsync()
        {
            KillLog = new List<KillListItem>();
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
        public async Task<int> GetCountAsync(string Name, KillListType KillType)
        {
            try
            {
                IDictionary dictionary = null;
                switch (KillType)
                {
                    case KillListType.Personal:
                        dictionary = KillCountPersonal;
                        break;

                    case KillListType.Clan:
                        dictionary = KillCountClan;
                        break;

                    case KillListType.Alliance:
                        dictionary = KillCountAlliance;
                        break;
                }

                //If personal, check for name
                if (dictionary.Contains(Name))
                    return (int)dictionary[Name];
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

        /// <summary>
        /// Gets the count personal.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="KillType">Type of the kill.</param>
        /// <returns></returns>
        public async Task<KillListItem> ProcessKillAsync(string Name1, string Clan1, string Name2, string Clan2)
        {
            try
            {
                ProcessCounts(Name1, Name2, KillCountPersonal);
                ProcessCounts(Clan1, Clan2, KillCountClan);

                KillListItem killItem = new KillListItem();
                killItem.P1 = Name1;
                killItem.P2 = Name2;
                killItem.Clan1 = Clan1;
                killItem.Clan2 = Clan2;
                killItem.Date = DateTime.Now;
                killItem.P1KillCount = await GetCountAsync(killItem.P1, KillListType.Personal);
                killItem.P2KillCount = await GetCountAsync(killItem.P2, KillListType.Personal);
                killItem.Clan1KillCount = await GetCountAsync(killItem.Clan1, KillListType.Clan);
                killItem.Clan2KillCount = await GetCountAsync(killItem.Clan2, KillListType.Clan);

                KillLog.Add(killItem);

                return killItem;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async void ProcessCounts(string Name1, string Name2, IDictionary dictionary)
        {
            //PROCESS KILLER
            if (dictionary.Contains(Name1))
                dictionary[Name1] = (Int32)dictionary[Name1] + 1;
            else
                dictionary.Add(Name1, 1);

            //PROCESS KILLER
            if (dictionary.Contains(Name2))
                dictionary[Name2] = (Int32)dictionary[Name2] - 1;
            else
                dictionary.Add(Name2, -1);
        }
    }




}