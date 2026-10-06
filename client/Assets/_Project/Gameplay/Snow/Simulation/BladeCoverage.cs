using UnityEngine;

namespace PlowParty.Gameplay.Snow.Simulation
{
    internal readonly struct BladeCoverage
    {
        private readonly Vector2 _centre;
        private readonly Vector2 _forward;
        private readonly Vector2 _right;
        private readonly float _halfWidth;
        private readonly float _halfDepth;

        public BladeCoverage(SnowBlade blade)
        {
            _centre = blade.Centre;
            _forward = blade.Forward.normalized;
            _right = new Vector2(_forward.y, -_forward.x);
            _halfWidth = blade.Width * 0.5f;
            _halfDepth = blade.Depth * 0.5f;
            Reach = new Vector2(
                Mathf.Abs(_right.x) * _halfWidth + Mathf.Abs(_forward.x) * _halfDepth,
                Mathf.Abs(_right.y) * _halfWidth + Mathf.Abs(_forward.y) * _halfDepth);
        }

        public Vector2 Reach { get; }

        public Vector2 Min => _centre - Reach;

        public Vector2 Max => _centre + Reach;

        public bool Covers(Vector2 point)
        {
            var offset = point - _centre;
            return Mathf.Abs(Vector2.Dot(offset, _right)) <= _halfWidth && Mathf.Abs(Vector2.Dot(offset, _forward)) <= _halfDepth;
        }
    }
}
