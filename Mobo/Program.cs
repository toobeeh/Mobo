using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Mobo
{
    class Program
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        public static List<Vote> MoveVotes { get; set; }

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
            Client.MessageReactionAdded += onreaction;

            Commands.RegisterCommands<Commands>();
            await Client.ConnectAsync(new DiscordActivity("mobo:vote #channel"));
            MoveVotes = new List<Vote>();

            Console.WriteLine("Mobo is ready to move!!");

            await Task.Delay(-1);
        }

        private static async Task onjoin(GuildCreateEventArgs e)
        {
            await e.Guild.GetDefaultChannel().SendMessageAsync("Heya!\nCheck out the command *mobo:manual* or make me say something with *mobo:say* ");
        }
        private static async Task onreaction(MessageReactionAddEventArgs e)
        {
            if(MoveVotes.Exists(v => v.Message.Equals(e.Message)) && e.Emoji == DiscordEmoji.FromName(Client, ":twisted_rightwards_arrows:")){
                var vote = MoveVotes.Find(v => v.Message.Equals(e.Message));
                if (vote.AddVote())
                {
                    if (vote.Reactions >= 4)
                    {
                        await MoveChat(e.Guild, e.Channel, 10, vote.Channel);
                        MoveVotes.Remove(vote);
                    }
                }
                else MoveVotes.Remove(vote);
            }
        }

        public static async Task MoveChat(DiscordGuild guild, DiscordChannel channel, int limit, DiscordChannel target)
        {
            var lastMessages = await channel.GetMessagesAsync(limit);
            await channel.SendMessageAsync("This conversation belongs to " + target.Mention + " and was moved there! \n" + channel.Mention + " is now closed for 2 mins to cool things down.\n\n*Remember to avoid offtopic chats!*");
            await channel.AddOverwriteAsync(guild.EveryoneRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.SendMessages, "Temp timeout start");

            var embedChat = new DiscordEmbedBuilder
            {
                Title = "**Moved chat from *#" + channel.Name + "* **",
                Description = "This conversation didn't meet the channel topic and should be continued here.\n\n\n<a:typing:745068588909592737>"
            };
            embedChat.Color = new DiscordColor("#ff1744");
            lastMessages.Reverse().ToList().ForEach(
                (m) => {
                    if(m.Author != Client.CurrentUser) embedChat.AddField("**" + m.Author.Username + "**", m.Content);
                }
            );

            await target.SendMessageAsync(embed: embedChat);
            await Task.Delay(120000);
            await channel.AddOverwriteAsync(guild.EveryoneRole, DSharpPlus.Permissions.SendMessages, DSharpPlus.Permissions.None, "Temp timeout stop");
        }
    }
}
