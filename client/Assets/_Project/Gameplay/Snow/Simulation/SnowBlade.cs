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

        public static SnowBlade Ahead(Vector2 position, Vector2 forward, SnowSettings settings)
        {
            var facing = forward.normalized;
            return new SnowBlade(position + facing * settings.BladeForwardOffset, facing, settings.BladeWidth, settings.BladeDepth);
        }

        public Vector2 Centre { get; }

        public Vector2 Forward { get; }

        public float Width { get; }

        public float Depth { get; }
    }
}
