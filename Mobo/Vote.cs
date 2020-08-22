using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Mobo
{
    public class Vote
    {
        public int Reactions { get; internal set; }
        public DiscordMessage Message { get; internal set; }
        public DiscordChannel Channel { get; internal set; }

        internal DateTime voteTime;

        public Vote(DiscordMessage voteMessage, DiscordChannel channel)
        {
            Reactions = 0;
            Message = voteMessage;
            Channel = channel;
            voteTime = DateTime.Now;
        }

        public bool AddVote()
        {
            if ((DateTime.Now - voteTime).TotalMinutes > 5) return false;
            Reactions++;
            return true;
        }
    }

    public class ExposeVote : Vote
    {
        public DiscordMember Creator { get; internal set; }
        public ExposeVote(DiscordMessage voteMessage, DiscordChannel channel, DiscordMember member) : base(voteMessage, channel)
        {
            Creator = member;
        }

        public bool AddVote(DiscordMember member)
        {
            if ((member.PermissionsIn(Channel).HasPermission(Permissions.Administrator)||member.Id == 334048043638849536) && (DateTime.Now - voteTime).TotalMinutes < 15)
            {
                Reactions++;
                return true;
            }
            else return false;
        }
    }
}
