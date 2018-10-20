using Discord.WebSocket;
using Kamael.Packets;
using Kamael.Packets.Character;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.Extensions.Configuration;
using PacketDotNet;
using SharpPcap;
using System;

namespace Luci.Services
{
    public class PacketService
    {
        //I want to ensure there is only ever 1 L2RPacketService (the packet logger)
        //I'm going to rely on Dependency injection in Startup.cs to provide
        //me with a Singleton instance which I am then going to refer to.
        private L2RPacketService _L2RPacketLogger { get; set; }

        private BountyService _bountyService { get; set; }
        private FortService _fortService { get; set; }
        private KillService _killService { get; set; }
        private DiscordSocketClient _discord { get; set; }
        private IConfiguration _config { get; set; }

        public PacketService(L2RPacketService L2RPacketLogger,         //DI should inject my Singleton instance here
                                BountyService BountyService,
                                FortService FortService,
                                KillService KillService,
                                DiscordSocketClient Discord,
                                IConfiguration config)
        {
            _L2RPacketLogger = L2RPacketLogger;
            _discord = Discord;
            _bountyService = BountyService;
            _fortService = FortService;
            _killService = KillService;
            _config = config;

            _L2RPacketLogger.StartAsync(InitializeDevice()).Wait();
        }

        private ICaptureDevice InitializeDevice()
        {
            /* Retrieve the device list  part of initialization*/
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            int iface = L2RPacketService.Initialization(Kamael.Globals.args);
            ICaptureDevice device = CaptureDeviceList.Instance[Convert.ToInt32(_config["packets:interface"])];

            //Register our handler function to the 'packet arrival' event
            //This uses a Delegate to pass a reference to PacketCapturer() below
            device.OnPacketArrival += new PacketArrivalEventHandler(PacketCapturer);

            return device;
        }

        public void PacketCapturer(object sender, CaptureEventArgs e)
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

                //process incoming packets
                if (srcPort == 12000)
                {
                    l2rPacket = ProcessPackets(payloadData);
                }
            }
        }

        private IL2RPacket ProcessPackets(byte[] payloadData)
        {
            try
            {
                //L2RPacketService proceesses the incoming payload and translates it to a concrete class
                IL2RPacket l2rPacket = L2RPacketService.AppendIncomingData(payloadData);
                if (l2rPacket is PacketPlayerKillNotify)
                {
                    //NOTIFY KILL
                    _killService.NotifyKill((PacketPlayerKillNotify)l2rPacket).Wait();
                    
                }
                else if (l2rPacket is PacketChatGuildListReadResult && _config["clanchat:enabled"] == "true")
                {
                    //NOTIFY CLAN CHAT
                    UtilService.NotifyClanChat((PacketChatGuildListReadResult)l2rPacket).Wait();
                }

                return l2rPacket;
            }
            catch (Exception)
            {
                //Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}