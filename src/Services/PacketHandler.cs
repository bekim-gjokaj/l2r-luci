using Discord.Commands;
using Discord.WebSocket;
using Kamael.Packets;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luci.Services
{
    public class PacketHandler
    {

        public static DiscordSocketClient _discord { get; set; }
        public static List<string> dictRecentKills = new List<string>();


        public PacketHandler(DiscordSocketClient Discord)
        {
            _discord = Discord;
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

                Console.WriteLine("{0}:{1}:{2}.{3}\tLen={4}\t{5}:{6} -> {7}:{8}",
                time.Hour, time.Minute, time.Second, time.Millisecond, len,
                srcIp, srcPort, dstIp, dstPort);

                // Decrypt and process incoming packets
                if (srcPort == 12000)
                {
                    l2rPacket = L2RPacketService.AppendIncomingData(payloadData);

                    if (l2rPacket is PacketClanMemberKillNotify)
                    {
                        await Task.Run(async () => await NotifyKill((PacketClanMemberKillNotify)l2rPacket));
                    }
                    else if (l2rPacket is PacketChatGuildListReadResult)
                    {
                        await Task.Run(async () => await NotifyClanChat((PacketChatGuildListReadResult)l2rPacket));
                    }
                }
            }
        }

        public static async Task NotifyKill(PacketClanMemberKillNotify kill)
        {

            string builder = string.Format(":boom: **{0}** _*of*_  **{1}** _*killed*_  **{2}** _*of*_  **{3}** @ {4}", kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name, DateTime.Now);
            string docbuilder = string.Format(":kissing_heart: **{0}** _*of*_  **{1}** _*killed*_  **{2}** _*of*_  **{3}** @ {4}", kill.PlayerName, kill.ClanName, kill.Player2Name, kill.Clan2Name, DateTime.Now);
            dictRecentKills.Add(builder);

            foreach (var guild in _discord.Guilds)
            {
                foreach (var textchan in guild.TextChannels)
                {
                    if (textchan.Name == "pzychos-bot-test" && (kill.Clan2Name == "Legacy" || kill.ClanName == "Legacy" ))
                    {
                        await textchan.SendMessageAsync(builder);
                    }
                    if(kill.PlayerName == "DocHoliday" && textchan.Name == "docs-slaughterhouse")
                    {
                        await textchan.SendMessageAsync(docbuilder);
                    }
                   
                }
            }
        }
        public static async Task NotifyClanChat(PacketChatGuildListReadResult chat)
        {

            string builder = string.Format(":speech_balloon: ***{0}:\t\t*** **{1}** _\t@ {2}_", chat.PlayerName, chat.Message, DateTime.Now);
            dictRecentKills.Add(builder);

            foreach (var guild in _discord.Guilds)
            {
                foreach (var textchan in guild.TextChannels)
                {
                    if (textchan.Name == "in-game-clan-chat")
                    {
                        await textchan.SendMessageAsync(builder);
                    }
                }
            }
        }

    }
}
