using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    public sealed class SnowGrid
    {
        private const int BitsPerCell = 4;
        private const int CellsPerWord = 32 / BitsPerCell;
        private const int DepthMask = (1 << BitsPerCell) - 1;
        private const int MaxDepth = DepthMask;
        private const int BlizzardSalt = -1;

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
                _masked[index] = IsInsideObstacle(arena, CellCentre(index));
                SetDepth(index, _masked[index] ? 0 : settings.FullDepth);
            }
        }

        public int Width { get; }

        public int Height { get; }

        public int WordCount => _words.Length;

        public int GetDepth(int x, int y)
        {
            return GetDepth(y * Width + x);
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
                    var index = y * Width + x;
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
            var regrowthStepsDue = Mathf.FloorToInt(elapsedPlayingTime / _settings.RegrowthInterval);
            while (_regrowthStepsApplied < regrowthStepsDue)
            {
                _regrowthStepsApplied++;
                ApplyRegrowthStep(_regrowthStepsApplied);
            }

            for (var wave = 0; wave < _blizzardProgress.Length; wave++)
            {
                var progress = Mathf.Clamp01((elapsedPlayingTime - _settings.BlizzardTimes[wave]) / _settings.BlizzardDuration);
                if (progress > _blizzardProgress[wave])
                {
                    SweepBlizzardFront(wave, _blizzardProgress[wave], progress);
                    _blizzardProgress[wave] = progress;
                }
            }
        }

        private void SweepBlizzardFront(int wave, float fromProgress, float toProgress)
        {
            var direction = BlizzardDirection(wave);
            for (var index = 0; index < _masked.Length; index++)
            {
                var position = PositionAlong(direction, index);
                if (_masked[index] || position <= fromProgress || position > toProgress)
                {
                    continue;
                }

                if (GetDepth(index) < _settings.FullDepth)
                {
                    SetDepth(index, _settings.FullDepth);
                }
            }
        }

        private int BlizzardDirection(int wave)
        {
            return (int)(SnowHash.Mix(_seed, wave, BlizzardSalt) % 4);
        }

        private float PositionAlong(int direction, int index)
        {
            var x = index % Width + 0.5f;
            var y = index / Width + 0.5f;
            switch (direction)
            {
                case 0:
                    return x / Width;
                case 1:
                    return 1f - x / Width;
                case 2:
                    return y / Height;
                default:
                    return 1f - y / Height;
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

            var index = y * Width + x;
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
            var x = index % Width;
            var y = index / Width;
            return _settings.Origin + new Vector2(x + 0.5f, y + 0.5f) * _settings.CellSize;
        }

        private static bool IsInsideObstacle(VehicleArena arena, Vector2 point)
        {
            for (var i = 0; i < arena.Boxes.Count; i++)
            {
                var box = arena.Boxes[i];
                var offset = point - box.Center;
                if (Mathf.Abs(offset.x) <= box.HalfExtents.x && Mathf.Abs(offset.y) <= box.HalfExtents.y)
                {
                    return true;
                }
            }

            for (var i = 0; i < arena.Circles.Count; i++)
            {
                var circle = arena.Circles[i];
                if ((point - circle.Center).sqrMagnitude <= circle.Radius * circle.Radius)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetDepth(int index)
        {
            var shift = index % CellsPerWord * BitsPerCell;
            return (_words[index / CellsPerWord] >> shift) & DepthMask;
        }

        private void SetDepth(int index, int depth)
        {
            var word = index / CellsPerWord;
            var shift = index % CellsPerWord * BitsPerCell;
            _words[word] = (_words[word] & ~(DepthMask << shift)) | (depth << shift);
        }
    }
}
