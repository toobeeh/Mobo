using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.Interactivity;
using System.Linq;
using System.Runtime;

namespace Mobo
{
    public class Commands : BaseCommandModule
    {
        [Command("manual")]
        [Description("Show the manual for bot usage")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        [RequireGuild()]
        public async Task Manual(CommandContext context)
        {
            string msg = "**Hi!** \nI'm Mobo, your move bot - cute, huh? :3\nMy job is to move offtopic conversations into the right channel.\nTo do so, you will have to call following command:\n\n";
            msg += "*mobo:move* `limit` `@role` `#channel`\nLimit sets how many messages you want to show in the notification.\nChannel sets the channel where the conversation should continue.\n\nExample:\n`mobo:move 10 #main`\n\n";
            msg += "\n\n Try *mobo:warn* too!";
            await context.RespondAsync(msg);
        }

        [Command("move")]
        [Description("Move a conversation into another channel")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        [RequireGuild()]
        public async Task Move(CommandContext context, int limit, DiscordChannel target)
        {

            await context.Message.DeleteAsync();
            var channel = context.Channel;
            var lastMessages = await channel.GetMessagesAsync(limit);
            await context.Channel.SendMessageAsync("This conversation belongs to " + target.Mention + " and was moved there! \n" + context.Channel.Mention + " is now closed for 2 mins to cool things down.\n\n*Remember to avoid offtopic chats!*");
            await channel.AddOverwriteAsync(context.Guild.EveryoneRole, DSharpPlus.Permissions.None, DSharpPlus.Permissions.SendMessages, "Temp timeout start");
            
            var embedChat = new DiscordEmbedBuilder
            {
                Title = "**Moved chat from *#" + context.Channel.Name + "* **",
                Description = "This conversation didn't meet the channel topic and should be continued here.\n\n\n<a:typing:745068588909592737>"
            };
            embedChat.Color = new DiscordColor("#ff1744");
            lastMessages.Reverse().ToList().ForEach(
                (m) => {
                    embedChat.AddField("**" + m.Author.Username + "**", m.Content);
                }
            );

            await target.SendMessageAsync(embed: embedChat);
            await Task.Delay(120000);
            await channel.AddOverwriteAsync(context.Guild.EveryoneRole, DSharpPlus.Permissions.SendMessages, DSharpPlus.Permissions.None, "Temp timeout stop");
        }

        [Command("warn")]
        [Description("Send a soft warning if the chat tends to get out of hand.")]
        [RequireGuild()]
        public async Task Warn(CommandContext context)
        {
            await context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync("**Watch out!**\nMaybe another channel fits better?");

        }

        [Command("say")]
        [Description("Say something.")]
        public async Task Say(CommandContext context)
        {
            string say = context.Message.Content.Replace("mobo:say","").Trim();
            await context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync(say);

        }
    }
}