using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct CircleObstacle
    {
        public CircleObstacle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public Vector2 Center { get; }

        public float Radius { get; }
    }
}
