using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Snow Config", fileName = nameof(SnowConfig))]
    public sealed class SnowConfig : ScriptableObject
    {
        [SerializeField] private Vector2 _origin = new Vector2(-15f, -15f);
        [SerializeField] private Vector2 _size = new Vector2(30f, 30f);
        [SerializeField] private float _cellSize = 0.5f;
        [SerializeField] private int _fullDepth = 3;
        [SerializeField] private float _regrowthInterval = 0.5f;
        [SerializeField] private float _regrowthChance = 0.05f;
        [SerializeField] private float[] _blizzardTimes = { 45f, 90f, 135f };
        [SerializeField] private float _blizzardDuration = 2.5f;
        [SerializeField] private float _bladeWidth = 1.08f;
        [SerializeField] private float _bladeDepth = 0.5f;
        [SerializeField] private float _bladeForwardOffset = 0.85f;
        [SerializeField] private float _preClearDuration = 0.4f;

        public float PreClearDuration => _preClearDuration;

        public SnowSettings ToSettings()
        {
            return new SnowSettings
            {
                Origin = _origin,
                Size = _size,
                CellSize = _cellSize,
                FullDepth = _fullDepth,
                RegrowthInterval = _regrowthInterval,
                RegrowthChance = _regrowthChance,
                BlizzardTimes = (float[])_blizzardTimes.Clone(),
                BlizzardDuration = _blizzardDuration,
                BladeWidth = _bladeWidth,
                BladeDepth = _bladeDepth,
                BladeForwardOffset = _bladeForwardOffset,
            };
        }
    }
}
