using Fusion;
using VContainer;

namespace PlowParty.Meta.Session.Network
{
    public sealed class LobbyLink : NetworkBehaviour, IPlayerLeft
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

        public void PlayerLeft(PlayerRef player)
        {
            if (HasStateAuthority)
            {
                _links.NotifyPlayerLeft(player.RawEncoded);
            }
        }

        public void RequestPartyStop(string partyCode)
        {
            RPC_PartyStopped(partyCode);
        }

        public void RequestPartyArrived(string partyCode)
        {
            RPC_PartyArrived(partyCode);
        }

        [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PartyStopped(NetworkString<_16> partyCode)
        {
            _links.NotifyPartyStopped(partyCode.ToString());
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PartyArrived(NetworkString<_16> partyCode, RpcInfo info = default)
        {
            _links.NotifyPartyArrived(info.Source.RawEncoded, partyCode.ToString());
        }
    }
}
