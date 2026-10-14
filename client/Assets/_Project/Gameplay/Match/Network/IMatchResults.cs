using PlowParty.Gameplay.Match.Simulation;

namespace PlowParty.Gameplay.Match.Network
{
    public interface IMatchResults
    {
        int PlacementCount { get; }

        MatchPlacement GetPlacement(int index);
    }
}
