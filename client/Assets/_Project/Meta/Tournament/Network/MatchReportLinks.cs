using Fusion;

namespace PlowParty.Meta.Tournament.Network
{
    public sealed class MatchReportLinks
    {
        private bool _spawnRequested;

        public MatchReportLink Current { get; private set; }

        public void SpawnOnce(NetworkRunner runner, NetworkObject prefab)
        {
            if (_spawnRequested || !runner.IsServer)
            {
                return;
            }

            _spawnRequested = true;
            runner.Spawn(prefab);
        }

        public void Attach(MatchReportLink link)
        {
            Current = link;
        }

        public void Detach(MatchReportLink link)
        {
            if (Current == link)
            {
                Current = null;
            }
        }
    }
}
