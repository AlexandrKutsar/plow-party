using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    public sealed class SnowGrid
    {
        public const int CellsPerWord = 32 / BitsPerCell;
        public const int MaxDepth = (1 << BitsPerCell) - 1;

        private const int BitsPerCell = 4;
        private const int DepthMask = MaxDepth;
        private const int BlizzardSalt = -1;
        private const int BlizzardPileSalt = -2;
        private const uint BlizzardSideCount = 4;

        private readonly SnowSettings _settings;
        private readonly int[] _words;
        private readonly bool[] _masked;
        private readonly int _seed;
        private readonly float[] _blizzardProgress;
        private int _regrowthStepsApplied;

        public SnowGrid(SnowSettings settings, VehicleArena arena, int seed)
        {
            _settings = settings;
            _seed = seed;
            _blizzardProgress = new float[settings.BlizzardTimes.Length];
            Width = Mathf.RoundToInt(settings.Size.x / settings.CellSize);
            Height = Mathf.RoundToInt(settings.Size.y / settings.CellSize);
            _words = new int[(Width * Height + CellsPerWord - 1) / CellsPerWord];
            _masked = new bool[Width * Height];
            for (var index = 0; index < Width * Height; index++)
            {
                _masked[index] = arena.Contains(CellCentre(index));
                SetDepth(index, _masked[index] ? 0 : settings.FullDepth);
            }
        }

        public int Width { get; }

        public int Height { get; }

        public int WordCount => _words.Length;

        public int GetDepth(int x, int y)
        {
            return GetDepth(IndexOf(x, y));
        }

        public int GetDepth(int cellIndex)
        {
            var shift = cellIndex % CellsPerWord * BitsPerCell;
            return (_words[cellIndex / CellsPerWord] >> shift) & DepthMask;
        }

        public int GetWord(int index)
        {
            return _words[index];
        }

        public void SetWord(int index, int value)
        {
            _words[index] = value;
        }

        public int Scrape(SnowBlade blade, int room)
        {
            var forward = blade.Forward.normalized;
            var right = new Vector2(forward.y, -forward.x);
            var halfWidth = blade.Width * 0.5f;
            var halfDepth = blade.Depth * 0.5f;
            var reach = new Vector2(
                Mathf.Abs(right.x) * halfWidth + Mathf.Abs(forward.x) * halfDepth,
                Mathf.Abs(right.y) * halfWidth + Mathf.Abs(forward.y) * halfDepth);
            var min = CellFloor(blade.Centre - reach);
            var max = CellFloor(blade.Centre + reach);
            var taken = 0;
            for (var y = Mathf.Max(min.y, 0); y <= Mathf.Min(max.y, Height - 1); y++)
            {
                for (var x = Mathf.Max(min.x, 0); x <= Mathf.Min(max.x, Width - 1) && taken < room; x++)
                {
                    var index = IndexOf(x, y);
                    var offset = CellCentre(index) - blade.Centre;
                    if (Mathf.Abs(Vector2.Dot(offset, right)) > halfWidth || Mathf.Abs(Vector2.Dot(offset, forward)) > halfDepth)
                    {
                        continue;
                    }

                    var depth = GetDepth(index);
                    var removed = Mathf.Min(depth, room - taken);
                    SetDepth(index, depth - removed);
                    taken += removed;
                }
            }

            return taken;
        }

        public int Spill(Vector2 point, int steps)
        {
            var centre = CellFloor(point);
            var farthestRing = Mathf.Max(
                Mathf.Max(centre.x, Width - 1 - centre.x),
                Mathf.Max(centre.y, Height - 1 - centre.y));
            var placed = 0;
            for (var ring = 0; ring <= farthestRing && placed < steps; ring++)
            {
                for (var y = centre.y - ring; y <= centre.y + ring && placed < steps; y++)
                {
                    var onEdgeRow = y == centre.y - ring || y == centre.y + ring;
                    var stride = onEdgeRow || ring == 0 ? 1 : 2 * ring;
                    for (var x = centre.x - ring; x <= centre.x + ring && placed < steps; x += stride)
                    {
                        placed += RaiseTowardsMax(x, y, steps - placed);
                    }
                }
            }

            return placed;
        }

        public void Tick(float elapsedPlayingTime)
        {
            ApplyRegrowthDueBy(elapsedPlayingTime);
            for (var wave = 0; wave < _blizzardProgress.Length; wave++)
            {
                var progress = BlizzardProgress(wave, elapsedPlayingTime);
                if (progress > _blizzardProgress[wave])
                {
                    SweepBlizzardFront(wave, _blizzardProgress[wave], progress);
                    _blizzardProgress[wave] = progress;
                }
            }
        }

        private void ApplyRegrowthDueBy(float elapsedPlayingTime)
        {
            if (_settings.RegrowthInterval <= 0f)
            {
                return;
            }

            var regrowthStepsDue = Mathf.FloorToInt(elapsedPlayingTime / _settings.RegrowthInterval);
            while (_regrowthStepsApplied < regrowthStepsDue)
            {
                _regrowthStepsApplied++;
                ApplyRegrowthStep(_regrowthStepsApplied);
            }
        }

        private float BlizzardProgress(int wave, float elapsedPlayingTime)
        {
            var sinceStart = elapsedPlayingTime - _settings.BlizzardTimes[wave];
            if (_settings.BlizzardDuration <= 0f)
            {
                return sinceStart >= 0f ? 1f : 0f;
            }

            return Mathf.Clamp01(sinceStart / _settings.BlizzardDuration);
        }

        private void SweepBlizzardFront(int wave, float fromProgress, float toProgress)
        {
            var side = BlizzardSideOf(wave);
            for (var index = 0; index < _masked.Length; index++)
            {
                var position = PositionFrom(side, index);
                if (_masked[index] || position <= fromProgress || position > toProgress)
                {
                    continue;
                }

                if (GetDepth(index) < _settings.FullDepth)
                {
                    SetDepth(index, _settings.FullDepth);
                }
            }

            DropBlizzardPilesPassed(wave, side, fromProgress, toProgress);
        }

        private void DropBlizzardPilesPassed(int wave, BlizzardSide side, float fromProgress, float toProgress)
        {
            for (var pile = 0; pile < _settings.BlizzardPilesPerWave; pile++)
            {
                var cell = BlizzardPileCell(wave, pile);
                if (cell < 0)
                {
                    return;
                }

                var position = PositionFrom(side, cell);
                if (position > fromProgress && position <= toProgress)
                {
                    Spill(CellCentre(cell), _settings.BlizzardPileSteps);
                }
            }
        }

        private int BlizzardPileCell(int wave, int pile)
        {
            var start = (int)(SnowHash.Mix(_seed, wave, BlizzardPileSalt - pile) % (uint)_masked.Length);
            for (var offset = 0; offset < _masked.Length; offset++)
            {
                var cell = (start + offset) % _masked.Length;
                if (!_masked[cell])
                {
                    return cell;
                }
            }

            return -1;
        }

        private BlizzardSide BlizzardSideOf(int wave)
        {
            return (BlizzardSide)(SnowHash.Mix(_seed, wave, BlizzardSalt) % BlizzardSideCount);
        }

        private float PositionFrom(BlizzardSide side, int index)
        {
            var cell = CellOf(index);
            var x = (cell.x + 0.5f) / Width;
            var y = (cell.y + 0.5f) / Height;
            switch (side)
            {
                case BlizzardSide.West:
                    return x;
                case BlizzardSide.East:
                    return 1f - x;
                case BlizzardSide.South:
                    return y;
                default:
                    return 1f - y;
            }
        }

        private void ApplyRegrowthStep(int step)
        {
            for (var index = 0; index < _masked.Length; index++)
            {
                if (_masked[index])
                {
                    continue;
                }

                var depth = GetDepth(index);
                if (depth < _settings.FullDepth && SnowHash.Chance(_seed, index, step) < _settings.RegrowthChance)
                {
                    SetDepth(index, depth + 1);
                }
            }
        }

        private int RaiseTowardsMax(int x, int y, int steps)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return 0;
            }

            var index = IndexOf(x, y);
            if (_masked[index])
            {
                return 0;
            }

            var depth = GetDepth(index);
            var added = Mathf.Min(MaxDepth - depth, steps);
            SetDepth(index, depth + added);
            return added;
        }

        private Vector2Int CellFloor(Vector2 point)
        {
            var local = (point - _settings.Origin) / _settings.CellSize;
            return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.y));
        }

        private Vector2 CellCentre(int index)
        {
            var cell = CellOf(index);
            return _settings.Origin + new Vector2(cell.x + 0.5f, cell.y + 0.5f) * _settings.CellSize;
        }

        private int IndexOf(int x, int y)
        {
            return y * Width + x;
        }

        private Vector2Int CellOf(int index)
        {
            return new Vector2Int(index % Width, index / Width);
        }

        private void SetDepth(int index, int depth)
        {
            var word = index / CellsPerWord;
            var shift = index % CellsPerWord * BitsPerCell;
            _words[word] = (_words[word] & ~(DepthMask << shift)) | (depth << shift);
        }
    }
}
