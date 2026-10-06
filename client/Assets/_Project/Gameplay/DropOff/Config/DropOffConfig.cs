using System;
using PlowParty.Gameplay.DropOff.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Drop-Off Config", fileName = nameof(DropOffConfig))]
    public sealed class DropOffConfig : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _zoneRadius = 3.5f;
        [SerializeField, Min(0f)] private float _snowFreeRadius = 5f;
        [SerializeField, Min(0f)] private float _fullUnloadDuration = 1.5f;
        [SerializeField, Min(0.01f)] private float _baseMultiplier = 1f;
        [SerializeField] private Tier[] _multiplierTiers =
        {
            new Tier(51, 1.5f),
            new Tier(100, 2f),
        };

        public DropOffSettings ToSettings()
        {
            var tiers = new MultiplierTier[_multiplierTiers.Length];
            for (var i = 0; i < tiers.Length; i++)
            {
                tiers[i] = _multiplierTiers[i].ToMultiplierTier();
            }

            return new DropOffSettings
            {
                ZoneRadius = _zoneRadius,
                SnowFreeRadius = _snowFreeRadius,
                FullUnloadDuration = _fullUnloadDuration,
                BaseMultiplier = _baseMultiplier,
                MultiplierTiers = tiers,
            };
        }

        [Serializable]
        private struct Tier
        {
            [SerializeField, Min(1)] private int _minLoad;
            [SerializeField, Min(0.01f)] private float _multiplier;

            public Tier(int minLoad, float multiplier)
            {
                _minLoad = minLoad;
                _multiplier = multiplier;
            }

            public MultiplierTier ToMultiplierTier()
            {
                return new MultiplierTier(_minLoad, _multiplier);
            }
        }
    }
}
