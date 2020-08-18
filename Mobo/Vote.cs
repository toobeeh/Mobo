using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Mobo
{
    class Vote
    {
        public int Reactions { get; private set; }
        public DiscordMessage Message { get; private set; }
        public DiscordChannel Channel { get; private set; }

        private DateTime voteTime;

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
}
