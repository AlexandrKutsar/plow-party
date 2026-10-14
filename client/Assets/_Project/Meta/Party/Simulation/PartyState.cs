using System.Collections.Generic;
using PlowParty.Shared;

namespace PlowParty.Meta.Party.Simulation
{
    public sealed class PartyState
    {
        public const int SoloId = -1;

        private readonly List<PartyMember> _members = new List<PartyMember>();
        private readonly List<PartyBot> _bots = new List<PartyBot>();
        private readonly int _capacity;
        private readonly PartyLooks _looks;

        public PartyState(int capacity, PartyMode mode = PartyMode.QuickPlay)
            : this(capacity, mode, PartyLooks.Default)
        {
        }

        public PartyState(int capacity, PartyMode mode, PartyLooks looks)
        {
            _capacity = capacity;
            _looks = looks;
            Mode = mode;
        }

        public IReadOnlyList<PartyMember> Members => _members;

        public IReadOnlyList<PartyBot> Bots => _bots;

        public PartyMode Mode { get; private set; }

        public bool IsSearching { get; private set; }

        public int Count => _members.Count;

        public int SlotCount => _capacity;

        public int ColorCount => _looks.ColorCount;

        public int CritterCount => _looks.CritterCount;

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

        public static PartyState Restore(int capacity, PartyMode mode, IEnumerable<PartyMember> members, bool isSearching = false, IEnumerable<PartyBot> bots = null)
        {
            return Restore(capacity, mode, PartyLooks.Default, members, isSearching, bots);
        }

        public static PartyState Restore(int capacity, PartyMode mode, PartyLooks looks, IEnumerable<PartyMember> members, bool isSearching = false, IEnumerable<PartyBot> bots = null)
        {
            var party = new PartyState(capacity, mode, looks) { IsSearching = isSearching };
            if (bots != null)
            {
                foreach (var bot in bots)
                {
                    if (bot.Slot >= 0 && bot.Slot < capacity && party.IndexOfBot(bot.Slot) < 0)
                    {
                        party._bots.Add(bot);
                    }
                }
            }

            foreach (var member in members)
            {
                if (party.IsFull || party.Contains(member.Id))
                {
                    continue;
                }

                var slot = member.Slot >= 0 && member.Slot < capacity && party.IndexOfMemberInSlot(member.Slot) < 0 ? member.Slot : party.ClaimSlot();
                party._bots.RemoveAll(bot => bot.Slot == slot);
                party._members.Add(member.WithSlot(slot));
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

        public bool TryGetMember(int id, out PartyMember member)
        {
            var index = IndexOf(id);
            member = index >= 0 ? _members[index] : default;
            return index >= 0;
        }

        public bool TryGetBot(int slot, out PartyBot bot)
        {
            var index = IndexOfBot(slot);
            bot = index >= 0 ? _bots[index] : default;
            return index >= 0;
        }

        public bool TryGetMemberInSlot(int slot, out PartyMember member)
        {
            var index = IndexOfMemberInSlot(slot);
            member = index >= 0 ? _members[index] : default;
            return index >= 0;
        }

        public bool IsColorTaken(int color, int byAnyoneBut)
        {
            foreach (var member in _members)
            {
                if (member.Id != byAnyoneBut && member.Color == color)
                {
                    return true;
                }
            }

            return false;
        }

        public PartyJoinOutcome Join(int id, string nickname, int critter = 0)
        {
            if (Contains(id))
            {
                return PartyJoinOutcome.AlreadyMember;
            }

            if (IsSearching)
            {
                return PartyJoinOutcome.Guest;
            }

            if (IsFull)
            {
                return PartyJoinOutcome.Full;
            }

            var slot = ClaimSlot();
            _bots.RemoveAll(bot => bot.Slot == slot);
            _members.Add(new PartyMember(id, nickname, false, slot, FirstFreeColor(), ValidCritter(critter) ? critter : 0));
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

        public bool StartSearch(int requesterId)
        {
            if (!IsLeader(requesterId) || !CanStart)
            {
                return false;
            }

            IsSearching = true;
            return true;
        }

        public void StopSearch(int memberId)
        {
            if (!Contains(memberId))
            {
                return;
            }

            IsSearching = false;
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

        public bool SetBot(int requesterId, int slot, BotDifficulty difficulty)
        {
            if (!IsLeader(requesterId) || slot < 0 || slot >= _capacity || IndexOfMemberInSlot(slot) >= 0)
            {
                return false;
            }

            var index = IndexOfBot(slot);
            if (index >= 0)
            {
                _bots[index] = new PartyBot(slot, difficulty);
            }
            else
            {
                _bots.Add(new PartyBot(slot, difficulty));
            }

            return true;
        }

        public bool RemoveBot(int requesterId, int slot)
        {
            var index = IndexOfBot(slot);
            if (!IsLeader(requesterId) || index < 0)
            {
                return false;
            }

            _bots.RemoveAt(index);
            return true;
        }

        public bool PickColor(int memberId, int color)
        {
            var index = IndexOf(memberId);
            if (index < 0 || color < 0 || color >= _looks.ColorCount || IsColorTaken(color, memberId))
            {
                return false;
            }

            _members[index] = _members[index].WithColor(color);
            return true;
        }

        public bool PickCritter(int memberId, int critter)
        {
            var index = IndexOf(memberId);
            if (index < 0 || !ValidCritter(critter))
            {
                return false;
            }

            _members[index] = _members[index].WithCritter(critter);
            return true;
        }

        public CustomGameSetup CustomGameSetup()
        {
            return CustomGameSetup(SoloId, SoloId);
        }

        public CustomGameSetup CustomGameSetup(int memberId, int asPlayerId)
        {
            var slots = new List<CustomGameSlot>(_capacity);
            for (var slot = 0; slot < _capacity; slot++)
            {
                if (TryGetMemberInSlot(slot, out var member))
                {
                    slots.Add(CustomGameSlot.Player(member.Id == memberId ? asPlayerId : member.Id, member.Color, member.Critter));
                }
                else if (TryGetBot(slot, out var bot))
                {
                    slots.Add(CustomGameSlot.Bot(bot.Difficulty));
                }
            }

            return new CustomGameSetup(slots);
        }

        private bool ValidCritter(int critter)
        {
            return critter >= 0 && critter < _looks.CritterCount;
        }

        private int ClaimSlot()
        {
            for (var slot = 0; slot < _capacity; slot++)
            {
                if (IndexOfMemberInSlot(slot) < 0 && IndexOfBot(slot) < 0)
                {
                    return slot;
                }
            }

            for (var slot = _capacity - 1; slot >= 0; slot--)
            {
                if (IndexOfMemberInSlot(slot) < 0)
                {
                    return slot;
                }
            }

            return -1;
        }

        private int FirstFreeColor()
        {
            for (var color = 0; color < _looks.ColorCount; color++)
            {
                if (!IsColorTaken(color, int.MinValue))
                {
                    return color;
                }
            }

            return 0;
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

        private int IndexOfMemberInSlot(int slot)
        {
            for (var i = 0; i < _members.Count; i++)
            {
                if (_members[i].Slot == slot)
                {
                    return i;
                }
            }

            return -1;
        }

        private int IndexOfBot(int slot)
        {
            for (var i = 0; i < _bots.Count; i++)
            {
                if (_bots[i].Slot == slot)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
