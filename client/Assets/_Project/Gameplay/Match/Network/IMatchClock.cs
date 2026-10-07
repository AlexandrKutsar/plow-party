using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Network
{
    public interface IMatchClock
    {
        bool IsRunning { get; }

        MatchPhase Phase { get; }

        int MatchNumber { get; }

        float PhaseRemaining { get; }

        float PlayingElapsed { get; }

        float PlayingRemaining { get; }
    }
}
