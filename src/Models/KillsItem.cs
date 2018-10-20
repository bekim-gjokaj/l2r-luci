using System;
using System.Collections.Generic;
using System.Text;

namespace Luci.Models
{
    public class KillsItem
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
        public Bounty PlayerBounty { get; set; }
    }
}
