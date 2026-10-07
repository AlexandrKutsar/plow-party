using PlowParty.Gameplay.Bots.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Tests
{
    internal sealed class FakeSnowDepths : ISnowDepths
    {
        private readonly int[] _depths;

        public FakeSnowDepths(int fill)
        {
            Width = Mathf.RoundToInt(BotTestSettings.Size.x / CellSize);
            Height = Mathf.RoundToInt(BotTestSettings.Size.y / CellSize);
            _depths = new int[Width * Height];
            for (var i = 0; i < _depths.Length; i++)
            {
                _depths[i] = fill;
            }
        }

        public Vector2 Origin => BotTestSettings.Origin;

        public float CellSize => 0.5f;

        public int Width { get; }

        public int Height { get; }

        public int FullDepth => 3;

        public int GetDepth(int x, int y)
        {
            return _depths[y * Width + x];
        }

        public FakeSnowDepths Fill(Vector2 min, Vector2 max, int depth)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var centre = Origin + new Vector2(x + 0.5f, y + 0.5f) * CellSize;
                    if (centre.x >= min.x && centre.x <= max.x && centre.y >= min.y && centre.y <= max.y)
                    {
                        _depths[y * Width + x] = depth;
                    }
                }
            }

            return this;
        }
    }
}
