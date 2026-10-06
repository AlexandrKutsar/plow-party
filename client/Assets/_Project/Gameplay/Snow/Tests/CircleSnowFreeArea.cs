using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Tests
{
    internal sealed class CircleSnowFreeArea : ISnowFreeArea
    {
        private readonly Vector2 _centre;
        private readonly float _radius;

        public CircleSnowFreeArea(Vector2 centre, float radius)
        {
            _centre = centre;
            _radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            return (point - _centre).sqrMagnitude <= _radius * _radius;
        }
    }
}
