using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Config
{
    [CreateAssetMenu(menuName = "Plow Party/Snow Config", fileName = nameof(SnowConfig))]
    public sealed class SnowConfig : ScriptableObject
    {
        [SerializeField] private Vector2 _origin = new Vector2(-15f, -15f);
        [SerializeField] private Vector2 _size = new Vector2(30f, 30f);
        [SerializeField, Min(0.01f)] private float _cellSize = 0.5f;
        [SerializeField, Range(1, SnowGrid.MaxDepth - 1)] private int _fullDepth = 3;
        [SerializeField, Min(0f)] private float _regrowthDelay = 6f;
        [SerializeField, Min(0f)] private float _regrowthStep = 3f;
        [SerializeField, Min(1)] private int _pileStepsPerScrape = 1;
        [SerializeField, Range(0f, 1f)] private float _pileSpeedPenalty = 0.4f;
        [SerializeField] private float[] _blizzardTimes = { 45f, 90f, 135f };
        [SerializeField, Min(0f)] private float _blizzardDuration = 2.5f;
        [SerializeField, Min(0)] private int _blizzardPilesPerWave = 3;
        [SerializeField, Min(0)] private int _blizzardPileSteps = 150;
        [SerializeField, Min(0.01f)] private float _bladeWidth = 1.08f;
        [SerializeField, Min(0.01f)] private float _bladeDepth = 0.5f;
        [SerializeField, Min(0f)] private float _bladeForwardOffset = 0.85f;
        [SerializeField, Min(0f)] private float _preClearDuration = 0.4f;
        [SerializeField, Range(1, 4)] private int _verticesPerCell = 2;
        [SerializeField, Min(0f)] private float _snowHeight = 0.15f;
        [SerializeField, Min(0f)] private float _pileHeight = 0.9f;
        [SerializeField, Min(0f)] private float _lowerTime = 0.05f;
        [SerializeField, Min(0f)] private float _raiseTime = 0.35f;

        public float PreClearDuration => _preClearDuration;

        public int VerticesPerCell => _verticesPerCell;

        public float SnowHeight => _snowHeight;

        public float PileHeight => _pileHeight;

        public float LowerTime => _lowerTime;

        public float RaiseTime => _raiseTime;

        public SnowSettings ToSettings()
        {
            return new SnowSettings
            {
                Origin = _origin,
                Size = _size,
                CellSize = _cellSize,
                FullDepth = _fullDepth,
                RegrowthDelay = _regrowthDelay,
                RegrowthStep = _regrowthStep,
                PileStepsPerScrape = _pileStepsPerScrape,
                PileSpeedPenalty = _pileSpeedPenalty,
                BlizzardTimes = (float[])_blizzardTimes.Clone(),
                BlizzardDuration = _blizzardDuration,
                BlizzardPilesPerWave = _blizzardPilesPerWave,
                BlizzardPileSteps = _blizzardPileSteps,
                BladeWidth = _bladeWidth,
                BladeDepth = _bladeDepth,
                BladeForwardOffset = _bladeForwardOffset,
            };
        }
    }
}
