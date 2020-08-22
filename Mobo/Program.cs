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
        public static List<ExposeVote> ExposeVotes { get; set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Awwww! You woke Mobo :o");
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = args[0], // get token from console argument
                TokenType = TokenType.Bot
            });
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "mobo:" },
                DmHelp = false,
                IgnoreExtraArguments = true
            });
            Client.GuildCreated += onjoin; // welcome message
            Client.MessageReactionAdded += onreaction; // reaction handler for votes
            Client.MessageCreated += async (MessageCreateEventArgs e) => { if (e.MentionedUsers.Contains(Client.CurrentUser)) await onmention(e); };
            Commands.RegisterCommands<Commands>();
            await Client.ConnectAsync(new DiscordActivity("mobo:vote #channel"));
            MoveVotes = new List<Vote>();
            ExposeVotes = new List<ExposeVote>();
            Console.WriteLine("Mobo is ready to move!!");

            await Task.Delay(-1);
        }

        private static async Task onjoin(GuildCreateEventArgs e)
        {
            // send welcome message
            await e.Guild.GetDefaultChannel().SendMessageAsync("Heya!\nCheck out the command *mobo:manual* or make me say something with *mobo:say* ");
        }
        private static async Task onreaction(MessageReactionAddEventArgs e)
        {
            // if reaction was made on a vote message and reaction is a valid vote emoji
            if (MoveVotes.Exists(v => v.Message.Equals(e.Message))
                && e.Emoji == DiscordEmoji.FromName(Client, ":twisted_rightwards_arrows:")) await HandleVoteReaction(e);
            // if reaction was made on a say message
            else if (ExposeVotes.Exists(v => v.Message.Equals(e.Message))) await HandleExposeVoteReaction(e);
        }
        private static async Task onmention(MessageCreateEventArgs e)
        {
            await e.Channel.SendMessageAsync(e.Author.Mention + " Pings are ***eeeevil!*** <a:l23:721872920347017216>");
        }

        public static async Task HandleVoteReaction(MessageReactionAddEventArgs e)
        {
            // get vote message
            var vote = MoveVotes.Find(v => v.Message.Equals(e.Message));
            // try increment votes - false if vote timeout has expired
            if (vote.AddVote())
            {
                if (vote.Reactions >= 4)
                {
                    // if move vote count is reached, move the chat
                    await MoveChat(e.Guild, e.Channel, 10, vote.Channel);
                    MoveVotes.Remove(vote);
                }
            }
            else MoveVotes.Remove(vote);
        }

        public static async Task HandleExposeVoteReaction(MessageReactionAddEventArgs e)
        {
            // get vote message
            var vote = ExposeVotes.Find(v => v.Message.Equals(e.Message));
            if (vote.AddVote(await e.Guild.GetMemberAsync(e.User.Id)))
            {
                var embedContent = new DiscordEmbedBuilder
                {
                    Title = "**Uh oh!** Exposed!!!",
                    Description = "**" + vote.Creator.Username + " made me write this:**"
                };
                embedContent.Color = new DiscordColor("#ff1744");
                embedContent.AddField("<a:typing:745068588909592737>", vote.Message.Content);
                await e.Channel.SendMessageAsync(embed: embedContent);
            }
            else await e.Message.DeleteReactionAsync(e.Emoji, e.User);
        }

        public static async Task MoveChat(DiscordGuild guild, DiscordChannel channel, int limit, DiscordChannel target)
        {
            var lastMessages = await channel.GetMessagesAsync(limit);
            // check if everyone has access
            var permSend = true;
            var channelOverwrites = channel.PermissionOverwrites.ToList();
            channelOverwrites.ForEach(o => {
                if (o.Id == guild.EveryoneRole.Id) permSend = !o.Denied.HasPermission(Permissions.SendMessages) && !o.Denied.HasPermission(Permissions.AccessChannels);
            });
            await channel.SendMessageAsync("This conversation belongs to " + target.Mention + " and was moved there! \n" + (permSend ? channel.Mention + " is now closed for 2 mins to cool things down.\n" : "") + "\n*Remember to avoid offtopic chats!*");
            if(permSend) await channel.AddOverwriteAsync(guild.EveryoneRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.SendMessages, "Temp timeout start");
            // build embed with recent chat
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
            if (permSend)
            {
                await Task.Delay(120000);
                // release channel
                await channel.AddOverwriteAsync(guild.EveryoneRole, DSharpPlus.Permissions.SendMessages, DSharpPlus.Permissions.None, "Temp timeout stop");
            }
        }
    }
}
