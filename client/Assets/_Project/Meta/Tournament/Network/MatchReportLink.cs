using Fusion;
using VContainer;

namespace PlowParty.Meta.Tournament.Network
{
    public sealed class MatchReportLink : NetworkBehaviour
    {
        private MatchReportLinks _links;

        [Networked] public int MatchNumber { get; private set; }

        [Networked] public int RosterSlotMask { get; private set; }

        [Networked] private NetworkString<_64> NetworkMatchId { get; set; }

        public string MatchId => NetworkMatchId.Value;

        [Inject]
        public void Construct(MatchReportLinks links)
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

        public void Publish(int matchNumber, string matchId, int rosterSlotMask)
        {
            if (!HasStateAuthority)
            {
                return;
            }

            NetworkMatchId = matchId;
            RosterSlotMask = rosterSlotMask;
            MatchNumber = matchNumber;
        }
    }
}
