using System.Collections.Generic;

namespace PlowParty.Meta.Session.Simulation
{
    public static class LobbyParties
    {
        private const char Separator = ',';

        public static string Format(IEnumerable<string> partyCodes)
        {
            return string.Join(Separator.ToString(), partyCodes);
        }

        public static bool Contains(string parties, string partyCode)
        {
            if (string.IsNullOrEmpty(parties) || string.IsNullOrEmpty(partyCode))
            {
                return false;
            }

            foreach (var code in parties.Split(Separator))
            {
                if (code == partyCode)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryFindLobbyOf(IReadOnlyList<LobbyListing> listings, string partyCode, out LobbyListing lobby)
        {
            foreach (var listing in listings)
            {
                if (Contains(listing.Parties, partyCode))
                {
                    lobby = listing;
                    return true;
                }
            }

            lobby = default;
            return false;
        }
    }
}
