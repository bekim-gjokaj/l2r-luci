using Discord.WebSocket;
using Kamael.Packets;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.Extensions.Configuration;
using PacketDotNet;
using SharpPcap;
using System;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class PacketHandlerService
    {

        private static DiscordSocketClient _discord;
        private static KillListService _killService;
        private static IConfigurationRoot _config;


        public PacketHandlerService(DiscordSocketClient Discord, KillListService KillService, IConfigurationRoot config)
        {
            _discord = Discord;
            _killService = KillService;
            _config = config;
        }


        private static async Task<IL2RPacket> ProcessPackets(byte[] payloadData)
        {
            try
            {
                //L2RPacketService proceesses the incoming payload and translates it to a concrete class
                IL2RPacket l2rPacket = L2RPacketService.AppendIncomingData(payloadData);


                if (l2rPacket is PacketClanMemberKillNotify && _config["killlist:enabled"] == "true")
                {
                    //NOTIFY KILL
                    await Task.Run(async () => await NotifyKill((PacketClanMemberKillNotify)l2rPacket));
                }
                else if (l2rPacket is PacketChatGuildListReadResult && _config["clanchat:enabled"] == "true")
                {
                    //NOTIFY CLAN CHAT
                    await Task.Run(async () => await NotifyClanChat((PacketChatGuildListReadResult)l2rPacket));
                }

                return l2rPacket;
            }
            catch (Exception)
            {
                return null;
            }
        }



        public static async Task NotifyKill(PacketClanMemberKillNotify kill)
        {
            KillListItem killItem = await _killService.ProcessKillAsync(kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name);

            //dictRecentKills.Add(builder);

            foreach (SocketGuild guild in _discord.Guilds)
            {
                foreach (SocketTextChannel textchan in guild.TextChannels)
                {
                    if (textchan.Name == _config["killlist:clanchannel"] && (kill.Clan2Name == _config["killlist:clanname"] || kill.ClanName == _config["killlist:clanname"]))
                    {
                        string strFormat = "";
                        if (kill.ClanName == _config["killlist:clanname"])
                        {
                            strFormat = _config["killlist:victoryformat"];
                        }
                        else
                        {
                            strFormat = _config["killlist:defeatformat"];
                        }

                        string result = string.Format(strFormat,
                                killItem.P1,
                                (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                killItem.Clan1,
                                (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                killItem.P2,
                                (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                killItem.Clan2,
                                (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                DateTime.Now);
                        await textchan.SendMessageAsync(result);

                    }
                    if (kill.PlayerName == _config["killlist:specialname"] && textchan.Name == _config["killlist:specialchannel"])
                    {
                        string docbuilder = string.Format(_config["killlist:specialformat"],
                                killItem.P1,
                                (killItem.P1KillCount < 0) ? Convert.ToString(killItem.P1KillCount) : "+" + killItem.P1KillCount,
                                killItem.Clan1,
                                (killItem.Clan1KillCount < 0) ? Convert.ToString(killItem.Clan1KillCount) : "+" + killItem.Clan1KillCount,
                                killItem.P2,
                                (killItem.P2KillCount < 0) ? Convert.ToString(killItem.P2KillCount) : "+" + killItem.P2KillCount,
                                killItem.Clan2,
                                (killItem.Clan2KillCount < 0) ? Convert.ToString(killItem.Clan2KillCount) : "+" + killItem.Clan2KillCount,
                                DateTime.Now);
                        await textchan.SendMessageAsync(docbuilder);
                    }

                }
            }
        }
        public static async Task NotifyClanChat(PacketChatGuildListReadResult chat)
        {

            string builder = string.Format(":speech_balloon: ***{0}:\t\t*** **{1}** _\t@ {2}_", chat.PlayerName, chat.Message, DateTime.Now);
            //dictRecentKills.Add(builder);

            foreach (SocketGuild guild in _discord.Guilds)
            {
                foreach (SocketTextChannel textchan in guild.TextChannels)
                {
                    if (textchan.Name == "in-game-clan-chat")
                    {
                        await textchan.SendMessageAsync(builder);
                    }
                }
            }
        }

        public static async void PacketCapturer(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;
            IL2RPacket l2rPacket = null;

            Packet packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            TcpPacket tcpPacket = (PacketDotNet.TcpPacket)packet.Extract(typeof(PacketDotNet.TcpPacket));
            if (tcpPacket != null)
            {
                IPPacket ipPacket = (IPPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                byte[] payloadData = tcpPacket.PayloadData;

                //Console.WriteLine("{0}:{1}:{2}.{3}\tLen={4}\t{5}:{6} -> {7}:{8}",
                //time.Hour, time.Minute, time.Second, time.Millisecond, len,
                //srcIp, srcPort, dstIp, dstPort);

                // Decrypt and process incoming packets
                if (srcPort == 12000)
                {
                    l2rPacket = await ProcessPackets(payloadData);
                }
            }
        }

    }
}
