using UnityEngine;

namespace PlowParty.Gameplay.Match.Simulation
{
    public sealed class MatchRules
    {
        private readonly MatchSettings _settings;

        public MatchRules(MatchSettings settings)
        {
            _settings = settings;
        }

        public bool WaitsForRestartRequest => _settings.ResultsDuration <= 0f;

        public static bool IsInputLocked(MatchPhase phase)
        {
            return phase != MatchPhase.Playing;
        }

        public static bool StartsNextMatch(MatchPhase from, MatchPhase to)
        {
            return from == MatchPhase.Results && to == MatchPhase.Countdown;
        }

        public static float PhaseElapsed(int tick, int phaseStartTick, float deltaTime)
        {
            return Mathf.Max(0, tick - phaseStartTick) * deltaTime;
        }

        public MatchPhase NextPhase(MatchPhase phase, float phaseElapsed, bool restartRequested)
        {
            switch (phase)
            {
                case MatchPhase.Countdown:
                    return phaseElapsed >= _settings.CountdownDuration ? MatchPhase.Playing : phase;
                case MatchPhase.Playing:
                    return phaseElapsed >= _settings.PlayingDuration ? MatchPhase.Results : phase;
                default:
                    return restartRequested || ResultsTimedOut(phaseElapsed) ? MatchPhase.Countdown : phase;
            }
        }

        public float PhaseRemaining(MatchPhase phase, float phaseElapsed)
        {
            return Mathf.Max(0f, DurationOf(phase) - phaseElapsed);
        }

        public float PlayingElapsed(MatchPhase phase, float phaseElapsed)
        {
            switch (phase)
            {
                case MatchPhase.Countdown:
                    return 0f;
                case MatchPhase.Playing:
                    return Mathf.Min(phaseElapsed, _settings.PlayingDuration);
                default:
                    return _settings.PlayingDuration;
            }
        }

        public float PlayingRemaining(MatchPhase phase, float phaseElapsed)
        {
            return _settings.PlayingDuration - PlayingElapsed(phase, phaseElapsed);
        }

        private float DurationOf(MatchPhase phase)
        {
            switch (phase)
            {
                case MatchPhase.Countdown:
                    return _settings.CountdownDuration;
                case MatchPhase.Playing:
                    return _settings.PlayingDuration;
                default:
                    return _settings.ResultsDuration;
            }
        }

        private bool ResultsTimedOut(float phaseElapsed)
        {
            return !WaitsForRestartRequest && phaseElapsed >= _settings.ResultsDuration;
        }
    }
}
