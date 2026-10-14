using Fusion;

namespace PlowParty.Meta.Party.Network
{
    public struct PartySeat : INetworkStruct
    {
        public PlayerRef Player { get; set; }

        public NetworkString<_16> Nickname { get; set; }

        public NetworkBool IsReady { get; set; }

        public byte Slot { get; set; }

        public byte Color { get; set; }

        public byte Critter { get; set; }
    }
}
