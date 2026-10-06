using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Snow.View
{
    internal sealed class SnowHeightField
    {
        private const float SettledDifference = 0.5f / byte.MaxValue;

        private readonly float[] _heightOfDepth = new float[SnowGrid.MaxDepth + 1];
        private readonly float[] _shown;
        private readonly float[] _target;
        private readonly byte[] _texels;
        private readonly int[] _moving;
        private readonly bool[] _isMoving;
        private readonly float _lowerTime;
        private readonly float _raiseTime;
        private int _movingCount;

        public SnowHeightField(SnowGrid grid, SnowSettings settings, SnowConfig config)
        {
            MaxHeight = Mathf.Max(config.SnowHeight + config.PileHeight, 1e-3f);
            FullSnowLine = config.SnowHeight / MaxHeight;
            _lowerTime = config.LowerTime;
            _raiseTime = config.RaiseTime;
            for (var depth = 0; depth <= SnowGrid.MaxDepth; depth++)
            {
                _heightOfDepth[depth] = MetresOf(depth, settings.FullDepth, config) / MaxHeight;
            }

            var cells = grid.Width * grid.Height;
            _shown = new float[cells];
            _target = new float[cells];
            _texels = new byte[cells];
            _moving = new int[cells];
            _isMoving = new bool[cells];
            Texture = new Texture2D(grid.Width, grid.Height, TextureFormat.R8, false, true)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "SnowHeight",
            };
            for (var cell = 0; cell < cells; cell++)
            {
                _target[cell] = _heightOfDepth[grid.GetDepth(cell)];
                _shown[cell] = _target[cell];
                _texels[cell] = ToTexel(_shown[cell]);
            }

            Upload();
        }

        public Texture2D Texture { get; }

        public float MaxHeight { get; }

        public float FullSnowLine { get; }

        public void SetDepth(int cell, int depth)
        {
            var target = _heightOfDepth[depth];
            if (Mathf.Approximately(target, _target[cell]))
            {
                return;
            }

            _target[cell] = target;
            if (!_isMoving[cell])
            {
                _isMoving[cell] = true;
                _moving[_movingCount++] = cell;
            }
        }

        public void Advance(float deltaTime)
        {
            if (_movingCount == 0)
            {
                return;
            }

            var lowerBlend = Blend(deltaTime, _lowerTime);
            var raiseBlend = Blend(deltaTime, _raiseTime);
            var changed = false;
            var kept = 0;
            for (var i = 0; i < _movingCount; i++)
            {
                var cell = _moving[i];
                var difference = _target[cell] - _shown[cell];
                var shown = Mathf.Abs(difference) <= SettledDifference
                    ? _target[cell]
                    : _shown[cell] + difference * (difference < 0f ? lowerBlend : raiseBlend);
                _shown[cell] = shown;
                var texel = ToTexel(shown);
                changed |= texel != _texels[cell];
                _texels[cell] = texel;
                if (Mathf.Approximately(shown, _target[cell]))
                {
                    _isMoving[cell] = false;
                }
                else
                {
                    _moving[kept++] = cell;
                }
            }

            _movingCount = kept;
            if (changed)
            {
                Upload();
            }
        }

        private static float MetresOf(int depth, int fullDepth, SnowConfig config)
        {
            if (depth <= fullDepth)
            {
                return config.SnowHeight * depth / fullDepth;
            }

            return config.SnowHeight + config.PileHeight * (depth - fullDepth) / (SnowGrid.MaxDepth - fullDepth);
        }

        private static float Blend(float deltaTime, float easeTime)
        {
            return easeTime <= 0f ? 1f : 1f - Mathf.Exp(-deltaTime / easeTime);
        }

        private static byte ToTexel(float height)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(height) * byte.MaxValue);
        }

        private void Upload()
        {
            Texture.SetPixelData(_texels, 0);
            Texture.Apply(false);
        }
    }
}
