using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    public sealed class SnowSettings
    {
        public Vector2 Origin { get; set; }

        public Vector2 Size { get; set; }

        public float CellSize { get; set; }

        public int FullDepth { get; set; }

        public float RegrowthInterval { get; set; }

        public float RegrowthChance { get; set; }

        public float[] BlizzardTimes { get; set; }

        public float BlizzardDuration { get; set; }
    }
}
