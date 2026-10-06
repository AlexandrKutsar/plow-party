namespace PlowParty.Gameplay.Hud.Simulation
{
    public sealed class ScoreRise
    {
        private readonly float _popupInterval;
        private bool _hasScore;
        private int _lastScore;
        private int _pending;
        private float _sinceLastPopup;

        public ScoreRise(float popupInterval)
        {
            _popupInterval = popupInterval;
        }

        public int Observe(int score, float deltaTime)
        {
            if (!_hasScore || score < _lastScore)
            {
                Restart(score);
                return 0;
            }

            _pending += score - _lastScore;
            _lastScore = score;
            _sinceLastPopup += deltaTime;
            if (_pending <= 0 || _sinceLastPopup < _popupInterval)
            {
                return 0;
            }

            var popup = _pending;
            _pending = 0;
            _sinceLastPopup = 0f;
            return popup;
        }

        public void Forget()
        {
            _hasScore = false;
        }

        private void Restart(int score)
        {
            _hasScore = true;
            _lastScore = score;
            _pending = 0;
            _sinceLastPopup = _popupInterval;
        }
    }
}
