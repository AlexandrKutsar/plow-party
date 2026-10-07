using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotStuckWatch
    {
        private readonly BotSettings _settings;
        private Vector2 _anchor;
        private float _anchorTime;
        private float _stillSince;
        private bool _isWatching;
        private bool _isStill;

        public BotStuckWatch(BotSettings settings)
        {
            _settings = settings;
        }

        public bool Observe(Vector2 position, float time, bool wantsToMove)
        {
            if (!wantsToMove || !_isWatching)
            {
                _isStill = false;
                _isWatching = wantsToMove;
                Restart(position, time);
                return false;
            }

            if ((position - _anchor).sqrMagnitude > _settings.StuckDistance * _settings.StuckDistance)
            {
                _isStill = false;
                Restart(position, time);
                return false;
            }

            if (!_isStill)
            {
                _isStill = true;
                _stillSince = _anchorTime;
            }

            return time - _anchorTime >= _settings.StuckTime;
        }

        public void Stop()
        {
            _isWatching = false;
            _isStill = false;
        }

        public void Restart(Vector2 position, float time)
        {
            _anchor = position;
            _anchorTime = time;
        }

        public float StillFor(float time)
        {
            return _isStill ? time - _stillSince : 0f;
        }
    }
}
