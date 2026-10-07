using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct ArenaVolume
    {
        public ArenaVolume(Rect area, float groundHeight, float topHeight)
        {
            Area = area;
            GroundHeight = groundHeight;
            TopHeight = topHeight;
        }

        public Rect Area { get; }

        public float GroundHeight { get; }

        public float TopHeight { get; }

        public static ArenaVolume FromBounds(Bounds bounds)
        {
            return new ArenaVolume(
                Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z),
                bounds.min.y,
                bounds.max.y);
        }
    }
}
