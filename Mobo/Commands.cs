﻿using System;
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
        [RequireGuild()]
        public async Task Manual(CommandContext context)
        {
            // just give the user some introduction
            string msg = "**Hi!** \nI'm Mobo, your move bot - cute, huh? :3\nMy job is to move offtopic conversations into the right channel.\nTo do so, use following commands:\n\n";
            msg += "mobo:move `limit` `#channel`\nThis instant-moves the chat and is admin-only.\nLimit sets how many messages you want to show in the notification.\nChannel sets the channel where the conversation should continue.\n\nExample:\n`mobo:move 10 #main`\n";
            msg += "\nmobo:vote `#channel`\nThis creates a vote message to move the chat.\nChannel sets the target channel.\nIf at least 3 members react, the chat will be moved.\n";
            msg += "\n Also try mobo:say `something`";
            await context.RespondAsync(msg);
        }

        [Command("move")]
        [Description("Move a conversation into another channel")]
        [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        [RequireGuild()]
        public async Task Move(CommandContext context, int limit, DiscordChannel target)
        {
            await context.Message.DeleteAsync();
            await Program.MoveChat(context.Guild, context.Channel, limit, target);     
        }

        [Command("vote")]
        [Description("Vote to move the chat.")]
        [RequireGuild()]
        public async Task Warn(CommandContext context, DiscordChannel target)
        {
            // create warning message with reaction
            // reacting will trigger onreaction in program.cs
            await context.Message.DeleteAsync();
            var warning = await context.Channel.SendMessageAsync("**Watch out!**\nMaybe " + target.Mention + " fits better?\n\n*React with :twisted_rightwards_arrows: within 5 mins to move the chat.*\n*4 reactions are needed.*");
            await warning.CreateReactionAsync(DiscordEmoji.FromName(Program.Client, ":twisted_rightwards_arrows:"));
            Program.MoveVotes.Add(new Vote(warning, target));
        }

        [Command("say")]
        [Description("Say something.")]
        public async Task Say(CommandContext context, params string[] say)
        {
            //foreach(DiscordRole role in context.Message.MentionedRoles)
            //{
            //    context.Message.Content.Replace($"@{role.Name}", $"`@{role.Name}`");
            //}
            // echo something and delete command
            await context.Message.DeleteAsync();
            string response = "";
            say.ToList().ForEach((p) => response += p + " ");
            var vote = await context.Channel.SendMessageAsync(context.Message.Content.Replace("mobo:say","").Trim());
            Program.ExposeVotes.Add(new ExposeVote(vote, context.Channel, context.Member));
        }

        [Command("pass")]
        [Description("Say something to another channel.")]
        public async Task Pass(CommandContext context, DiscordChannel target, params string[] say)
        {
            // echo something to another channel and delete command
            await context.Message.DeleteAsync();
            string response = "";
            say.ToList().ForEach((p) => response += p + " ");
            await target.SendMessageAsync(response);
        }

    }
}