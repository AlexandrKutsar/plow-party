using System;
using Fusion;

namespace PlowParty.Meta.Party.Network
{
    public sealed class PartyLinks
    {
        public event Action Removed;

        public PartyLink Current { get; private set; }

        public void Spawn(NetworkRunner runner, NetworkObject prefab)
        {
            if (runner.IsServer && Current == null)
            {
                runner.Spawn(prefab);
            }
        }

        public void Attach(PartyLink link)
        {
            Current = link;
        }

        public void Detach(PartyLink link)
        {
            if (Current == link)
            {
                Current = null;
            }
        }

        public void NotifyRemoved()
        {
            Removed?.Invoke();
        }
    }
}
