using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    public readonly struct SnowBlade
    {
        public SnowBlade(Vector2 centre, Vector2 forward, float width, float depth)
        {
            Centre = centre;
            Forward = forward;
            Width = width;
            Depth = depth;
        }

        public Vector2 Centre { get; }

        public Vector2 Forward { get; }

        public float Width { get; }

        public float Depth { get; }
    }
}
