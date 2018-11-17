using System;
using System.Collections.Generic;
using System.Text;

namespace Luci.Models
{
    public class Survey
    {
        public SortedDictionary<string, string> Responses = new SortedDictionary<string, string>();
        public bool Enabled { get; set; }
        public Int64 GuildId { get; set; }
        public Int64 ChannelId { get; set; }
        public int Color1 { get; set; }
        public int Color2 { get; set; }
        public int Color3 { get; set; }
        public string Count { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }
        public string ResponseMsg { get; set; }
        public string Desc { get; set; }
        public string Title { get; set; }
        public string TitleYes { get; set; }
        public string TitleNo { get; set; }
        public string TitleMaybe { get; set; }
        public string TitleTotal { get; set; }
      
    }
}
