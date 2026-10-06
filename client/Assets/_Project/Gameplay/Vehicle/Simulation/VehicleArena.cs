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

        public bool Contains(Vector2 point)
        {
            for (var i = 0; i < _boxes.Count; i++)
            {
                var offset = point - _boxes[i].Center;
                if (Mathf.Abs(offset.x) <= _boxes[i].HalfExtents.x && Mathf.Abs(offset.y) <= _boxes[i].HalfExtents.y)
                {
                    return true;
                }
            }

            for (var i = 0; i < _circles.Count; i++)
            {
                if ((point - _circles[i].Center).sqrMagnitude <= _circles[i].Radius * _circles[i].Radius)
                {
                    return true;
                }
            }

            return false;
        }

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
