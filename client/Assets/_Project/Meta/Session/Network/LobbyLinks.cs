using System;
using Fusion;

namespace PlowParty.Meta.Session.Network
{
    public sealed class LobbyLinks
    {
        public event Action<string> PartyStopped;

        public LobbyLink Current { get; private set; }

        public void Spawn(NetworkRunner runner, NetworkObject prefab)
        {
            if (runner != null && runner.IsServer && Current == null && prefab != null)
            {
                runner.Spawn(prefab);
            }
        }

        public void Despawn(NetworkRunner runner)
        {
            if (runner != null && runner.IsServer && Current != null)
            {
                runner.Despawn(Current.Object);
            }
        }

        public void Attach(LobbyLink link)
        {
            Current = link;
        }

        public void Detach(LobbyLink link)
        {
            if (Current == link)
            {
                Current = null;
            }
        }

        public void NotifyPartyStopped(string partyCode)
        {
            PartyStopped?.Invoke(partyCode);
        }
    }
}
