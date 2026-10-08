using System.Collections.Generic;

namespace PlowParty.Meta.Session.Simulation
{
    public static class LobbyPick
    {
        private const float MillisecondsPerSecond = 1000f;

        public static IReadOnlyList<LobbyListing> Candidates(IReadOnlyList<LobbyListing> listings, int partySize, long nowMilliseconds, float minSecondsBeforeStart)
        {
            var candidates = new List<LobbyListing>();
            foreach (var listing in listings)
            {
                if (Fits(listing, partySize, nowMilliseconds, minSecondsBeforeStart))
                {
                    candidates.Add(listing);
                }
            }

            candidates.Sort(Compare);
            return candidates;
        }

        public static bool TryMergeTarget(IReadOnlyList<LobbyListing> listings, LobbyListing own, int partySize, long nowMilliseconds, float minSecondsBeforeStart, out LobbyListing target)
        {
            target = default;
            if (own.Players > partySize)
            {
                return false;
            }

            foreach (var listing in Candidates(listings, partySize, nowMilliseconds, minSecondsBeforeStart))
            {
                if (listing.Name != own.Name && listing.IsOlderThan(own))
                {
                    target = listing;
                    return true;
                }
            }

            return false;
        }

        private static bool Fits(LobbyListing listing, int partySize, long nowMilliseconds, float minSecondsBeforeStart)
        {
            var secondsBeforeStart = (listing.StartsAtMilliseconds - nowMilliseconds) / MillisecondsPerSecond;
            return listing.StartsAtMilliseconds > 0 && listing.FreeSlots >= partySize && secondsBeforeStart >= minSecondsBeforeStart;
        }

        private static int Compare(LobbyListing a, LobbyListing b)
        {
            if (a.Players != b.Players)
            {
                return b.Players.CompareTo(a.Players);
            }

            if (a.StartsAtMilliseconds != b.StartsAtMilliseconds)
            {
                return a.StartsAtMilliseconds.CompareTo(b.StartsAtMilliseconds);
            }

            return string.CompareOrdinal(a.Name, b.Name);
        }
    }
}
