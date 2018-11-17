using Discord.WebSocket;
using Kamael.Packets;
using Kamael.Packets.Character;
using Kamael.Packets.Chat;
using Kamael.Packets.Clan;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using static Kamael.Packets.L2RPacketService;

namespace Luci.Services
{
    public class PacketService
    {
        //I want to ensure there is only ever 1 L2RPacketService (the packet logger)
        //I'm going to rely on Dependency injection in Startup.cs to provide
        //me with a Singleton instance which I am then going to refer to.
        private L2RPacketService _L2RPacketLogger { get; set; }

        private BountyService _bountyService { get; set; }
        private SurveyService _fortService { get; set; }
        private KillService _killService { get; set; }
        private PlayerService _playerService { get; set; }
        private DiscordSocketClient _discord { get; set; }
        private IConfiguration _config { get; set; }
        /// <summary>
        /// When true the background thread will terminate
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private bool BackgroundThreadStop;

        /// <summary>
        /// Object that is used to prevent two threads from accessing
        /// PacketQueue at the same time
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private object QueueLock = new object();

        /// <summary>
        /// The queue that the callback thread puts packets in. Accessed by
        /// the background thread when QueueLock is held
        /// </summary>
        private List<RawCapture> PacketQueue = new List<RawCapture>();

        /// <summary>
        /// The last time PcapDevice.Statistics() was called on the active device.
        /// Allow periodic display of device statistics
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private DateTime LastStatisticsOutput;

        /// <summary>
        /// Interval between PcapDevice.Statistics() output
        /// </summary>
        /// <param name="args">
        /// A <see cref="System.String"/>
        /// </param>
        private TimeSpan LastStatisticsInterval = new TimeSpan(0, 0, 2);

        private System.Threading.Thread backgroundThread;
        private PacketArrivalEventHandler arrivalEventHandler;
        private CaptureStoppedEventHandler captureStoppedEventHandler;

        private Queue<PacketWrapper> packetStrings;

        private int packetCount;
        private BindingSource bs;
        private ICaptureStatistics captureStatistics;
        private bool statisticsUiNeedsUpdate = false;


        public PacketService(L2RPacketService L2RPacketLogger,         //DI should inject my Singleton instance here
                                BountyService BountyService,
                                SurveyService FortService,
                                PlayerService PlayerService,
                                KillService KillService,
                                DiscordSocketClient Discord,
                                IConfiguration config)
        {
            _L2RPacketLogger = L2RPacketLogger;
            _discord = Discord;
            _bountyService = BountyService;
            _fortService = FortService;
            _playerService = PlayerService;
            _killService = KillService;
            _config = config;

            _L2RPacketLogger.L2RPacketArrivalEvent += OnL2RPacketArrival;
            _L2RPacketLogger.StartCapture();
        }


        private ICaptureDevice InitializeDevice()
        {
            /* Retrieve the device list  part of initialization*/
            CaptureDeviceList devices = CaptureDeviceList.Instance;
            ICaptureDevice device = CaptureDeviceList.Instance[Convert.ToInt32(_config["packets:interface"])];
            
            // setup background capture
            arrivalEventHandler = new PacketArrivalEventHandler(device_OnPacketArrival);
            device.OnPacketArrival += arrivalEventHandler;
            captureStoppedEventHandler = new CaptureStoppedEventHandler(device_OnCaptureStopped);
            device.OnCaptureStopped += captureStoppedEventHandler;

            return device;
        }


        void device_OnCaptureStopped(object sender, CaptureStoppedEventStatus status)
        {
            if (status != CaptureStoppedEventStatus.CompletedWithoutError)
            {
                //MessageBox.Show("Error stopping capture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            
        }

        private async void OnL2RPacketArrival(object sender, L2RPacketArrivalEventArgs e)
        {
            try
            {
                //L2RPacketService proceesses the incoming payload and translates it to a concrete class
                IL2RPacket l2rPacket = e.Packet;
                if (l2rPacket is PacketPlayerKillNotify)
                {
                    //NOTIFY KILL
                    //await _killService.NotifyKill((PacketPlayerKillNotify)l2rPacket);

                }
                else if (l2rPacket is PacketClanMemberKillNotify)
                {
                    //NOTIFY KILL
                    await _killService.NotifyKill((PacketClanMemberKillNotify)l2rPacket);

                }
                else if (l2rPacket is PacketChatGuildListReadResult && _config["clanchat:enabled"] == "true")
                {
                    //NOTIFY CLAN CHAT
                    await UtilService.NotifyClanChat((PacketChatGuildListReadResult)l2rPacket);
                }
                else if (l2rPacket is PacketClanMemberListReadResult)
                {
                    //NOTIFY CLAN INFO
                    await _playerService.NotifyClanMembersAsync((PacketClanMemberListReadResult)l2rPacket);
                }
                else if (l2rPacket is PacketClanInfoReadResult)
                {
                    //NOTIFY CLAN INFO
                    await _playerService.NotifyClanInfoAsync((PacketClanInfoReadResult)l2rPacket);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Process packet: " + ex.ToString());
            }
        }

        
    }
}