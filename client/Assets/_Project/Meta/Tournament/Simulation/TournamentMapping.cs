using System.Collections.Generic;
using PlowParty.Meta.Tournament.Api;

namespace PlowParty.Meta.Tournament.Simulation
{
    public static class TournamentMapping
    {
        public static TournamentBoard ToBoard(LeaderboardResponse top, LeaderboardResponse aroundMe, MedalsResponse medals, string myAccountId)
        {
            var rows = new List<Standing>();
            var lastRank = 0;
            foreach (var entry in top.Entries)
            {
                rows.Add(ToStanding(entry, myAccountId));
                lastRank = entry.Rank;
            }

            foreach (var entry in aroundMe.Entries)
            {
                if (entry.Rank <= lastRank)
                {
                    continue;
                }

                if (entry.Rank > lastRank + 1)
                {
                    rows.Add(Standing.Gap);
                }

                rows.Add(ToStanding(entry, myAccountId));
                lastRank = entry.Rank;
            }

            var myRank = aroundMe.Me != null ? aroundMe.Me.Rank : 0;
            return new TournamentBoard(rows, myRank, top.Players, MedalOf(medals, myAccountId));
        }

        private static Standing ToStanding(StandingResponse entry, string myAccountId)
        {
            return new Standing(entry.Rank, entry.Nickname, entry.Score, entry.AccountId == myAccountId);
        }

        private static Medal MedalOf(MedalsResponse medals, string myAccountId)
        {
            foreach (var entry in medals.Medals)
            {
                if (entry.AccountId == myAccountId)
                {
                    return Parse(entry.Medal);
                }
            }

            return Medal.None;
        }

        private static Medal Parse(string medal)
        {
            switch (medal)
            {
                case "gold":
                    return Medal.Gold;
                case "silver":
                    return Medal.Silver;
                case "bronze":
                    return Medal.Bronze;
                default:
                    return Medal.None;
            }
        }
    }
}
