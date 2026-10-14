using UnityEngine;

namespace PlowParty.Meta.Party
{
    public interface IParticipantPalette
    {
        int ColorCount { get; }

        int CritterCount { get; }

        Color ColorOf(int color);
    }
}
