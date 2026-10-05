using System.Collections.Generic;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class VehicleArena
    {
        private readonly List<BoxObstacle> _boxes = new List<BoxObstacle>();
        private readonly List<CircleObstacle> _circles = new List<CircleObstacle>();

        public static VehicleArena Create()
        {
            return new VehicleArena();
        }

        public IReadOnlyList<BoxObstacle> Boxes => _boxes;

        public IReadOnlyList<CircleObstacle> Circles => _circles;

        public VehicleArena AddBox(Vector2 center, Vector2 halfExtents)
        {
            _boxes.Add(new BoxObstacle(center, halfExtents));
            return this;
        }

        public VehicleArena AddCircle(Vector2 center, float radius)
        {
            _circles.Add(new CircleObstacle(center, radius));
            return this;
        }
    }
}
