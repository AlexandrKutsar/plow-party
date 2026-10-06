using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Network
{
    public interface IMatchResults
    {
        int PlacementCount { get; }

        bool CanRequestRestart { get; }

        MatchPlacement GetPlacement(int index);

        void RequestRestart();
    }
}
