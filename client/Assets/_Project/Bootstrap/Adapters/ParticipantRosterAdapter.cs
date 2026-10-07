using System.Collections.Generic;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Meta.Account;
using PlowParty.Meta.Tournament.Network;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Bootstrap.Adapters
{
    public sealed class ParticipantRosterAdapter : IMatchRoster
    {
        private readonly ParticipantRoster _roster;
        private readonly VehicleRegistry _vehicles;
        private readonly AccountService _account;

        public ParticipantRosterAdapter(ParticipantRoster roster, VehicleRegistry vehicles, AccountService account)
        {
            _roster = roster;
            _vehicles = vehicles;
            _account = account;
        }

        public IReadOnlyList<RosterSeat> ReadSeats()
        {
            var localSlot = _vehicles.TryGetLocal(out var local) ? local.Slot : -1;
            var seats = new List<RosterSeat>(_roster.SlotCount);
            for (var slot = 0; slot < _roster.SlotCount; slot++)
            {
                seats.Add(new RosterSeat(slot, AccountOf(slot, localSlot)));
            }

            return seats;
        }

        private string AccountOf(int slot, int localSlot)
        {
            if (!_roster.IsSeated(slot) || _roster.IsBot(slot))
            {
                return null;
            }

            if (_roster.TryGetAccountId(slot, out var accountId))
            {
                return accountId;
            }

            return slot == localSlot ? _account.AccountId : null;
        }
    }
}
