using Discord;
using Discord.WebSocket;
using Kamael.Packets;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.Extensions.Configuration;
using PacketDotNet;
using SharpPcap;
using System;
using System.Threading.Tasks;
using static Luci.KillListService;

namespace Luci.Services
{
    public class PacketService
    {

        private static DiscordSocketClient _discord;
        private static KillListService _killService;
        private static IConfigurationRoot _config;


        public PacketService(DiscordSocketClient Discord, KillListService KillService, IConfigurationRoot config)
        {
            _discord = Discord;
            _killService = KillService;
            _config = config;
        }


        private async Task<IL2RPacket> ProcessPackets(byte[] payloadData)
        {
            try
            {
                //L2RPacketService proceesses the incoming payload and translates it to a concrete class
                IL2RPacket l2rPacket = L2RPacketService.AppendIncomingData(payloadData);


                if (l2rPacket is PacketClanMemberKillNotify && _config["killlist:enabled"] == "true")
                {
                    //NOTIFY KILL
                    await NotifyKill((PacketClanMemberKillNotify)l2rPacket);
                }
                else if (l2rPacket is PacketChatGuildListReadResult && _config["clanchat:enabled"] == "true")
                {
                    //NOTIFY CLAN CHAT
                    await NotifyClanChat((PacketChatGuildListReadResult)l2rPacket);
                }

                return l2rPacket;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                return null;
            }
        }


        public async void PacketCapturer(object sender, CaptureEventArgs e)
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
