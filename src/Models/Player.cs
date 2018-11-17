using Luci.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Luci.Models
{

    public class Player
    {
        public Guid BountyID { get; set; }
        public string PlayerName { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        public Dictionary<string, int> Log { get; set; }
        public KillsType Type { get; set; }
        public DateTime Expiration { get; set; }
    }
}
