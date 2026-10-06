using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.DropOff.Simulation;
using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Network
{
    public sealed class DropOffSnowFreeArea : ISnowFreeArea
    {
        private readonly DropOffZone _zone;
        private readonly DropOffRules _rules;

        public DropOffSnowFreeArea(DropOffZone zone, DropOffConfig config, BucketConfig bucketConfig)
        {
            _zone = zone;
            _rules = new DropOffRules(config.ToSettings(), bucketConfig.ToSettings());
        }

        public bool Contains(Vector2 point)
        {
            return _rules.IsSnowFree(_zone.Centre, point);
        }
    }
}
