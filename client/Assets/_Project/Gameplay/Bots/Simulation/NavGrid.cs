using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class NavGrid
    {
        private const float SightStepShare = 0.25f;
        private const int MaxSearchRing = 8;

        private readonly bool[] _walkable;

        private NavGrid(Vector2 origin, float cellSize, int width, int height)
        {
            Origin = origin;
            CellSize = cellSize;
            Width = width;
            Height = height;
            _walkable = new bool[width * height];
        }

        public Vector2 Origin { get; }

        public float CellSize { get; }

        public int Width { get; }

        public int Height { get; }

        public int CellCount => _walkable.Length;

        public static NavGrid Build(VehicleArena arena, Vector2 origin, Vector2 size, float cellSize, float clearance)
        {
            var grid = new NavGrid(origin, cellSize, Mathf.RoundToInt(size.x / cellSize), Mathf.RoundToInt(size.y / cellSize));
            for (var index = 0; index < grid._walkable.Length; index++)
            {
                grid._walkable[index] = IsClear(arena, grid.CentreOf(grid.CellAt(index)), clearance);
            }

            return grid;
        }

        public int IndexOf(Vector2Int cell)
        {
            return cell.y * Width + cell.x;
        }

        public Vector2Int CellAt(int index)
        {
            return new Vector2Int(index % Width, index / Width);
        }

        public bool Contains(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public bool IsWalkable(Vector2Int cell)
        {
            return Contains(cell) && _walkable[IndexOf(cell)];
        }

        public bool IsWalkable(int index)
        {
            return _walkable[index];
        }

        public Vector2Int CellOf(Vector2 point)
        {
            var local = (point - Origin) / CellSize;
            return new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt(local.x), 0, Width - 1),
                Mathf.Clamp(Mathf.FloorToInt(local.y), 0, Height - 1));
        }

        public Vector2 CentreOf(Vector2Int cell)
        {
            return Origin + new Vector2(cell.x + 0.5f, cell.y + 0.5f) * CellSize;
        }

        public Vector2Int NearestWalkable(Vector2Int cell)
        {
            if (IsWalkable(cell))
            {
                return cell;
            }

            for (var ring = 1; ring <= MaxSearchRing; ring++)
            {
                var best = cell;
                var bestDistance = float.MaxValue;
                for (var y = cell.y - ring; y <= cell.y + ring; y++)
                {
                    for (var x = cell.x - ring; x <= cell.x + ring; x++)
                    {
                        var candidate = new Vector2Int(x, y);
                        var distance = (candidate - cell).sqrMagnitude;
                        if (distance < bestDistance && IsWalkable(candidate))
                        {
                            best = candidate;
                            bestDistance = distance;
                        }
                    }
                }

                if (bestDistance < float.MaxValue)
                {
                    return best;
                }
            }

            return cell;
        }

        public Vector2 NearestWalkablePoint(Vector2 point)
        {
            var cell = CellOf(point);
            return IsWalkable(cell) ? point : CentreOf(NearestWalkable(cell));
        }

        public bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            var offset = to - from;
            var distance = offset.magnitude;
            var step = CellSize * SightStepShare;
            var samples = Mathf.CeilToInt(distance / step);
            for (var i = 1; i <= samples; i++)
            {
                if (!IsWalkable(CellOf(from + offset * ((float)i / samples))))
                {
                    return false;
                }
            }

            return true;
        }

        public Vector2 OpenDirection(Vector2 point, int radius)
        {
            var centre = CellOf(point);
            var away = Vector2.zero;
            for (var y = centre.y - radius; y <= centre.y + radius; y++)
            {
                for (var x = centre.x - radius; x <= centre.x + radius; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (cell != centre && !IsWalkable(cell))
                    {
                        var offset = point - CentreOf(cell);
                        away += offset / Mathf.Max(offset.sqrMagnitude, 0.01f);
                    }
                }
            }

            return away.sqrMagnitude > 0f ? away.normalized : Vector2.zero;
        }

        private static bool IsClear(VehicleArena arena, Vector2 point, float clearance)
        {
            var boxes = arena.Boxes;
            for (var i = 0; i < boxes.Count; i++)
            {
                var offset = point - boxes[i].Center;
                var outside = new Vector2(
                    Mathf.Max(0f, Mathf.Abs(offset.x) - boxes[i].HalfExtents.x),
                    Mathf.Max(0f, Mathf.Abs(offset.y) - boxes[i].HalfExtents.y));
                if (outside.sqrMagnitude <= clearance * clearance)
                {
                    return false;
                }
            }

            var circles = arena.Circles;
            for (var i = 0; i < circles.Count; i++)
            {
                var reach = circles[i].Radius + clearance;
                if ((point - circles[i].Center).sqrMagnitude <= reach * reach)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
