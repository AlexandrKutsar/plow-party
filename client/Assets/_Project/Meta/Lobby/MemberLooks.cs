using PlowParty.Meta.Party;
using PlowParty.Meta.Party.Simulation;
using UnityEngine;

namespace PlowParty.Meta.Lobby
{
    public sealed class MemberLooks
    {
        public const int SoloId = PartyState.SoloId;

        private readonly PartyService _party;
        private readonly IParticipantPalette _palette;

        public MemberLooks(PartyService party, IParticipantPalette palette)
        {
            _party = party;
            _palette = palette;
        }

        public MemberLook LookOf(int memberId)
        {
            return _party.Party.TryGetMember(memberId, out var member)
                ? new MemberLook(_palette.ColorOf(member.Color), member.Critter)
                : new MemberLook(Color.white, 0);
        }

        public Color ColorOf(int color)
        {
            return _palette.ColorOf(color);
        }
    }
}
