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
        private readonly SessionExit _exit;

        public MatchStandingFeed(IMatchClock clock, SessionExit exit)
        {
            _clock = clock;
            _exit = exit;
        }

        public void Tick()
        {
            if (_clock.IsRunning)
            {
                _exit.ObserveMatch(_clock.Phase == MatchPhase.Results ? MatchStanding.Finished : MatchStanding.Underway);
            }
        }
    }
}
