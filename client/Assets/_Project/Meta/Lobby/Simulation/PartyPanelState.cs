using System.Collections.Generic;
using PlowParty.Meta.Party;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Simulation
{
    public sealed class PartyPanelState
    {
        private PartyPanelState()
        {
        }

        public string Title { get; private set; }

        public string Code { get; private set; }

        public bool CanCopy { get; private set; }

        public IReadOnlyList<PartyRow> Rows { get; private set; }

        public PartyMode Mode { get; private set; }

        public bool CanPickQuickPlay { get; private set; }

        public bool CanPickCustomGame { get; private set; }

        public bool ShowReady { get; private set; }

        public bool IsReady { get; private set; }

        public string ReadyLabel { get; private set; }

        public bool ShowSearch { get; private set; }

        public bool CanSearch { get; private set; }

        public string SearchLabel { get; private set; }

        public bool ShowLeave { get; private set; }

        public bool ShowSetup { get; private set; }

        public string NotCountedText { get; private set; }

        public IReadOnlyList<SetupSlotRow> Slots { get; private set; }

        public IReadOnlyList<ColorChoice> Colors { get; private set; }

        public bool CanPickLook { get; private set; }

        public int Critter { get; private set; }

        public string CritterName { get; private set; }

        public string Failure { get; private set; }

        public static PartyPanelState For(PartyStage stage, string code, PartyState party, int localId, bool canSearch, string failure = null)
        {
            var inParty = stage == PartyStage.InParty;
            var solo = stage == PartyStage.None;
            var isLeader = (inParty || solo) && party.IsLeader(localId);
            var isReady = inParty && IsMemberReady(party, localId);
            var showSetup = (inParty || solo) && party.Mode == PartyMode.CustomGame;
            var hasLocal = party.TryGetMember(localId, out var local);
            return new PartyPanelState
            {
                Title = solo ? LobbyText.CustomGameTitle() : LobbyText.PartyTitle(!inParty, code),
                Code = code,
                CanCopy = inParty && code != null,
                Rows = inParty ? RowsOf(party, isLeader) : new PartyRow[0],
                Mode = party.Mode,
                CanPickQuickPlay = isLeader,
                CanPickCustomGame = isLeader,
                ShowReady = inParty && !isLeader,
                IsReady = isReady,
                ReadyLabel = LobbyText.ReadyButton(isReady),
                ShowSearch = isLeader,
                CanSearch = isLeader && canSearch,
                SearchLabel = LobbyText.SearchButton(party.Mode),
                ShowLeave = !solo,
                ShowSetup = showSetup,
                NotCountedText = LobbyText.NotCounted(),
                Slots = showSetup ? SlotsOf(party, isLeader) : new SetupSlotRow[0],
                Colors = ColorsOf(party, localId),
                CanPickLook = showSetup && hasLocal,
                Critter = hasLocal ? local.Critter : 0,
                CritterName = hasLocal ? LobbyText.CritterName(local.Critter) : string.Empty,
                Failure = failure ?? string.Empty,
            };
        }

        public static BotDifficulty NextDifficulty(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Weak:
                    return BotDifficulty.Medium;
                case BotDifficulty.Medium:
                    return BotDifficulty.Strong;
                default:
                    return BotDifficulty.Weak;
            }
        }

        private static List<SetupSlotRow> SlotsOf(PartyState party, bool localIsLeader)
        {
            var slots = new List<SetupSlotRow>(party.SlotCount);
            for (var slot = 0; slot < party.SlotCount; slot++)
            {
                if (party.TryGetMemberInSlot(slot, out var member))
                {
                    var label = party.IsLeader(member.Id) ? member.Nickname : $"{member.Nickname} · {LobbyText.MemberReady(member.IsReady)}";
                    slots.Add(new SetupSlotRow(slot, label, member.Color, false, false));
                }
                else if (party.TryGetBot(slot, out var bot))
                {
                    slots.Add(new SetupSlotRow(slot, LobbyText.BotSlot(bot.Difficulty), -1, false, localIsLeader));
                }
                else
                {
                    slots.Add(new SetupSlotRow(slot, LobbyText.EmptySlot(), -1, localIsLeader, false));
                }
            }

            return slots;
        }

        private static List<ColorChoice> ColorsOf(PartyState party, int localId)
        {
            var hasLocal = party.TryGetMember(localId, out var local);
            var colors = new List<ColorChoice>(party.ColorCount);
            for (var color = 0; color < party.ColorCount; color++)
            {
                colors.Add(new ColorChoice(color, hasLocal && local.Color == color, party.IsColorTaken(color, localId)));
            }

            return colors;
        }

        private static List<PartyRow> RowsOf(PartyState party, bool localIsLeader)
        {
            var rows = new List<PartyRow>(party.Count);
            foreach (var member in party.Members)
            {
                var isLeader = party.IsLeader(member.Id);
                var readyText = isLeader ? string.Empty : LobbyText.MemberReady(member.IsReady);
                rows.Add(new PartyRow(member.Id, member.Nickname, isLeader, readyText, localIsLeader && !isLeader));
            }

            return rows;
        }

        private static bool IsMemberReady(PartyState party, int localId)
        {
            foreach (var member in party.Members)
            {
                if (member.Id == localId)
                {
                    return member.IsReady;
                }
            }

            return false;
        }
    }
}
