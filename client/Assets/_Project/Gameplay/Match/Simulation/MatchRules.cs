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

        public static bool IsInputLocked(MatchPhase phase)
        {
            return phase != MatchPhase.Playing;
        }

        public static float PhaseElapsed(int tick, int phaseStartTick, float deltaTime)
        {
            return Mathf.Max(0, tick - phaseStartTick) * deltaTime;
        }

        public MatchPhase NextPhase(MatchPhase phase, float phaseElapsed, bool allSlotsFilled)
        {
            switch (phase)
            {
                case MatchPhase.WaitingForPlayers:
                    return allSlotsFilled ? MatchPhase.Countdown : phase;
                case MatchPhase.Countdown:
                    return phaseElapsed >= _settings.CountdownDuration ? MatchPhase.Playing : phase;
                case MatchPhase.Playing:
                    return phaseElapsed >= _settings.PlayingDuration ? MatchPhase.Results : phase;
                default:
                    return phase;
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
                case MatchPhase.WaitingForPlayers:
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
                case MatchPhase.WaitingForPlayers:
                    return _settings.WaitingDuration;
                case MatchPhase.Countdown:
                    return _settings.CountdownDuration;
                case MatchPhase.Playing:
                    return _settings.PlayingDuration;
                default:
                    return 0f;
            }
        }
    }
}
