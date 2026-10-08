namespace PlowParty.Meta.Session.Simulation
{
    public readonly struct LobbyListing
    {
        public LobbyListing(string name, int players, int maxPlayers, long startsAtMilliseconds, long openedAtMilliseconds)
        {
            Name = name ?? string.Empty;
            Players = players;
            MaxPlayers = maxPlayers;
            StartsAtMilliseconds = startsAtMilliseconds;
            OpenedAtMilliseconds = openedAtMilliseconds;
        }

        public string Name { get; }

        public int Players { get; }

        public int MaxPlayers { get; }

        public long StartsAtMilliseconds { get; }

        public long OpenedAtMilliseconds { get; }

        public int FreeSlots => MaxPlayers - Players;

        public bool IsOlderThan(LobbyListing other)
        {
            if (OpenedAtMilliseconds != other.OpenedAtMilliseconds)
            {
                return OpenedAtMilliseconds < other.OpenedAtMilliseconds;
            }

            return string.CompareOrdinal(Name, other.Name) < 0;
        }
    }
}
