using System.Collections.Generic;
using PlowParty.Meta.Party;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Lobby.Simulation
{
    public sealed class PartyPanelState
    {
        private const bool CustomGameAvailable = false;

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

        public static PartyPanelState For(PartyStage stage, string code, PartyState party, int localId, bool canSearch)
        {
            var inParty = stage == PartyStage.InParty;
            var isLeader = inParty && party.IsLeader(localId);
            var isReady = inParty && IsMemberReady(party, localId);
            return new PartyPanelState
            {
                Title = LobbyText.PartyTitle(!inParty, code),
                Code = code,
                CanCopy = inParty && code != null,
                Rows = inParty ? RowsOf(party, isLeader) : new PartyRow[0],
                Mode = party.Mode,
                CanPickQuickPlay = isLeader,
                CanPickCustomGame = isLeader && CustomGameAvailable,
                ShowReady = inParty && !isLeader,
                IsReady = isReady,
                ReadyLabel = LobbyText.ReadyButton(isReady),
                ShowSearch = isLeader,
                CanSearch = isLeader && canSearch,
            };
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
