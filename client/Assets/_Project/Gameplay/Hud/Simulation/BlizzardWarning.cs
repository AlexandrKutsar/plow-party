namespace PlowParty.Gameplay.Hud.Simulation
{
    public sealed class BlizzardWarning
    {
        private readonly float[] _blizzardTimes;
        private readonly float _lead;

        public BlizzardWarning(float[] blizzardTimes, float lead)
        {
            _blizzardTimes = blizzardTimes;
            _lead = lead;
        }

        public bool TryGetSecondsLeft(float playingElapsed, out float secondsLeft)
        {
            for (var i = 0; i < _blizzardTimes.Length; i++)
            {
                var left = _blizzardTimes[i] - playingElapsed;
                if (left > 0f && left <= _lead)
                {
                    secondsLeft = left;
                    return true;
                }
            }

            secondsLeft = 0f;
            return false;
        }
    }
}
