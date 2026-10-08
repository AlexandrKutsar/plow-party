using System.Collections.Generic;

namespace PlowParty.Meta.Party.Simulation
{
    public sealed class PartyState
    {
        private readonly List<PartyMember> _members = new List<PartyMember>();
        private readonly int _capacity;

        public PartyState(int capacity, PartyMode mode = PartyMode.QuickPlay)
        {
            _capacity = capacity;
            Mode = mode;
        }

        public IReadOnlyList<PartyMember> Members => _members;

        public PartyMode Mode { get; private set; }

        public int Count => _members.Count;

        public bool IsFull => _members.Count >= _capacity;

        public bool HasLeader => _members.Count > 0;

        public int LeaderId => HasLeader ? _members[0].Id : -1;

        public bool CanStart
        {
            get
            {
                if (!HasLeader)
                {
                    return false;
                }

                for (var i = 1; i < _members.Count; i++)
                {
                    if (!_members[i].IsReady)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static PartyState Restore(int capacity, PartyMode mode, IEnumerable<PartyMember> members)
        {
            var party = new PartyState(capacity, mode);
            foreach (var member in members)
            {
                if (!party.IsFull && !party.Contains(member.Id))
                {
                    party._members.Add(member);
                }
            }

            return party;
        }

        public bool IsLeader(int id)
        {
            return HasLeader && LeaderId == id;
        }

        public bool Contains(int id)
        {
            return IndexOf(id) >= 0;
        }

        public PartyJoinOutcome Join(int id, string nickname)
        {
            if (Contains(id))
            {
                return PartyJoinOutcome.AlreadyMember;
            }

            if (IsFull)
            {
                return PartyJoinOutcome.Full;
            }

            _members.Add(new PartyMember(id, nickname, false));
            return PartyJoinOutcome.Joined;
        }

        public bool Leave(int id)
        {
            var index = IndexOf(id);
            if (index < 0)
            {
                return false;
            }

            _members.RemoveAt(index);
            return true;
        }

        public bool Remove(int requesterId, int memberId)
        {
            return IsLeader(requesterId) && requesterId != memberId && Leave(memberId);
        }

        public bool SetReady(int memberId, bool isReady)
        {
            var index = IndexOf(memberId);
            if (index <= 0)
            {
                return false;
            }

            _members[index] = _members[index].WithReady(isReady);
            return true;
        }

        public void StopSearch(int memberId)
        {
            SetReady(memberId, false);
        }

        public bool SetMode(int requesterId, PartyMode mode)
        {
            if (!IsLeader(requesterId))
            {
                return false;
            }

            Mode = mode;
            return true;
        }

        private int IndexOf(int id)
        {
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
