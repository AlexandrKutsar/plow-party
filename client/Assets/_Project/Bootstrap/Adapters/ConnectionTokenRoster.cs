using System.Collections.Generic;
using Fusion;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Session;
using PlowParty.Meta.Account;
using PlowParty.Meta.Session.Config;
using PlowParty.Meta.Tournament.Network;
using PlowParty.Meta.Tournament.Simulation;
using PlowParty.Shared;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class ConnectionTokenRoster : IMatchRoster
    {
        private readonly VehicleRegistry _vehicles;
        private readonly NetworkSession _session;
        private readonly MatchLineupStore _lineups;
        private readonly AccountService _account;
        private readonly MatchmakingConfig _matchmaking;

        public ConnectionTokenRoster(VehicleRegistry vehicles, NetworkSession session, MatchLineupStore lineups, AccountService account, MatchmakingConfig matchmaking)
        {
            _vehicles = vehicles;
            _session = session;
            _lineups = lineups;
            _account = account;
            _matchmaking = matchmaking;
        }

        public IReadOnlyList<RosterSeat> ReadSeats()
        {
            var seats = new List<RosterSeat>();
            var taken = new HashSet<int>();
            foreach (var vehicle in _vehicles.Vehicles)
            {
                if (taken.Add(vehicle.Slot))
                {
                    seats.Add(new RosterSeat(vehicle.Slot, AccountOf(vehicle.Object.InputAuthority)));
                }
            }

            var maxSlots = _lineups.TryGet(out var lineup) ? lineup.MaxSlots : _matchmaking.MaxSlots;
            for (var slot = 0; slot < maxSlots; slot++)
            {
                if (!taken.Contains(slot))
                {
                    seats.Add(new RosterSeat(slot, null));
                }
            }

            return seats;
        }

        private string AccountOf(PlayerRef player)
        {
            var runner = _session.Runner;
            if (runner == null || player == PlayerRef.None)
            {
                return null;
            }

            if (ParticipantToken.TryFromBytes(runner.GetPlayerConnectionToken(player), out var token))
            {
                return token.AccountId;
            }

            return player == runner.LocalPlayer ? _account.AccountId : null;
        }
    }
}
