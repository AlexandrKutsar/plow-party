using PlowParty.Gameplay.Hud.View;
using PlowParty.Meta.Session;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class MatchExitAdapter : IMatchExit
    {
        private readonly SessionExit _exit;

        public MatchExitAdapter(SessionExit exit)
        {
            _exit = exit;
        }

        public void LeaveToMenu()
        {
            _exit.LeaveToMenu();
        }
    }
}
