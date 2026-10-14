using UnityEngine;

namespace PlowParty.Meta.Lobby
{
    public readonly struct MemberLook
    {
        public MemberLook(Color color, int critter)
        {
            Color = color;
            Critter = critter;
        }

        public Color Color { get; }

        public int Critter { get; }
    }
}
