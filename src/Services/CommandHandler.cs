using Discord;
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
            await ScanMessages(msg, context);
        }

        private static async Task ScanMessages(SocketUserMessage msg, SocketCommandContext context)
        {
            if (msg.Content.ToLower().Contains("should i"))
            {
                Random Rnd = new Random();
                var eb = new EmbedBuilder();
                int selection = Rnd.Next(4);

                switch (selection)
                {
                    case 0:
                        eb.ImageUrl = "https://media.giphy.com/media/deDsaGovR3BMiNh45V/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 1:
                        await context.Channel.SendMessageAsync("Do It. Do It. Do It.");
                        break;

                    case 2:
                        eb.ImageUrl = "https://media.giphy.com/media/xtnYGffYPqnm6is6ki/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 3:
                        eb.ImageUrl = "https://media.giphy.com/media/entaMSGWlwnriUkEdr/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 4:
                        eb.ImageUrl = "https://media.giphy.com/media/5bo5M40sbcf8ZIIMxm/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;
                }
            }
            else if (msg.Content.ToLower().Contains(" vic ") || msg.Content.ToLower().Contains(" doc "))
            {
                Random Rnd = new Random();
                var eb = new EmbedBuilder();
                int selection = Rnd.Next(4);

                switch (selection)
                {
                    case 0:
                        eb.ImageUrl = "https://media.giphy.com/media/k7iFqM4WWJcCYPD74u/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 1:
                        eb.ImageUrl = "https://media.giphy.com/media/5gWGpnBMRMSy7oTdl8/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 3:
                        eb.ImageUrl = "https://media.giphy.com/media/kgT4SKnVdOc1ZbtYCu/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 4:
                        eb.ImageUrl = "https://media.giphy.com/media/wZtpAJCNm9KvfHEaPc/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;
                }
            }
            else if (msg.Content.ToLower().Contains(" jeep"))
            {
                Random Rnd = new Random();
                var eb = new EmbedBuilder();
                int selection = Rnd.Next(4);

                switch (selection)
                {
                    case 0:
                        await context.Channel.SendMessageAsync("Beep Beep");
                        break;

                    case 1:
                        eb.ImageUrl = "https://media.giphy.com/media/2m0nyHcVuwL7faEiU2/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 3:
                        eb.ImageUrl = "https://media.giphy.com/media/8lZd54E7AKEgR8vKe7/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 4:
                        eb.ImageUrl = "https://media.giphy.com/media/fxev9Fgq6xj2l2VXYd/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;
                }
            }
            else if (msg.Content.ToLower().Contains(" pzycho") || msg.Content.ToLower().Contains(" tif") || msg.Content.ToLower().Contains(" tiff"))
            {
                Random Rnd = new Random();
                var eb = new EmbedBuilder();
                int selection = Rnd.Next(4);

                switch (selection)
                {
                    case 0:
                        eb.ImageUrl = "https://media.giphy.com/media/2YaJDW5GoEozoSiCSV/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 1:
                        eb.ImageUrl = "https://media.giphy.com/media/41eyam9suuYmQ7Ze0R/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 3:
                        eb.ImageUrl = "https://media.giphy.com/media/7mQJHnBKR8V4Ilepr7/giphy.gif";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;

                    case 4:
                        eb.ImageUrl = "https://giphy.com/gifs/disenchantment-animation-netflix-3LwMFJ140gG8sD43Lr";
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        break;
                }
            }

        }


    }
}