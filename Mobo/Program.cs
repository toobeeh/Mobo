using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System.IO;

namespace Mobo
{
    class Program
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Awwww! You woke Mobo :o");

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = args[0],
                TokenType = TokenType.Bot
            });
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "mobo:" },
                DmHelp = false,
                IgnoreExtraArguments = true
            });
            Client.GuildCreated += onjoin;
            Commands.RegisterCommands<Commands>();
            await Client.ConnectAsync();

            Console.WriteLine("Mobo is ready to move!!");

            await Task.Delay(-1);
        }

        private static async Task onjoin(GuildCreateEventArgs e)
        {
            await e.Guild.GetDefaultChannel().SendMessageAsync("Heya!\nCheck out the command *mobo:manual* ");
        }
    }
}
