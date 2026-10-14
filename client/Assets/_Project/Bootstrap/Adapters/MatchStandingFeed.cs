using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Meta.Session;
using PlowParty.Meta.Session.Simulation;
using VContainer.Unity;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class MatchStandingFeed : ITickable
    {
        private readonly IMatchClock _clock;
        private readonly IMatchAdmission _admission;
        private readonly SessionExit _exit;

        public MatchStandingFeed(IMatchClock clock, IMatchAdmission admission, SessionExit exit)
        {
            _clock = clock;
            _admission = admission;
            _exit = exit;
        }

        public void Tick()
        {
            if (_clock.IsRunning)
            {
                _exit.ObserveMatch(_admission.IsLocalPlayerTurnedAway ? MatchStanding.TurnedAway : StandingOf(_clock.Phase));
            }
        }

        private static MatchStanding StandingOf(MatchPhase phase)
        {
            switch (phase)
            {
                case MatchPhase.Playing:
                    return MatchStanding.Playing;
                case MatchPhase.Results:
                    return MatchStanding.Finished;
                default:
                    return MatchStanding.BeforePlaying;
            }
        }
    }
}
