using System.Collections.Generic;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Meta.Tournament.Network;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class MatchProgressAdapter : IMatchProgress
    {
        private readonly IMatchClock _clock;
        private readonly IMatchResults _results;
        private readonly VehicleRegistry _vehicles;
        private readonly IScoreReader _scores;

        public MatchProgressAdapter(IMatchClock clock, IMatchResults results, VehicleRegistry vehicles, IScoreReader scores)
        {
            _clock = clock;
            _results = results;
            _vehicles = vehicles;
            _scores = scores;
        }

        public bool IsRunning => _clock.IsRunning;

        public MatchProgressPhase Phase
        {
            get
            {
                switch (_clock.Phase)
                {
                    case MatchPhase.Countdown:
                        return MatchProgressPhase.Countdown;
                    case MatchPhase.Playing:
                        return MatchProgressPhase.Playing;
                    case MatchPhase.Results:
                        return MatchProgressPhase.Results;
                    default:
                        return MatchProgressPhase.Waiting;
                }
            }
        }

        public int MatchNumber => _clock.MatchNumber;

        public float PlayingElapsed => _clock.PlayingElapsed;

        public void ReadFinalScores(Dictionary<int, int> scoresBySlot)
        {
            scoresBySlot.Clear();
            for (var i = 0; i < _results.PlacementCount; i++)
            {
                var placement = _results.GetPlacement(i);
                scoresBySlot[placement.Slot] = placement.Score;
            }
        }

        public void ReadLiveScores(Dictionary<int, int> scoresBySlot)
        {
            scoresBySlot.Clear();
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                scoresBySlot[vehicles[i].Slot] = _scores.ScoreOf(vehicles[i]);
            }
        }
    }
}
