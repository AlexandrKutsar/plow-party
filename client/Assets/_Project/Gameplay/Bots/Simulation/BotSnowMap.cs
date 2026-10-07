using System;
using UnityEngine;
using Random = System.Random;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotSnowMap
    {
        private const float MinTargetDistance = 3f;
        private const float HeadingBonus = 0.5f;

        private readonly NavGrid _grid;
        private readonly int[] _snow;
        private readonly int[] _pile;
        private readonly int[] _richness;
        private int _maxRichness = 1;

        public BotSnowMap(NavGrid grid)
        {
            _grid = grid;
            _snow = new int[grid.CellCount];
            _pile = new int[grid.CellCount];
            _richness = new int[grid.CellCount];
        }

        public int SnowAt(Vector2Int cell)
        {
            return _snow[_grid.IndexOf(cell)];
        }

        public int PileAt(Vector2Int cell)
        {
            return _pile[_grid.IndexOf(cell)];
        }

        public float RichnessShare(Vector2Int cell)
        {
            return (float)_richness[_grid.IndexOf(cell)] / _maxRichness;
        }

        public void Refresh(ISnowDepths depths)
        {
            Array.Clear(_snow, 0, _snow.Length);
            Array.Clear(_pile, 0, _pile.Length);
            for (var y = 0; y < depths.Height; y++)
            {
                for (var x = 0; x < depths.Width; x++)
                {
                    var depth = depths.GetDepth(x, y);
                    if (depth <= 0)
                    {
                        continue;
                    }

                    var centre = depths.Origin + new Vector2(x + 0.5f, y + 0.5f) * depths.CellSize;
                    var index = _grid.IndexOf(_grid.CellOf(centre));
                    _snow[index] += Mathf.Min(depth, depths.FullDepth);
                    _pile[index] += Mathf.Max(0, depth - depths.FullDepth);
                }
            }

            var snowCellsPerNavCell = Mathf.RoundToInt(_grid.CellSize / depths.CellSize);
            _maxRichness = Mathf.Max(1, 9 * snowCellsPerNavCell * snowCellsPerNavCell * depths.FullDepth);
            SumNeighbourhoods();
        }

        public bool TryFindSnow(
            Vector2 from,
            Vector2 forward,
            Vector2[] others,
            int otherCount,
            BotSettings settings,
            float noise,
            Random random,
            out Vector2 target,
            out float richness)
        {
            target = from;
            richness = 0f;
            var bestScore = 0f;
            for (var index = 0; index < _richness.Length; index++)
            {
                if (_richness[index] <= 0 || _snow[index] <= 0 || !_grid.IsWalkable(index))
                {
                    continue;
                }

                var centre = _grid.CentreOf(_grid.CellAt(index));
                var offset = centre - from;
                var distance = offset.magnitude;
                if (distance < MinTargetDistance)
                {
                    continue;
                }

                var share = (float)_richness[index] / _maxRichness;
                var heading = 1f + HeadingBonus * Vector2.Dot(forward, offset / distance);
                var crowd = IsCrowded(centre, others, otherCount, settings.CrowdRadius) ? settings.CrowdPenalty : 1f;
                var jitter = 1f + noise * ((float)random.NextDouble() - 0.5f);
                var score = share * heading * crowd * jitter / (1f + distance / settings.SnowFalloff);
                if (score > bestScore)
                {
                    bestScore = score;
                    target = centre;
                    richness = share;
                }
            }

            return bestScore > 0f;
        }

        public bool TryFindPile(Vector2 from, BotSettings settings, out Vector2 point, out int steps)
        {
            point = from;
            steps = 0;
            var bestScore = 0f;
            for (var index = 0; index < _pile.Length; index++)
            {
                if (_pile[index] < settings.MinPileSteps)
                {
                    continue;
                }

                var centre = _grid.CentreOf(_grid.CellAt(index));
                var score = _pile[index] / (1f + Vector2.Distance(from, centre) / settings.PileFalloff);
                if (score > bestScore)
                {
                    bestScore = score;
                    point = centre;
                    steps = _pile[index];
                }
            }

            return bestScore > 0f;
        }

        private static bool IsCrowded(Vector2 point, Vector2[] others, int otherCount, float radius)
        {
            for (var i = 0; i < otherCount; i++)
            {
                if ((others[i] - point).sqrMagnitude <= radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        private void SumNeighbourhoods()
        {
            for (var index = 0; index < _richness.Length; index++)
            {
                var cell = _grid.CellAt(index);
                var sum = 0;
                for (var y = cell.y - 1; y <= cell.y + 1; y++)
                {
                    for (var x = cell.x - 1; x <= cell.x + 1; x++)
                    {
                        var neighbour = new Vector2Int(x, y);
                        if (_grid.Contains(neighbour))
                        {
                            sum += _snow[_grid.IndexOf(neighbour)];
                        }
                    }
                }

                _richness[index] = sum;
            }
        }
    }
}
