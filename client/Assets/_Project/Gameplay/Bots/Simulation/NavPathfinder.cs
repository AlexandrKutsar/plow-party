using System.Collections.Generic;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class NavPathfinder
    {
        private const float Diagonal = 1.41421356f;
        private const int NeighbourCount = 8;

        private static readonly Vector2Int[] Steps =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        private readonly NavGrid _grid;
        private readonly float[] _cost;
        private readonly int[] _parent;
        private readonly int[] _seenStamp;
        private readonly int[] _closedStamp;
        private readonly int[] _heapCells;
        private readonly float[] _heapPriorities;
        private readonly List<int> _cells = new List<int>();
        private int _heapCount;
        private int _stamp;

        public NavPathfinder(NavGrid grid)
        {
            _grid = grid;
            _cost = new float[grid.CellCount];
            _parent = new int[grid.CellCount];
            _seenStamp = new int[grid.CellCount];
            _closedStamp = new int[grid.CellCount];
            _heapCells = new int[grid.CellCount * NeighbourCount + 1];
            _heapPriorities = new float[grid.CellCount * NeighbourCount + 1];
        }

        public NavGrid Grid => _grid;

        public bool TryFindPath(Vector2 from, Vector2 to, List<Vector2> path)
        {
            path.Clear();
            var start = _grid.NearestWalkable(_grid.CellOf(from));
            var goal = _grid.NearestWalkable(_grid.CellOf(to));
            if (!_grid.IsWalkable(start) || !_grid.IsWalkable(goal))
            {
                return false;
            }

            var goalPoint = _grid.IsWalkable(_grid.CellOf(to)) ? to : _grid.CentreOf(goal);
            if (_grid.HasLineOfSight(from, goalPoint))
            {
                path.Add(goalPoint);
                return true;
            }

            if (!Search(_grid.IndexOf(start), _grid.IndexOf(goal)))
            {
                return false;
            }

            Smooth(from, start, _grid.IndexOf(goal), goalPoint, path);
            return true;
        }

        private bool Search(int start, int goal)
        {
            _stamp++;
            _heapCount = 0;
            var goalCell = _grid.CellAt(goal);
            Visit(start, -1, 0f, goalCell);
            while (_heapCount > 0)
            {
                var current = Pop();
                if (_closedStamp[current] == _stamp)
                {
                    continue;
                }

                _closedStamp[current] = _stamp;
                if (current == goal)
                {
                    return true;
                }

                Expand(current, goalCell);
            }

            return false;
        }

        private void Expand(int current, Vector2Int goalCell)
        {
            var cell = _grid.CellAt(current);
            for (var i = 0; i < NeighbourCount; i++)
            {
                var step = Steps[i];
                var next = cell + step;
                if (!_grid.IsWalkable(next) || IsCutCorner(cell, step))
                {
                    continue;
                }

                var index = _grid.IndexOf(next);
                if (_closedStamp[index] == _stamp)
                {
                    continue;
                }

                var cost = _cost[current] + (step.x != 0 && step.y != 0 ? Diagonal : 1f);
                if (_seenStamp[index] != _stamp || cost < _cost[index])
                {
                    Visit(index, current, cost, goalCell);
                }
            }
        }

        private bool IsCutCorner(Vector2Int cell, Vector2Int step)
        {
            return step.x != 0 && step.y != 0
                && (!_grid.IsWalkable(new Vector2Int(cell.x + step.x, cell.y)) || !_grid.IsWalkable(new Vector2Int(cell.x, cell.y + step.y)));
        }

        private void Visit(int index, int parent, float cost, Vector2Int goalCell)
        {
            _seenStamp[index] = _stamp;
            _cost[index] = cost;
            _parent[index] = parent;
            Push(index, cost + Octile(_grid.CellAt(index), goalCell));
        }

        private static float Octile(Vector2Int from, Vector2Int to)
        {
            var dx = Mathf.Abs(from.x - to.x);
            var dy = Mathf.Abs(from.y - to.y);
            return Mathf.Max(dx, dy) + (Diagonal - 1f) * Mathf.Min(dx, dy);
        }

        private void Smooth(Vector2 from, Vector2Int start, int goal, Vector2 goalPoint, List<Vector2> path)
        {
            _cells.Clear();
            for (var index = goal; index >= 0; index = _parent[index])
            {
                _cells.Add(index);
            }

            _cells.Reverse();
            var anchor = from;
            if (!_grid.IsWalkable(_grid.CellOf(from)))
            {
                anchor = _grid.CentreOf(start);
                path.Add(anchor);
            }

            var last = _cells.Count - 1;
            var reached = 0;
            while (reached < last)
            {
                var next = last;
                while (next > reached + 1 && !_grid.HasLineOfSight(anchor, PointAt(next, last, goalPoint)))
                {
                    next--;
                }

                anchor = PointAt(next, last, goalPoint);
                path.Add(anchor);
                reached = next;
            }

            if (path.Count == 0)
            {
                path.Add(goalPoint);
            }
        }

        private Vector2 PointAt(int index, int last, Vector2 goalPoint)
        {
            return index == last ? goalPoint : _grid.CentreOf(_grid.CellAt(_cells[index]));
        }

        private void Push(int cell, float priority)
        {
            var child = _heapCount++;
            while (child > 0)
            {
                var parent = (child - 1) / 2;
                if (_heapPriorities[parent] <= priority)
                {
                    break;
                }

                _heapCells[child] = _heapCells[parent];
                _heapPriorities[child] = _heapPriorities[parent];
                child = parent;
            }

            _heapCells[child] = cell;
            _heapPriorities[child] = priority;
        }

        private int Pop()
        {
            var top = _heapCells[0];
            var lastCell = _heapCells[--_heapCount];
            var lastPriority = _heapPriorities[_heapCount];
            var parent = 0;
            while (true)
            {
                var child = parent * 2 + 1;
                if (child >= _heapCount)
                {
                    break;
                }

                if (child + 1 < _heapCount && _heapPriorities[child + 1] < _heapPriorities[child])
                {
                    child++;
                }

                if (_heapPriorities[child] >= lastPriority)
                {
                    break;
                }

                _heapCells[parent] = _heapCells[child];
                _heapPriorities[parent] = _heapPriorities[child];
                parent = child;
            }

            _heapCells[parent] = lastCell;
            _heapPriorities[parent] = lastPriority;
            return top;
        }
    }
}
