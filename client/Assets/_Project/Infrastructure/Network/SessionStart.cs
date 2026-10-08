using System.Collections.Generic;
using Fusion;

namespace PlowParty.Infrastructure.Network
{
    public sealed class SessionStart
    {
        public GameMode Mode { get; set; } = GameMode.AutoHostOrClient;

        public string SessionName { get; set; }

        public Dictionary<string, SessionProperty> Properties { get; set; }

        public byte[] ConnectionToken { get; set; }

        public int MaxPlayers { get; set; }

        public bool IncludeActiveScene { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
