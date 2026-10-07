using PlowParty.Gameplay.Bots.Simulation;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Network
{
    public sealed class SnowDepthsReader : ISnowDepths
    {
        private readonly SnowGridDriver _snow;
        private readonly SnowSettings _settings;

        public SnowDepthsReader(SnowGridDriver snow, SnowSettings settings)
        {
            _snow = snow;
            _settings = settings;
        }

        public Vector2 Origin => _settings.Origin;

        public float CellSize => _settings.CellSize;

        public int Width => _snow.Width;

        public int Height => _snow.Height;

        public int FullDepth => _settings.FullDepth;

        public int GetDepth(int x, int y)
        {
            return _snow.GetHostDepth(x, y);
        }
    }
}
