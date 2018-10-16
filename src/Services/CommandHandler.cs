﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Luci
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private readonly KillListService _killList;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider,
            KillListService killList)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
            _killList = killList;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
            if (msg == null)
            {
                return;
            }

            if (msg.Author.Id == _discord.CurrentUser.Id)
            {
                return;     // Ignore self when checking commands
            }

            SocketCommandContext context = new SocketCommandContext(_discord, msg);     // Create the command context

            int argPos = 0;     // Check if the message has a valid command prefix
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _provider);     // Execute the command

                if (!result.IsSuccess)     // If not successful, reply with the error.
                {
                    await context.Channel.SendMessageAsync(result.ToString());
                }
            }

            if (msg.Content.ToLower().Contains("should i"))
            {
                var eb = new EmbedBuilder() { ImageUrl= "https://media.giphy.com/media/deDsaGovR3BMiNh45V/giphy.gif" };
                await context.Channel.SendMessageAsync("", false, eb);
                
            }
        }
    }
}
