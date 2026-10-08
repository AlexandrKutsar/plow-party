using Fusion;
using VContainer;

namespace PlowParty.Meta.Session.Network
{
    public sealed class LobbyLink : NetworkBehaviour
    {
        private LobbyLinks _links;

        [Inject]
        public void Construct(LobbyLinks links)
        {
            _links = links;
        }

        public override void Spawned()
        {
            _links.Attach(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _links.Detach(this);
        }

        public void RequestPartyStop(string partyCode)
        {
            RPC_PartyStopped(partyCode);
        }

        [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PartyStopped(NetworkString<_16> partyCode)
        {
            _links.NotifyPartyStopped(partyCode.ToString());
        }
    }
}
