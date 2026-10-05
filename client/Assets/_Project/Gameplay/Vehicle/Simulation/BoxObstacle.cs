using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct BoxObstacle
    {
        public BoxObstacle(Vector2 center, Vector2 halfExtents)
        {
            Center = center;
            HalfExtents = halfExtents;
        }

        public Vector2 Center { get; }

        public Vector2 HalfExtents { get; }
    }
}
