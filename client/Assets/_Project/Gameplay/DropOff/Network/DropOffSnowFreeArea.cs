using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Network
{
    public sealed class DropOffSnowFreeArea : ISnowFreeArea
    {
        private readonly DropOffZone _zone;
        private readonly float _radius;

        public DropOffSnowFreeArea(DropOffZone zone, DropOffConfig config)
        {
            _zone = zone;
            _radius = config.ToSettings().SnowFreeRadius;
        }

        public bool Contains(Vector2 point)
        {
            return (point - _zone.Centre).sqrMagnitude <= _radius * _radius;
        }
    }
}
