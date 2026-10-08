using Fusion;

namespace PlowParty.Gameplay.Participants.Network
{
    public struct ParticipantSeat : INetworkStruct
    {
        public NetworkString<_16> Nickname { get; set; }

        public byte Species { get; set; }

        public byte Color { get; set; }

        public NetworkBool IsBot { get; set; }

        public NetworkBool IsSeated { get; set; }
    }
}
