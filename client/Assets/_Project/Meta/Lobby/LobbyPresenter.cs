using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Lobby.View;
using PlowParty.Meta.Party;
using PlowParty.Meta.Session;
using PlowParty.Shared;
using VContainer.Unity;

namespace PlowParty.Meta.Lobby
{
    public sealed class LobbyPresenter : IStartable, ITickable, IDisposable
    {
        private readonly Matchmaker _matchmaker;
        private readonly PartyService _party;
        private readonly PlayMenuView _playMenu;
        private readonly PartyPanelView _partyPanel;
        private readonly SearchView _search;
        private readonly PodiumView _podium;
        private readonly MemberLooks _looks;
        private readonly List<MemberLook> _lineup = new List<MemberLook>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private int _shownSeconds = -1;

        public LobbyPresenter(
            Matchmaker matchmaker,
            PartyService party,
            PlayMenuView playMenu,
            PartyPanelView partyPanel,
            SearchView search,
            PodiumView podium,
            MemberLooks looks)
        {
            _matchmaker = matchmaker;
            _party = party;
            _playMenu = playMenu;
            _partyPanel = partyPanel;
            _search = search;
            _podium = podium;
            _looks = looks;
        }

        public void Start()
        {
            _matchmaker.Changed += Refresh;
            _party.Changed += Refresh;
            _playMenu.QuickPlayRequested += OnSearchRequested;
            _playMenu.CreatePartyRequested += OnCreatePartyRequested;
            _playMenu.JoinRequested += OnJoinRequested;
            _partyPanel.RemoveRequested += OnRemoveRequested;
            _partyPanel.ModeRequested += OnModeRequested;
            _partyPanel.ReadyRequested += OnReadyRequested;
            _partyPanel.SearchRequested += OnSearchRequested;
            _partyPanel.LeaveRequested += OnLeaveRequested;
            _search.StopRequested += OnStopRequested;
            Refresh();
        }

        public void Tick()
        {
            if (_matchmaker.Stage == LobbyStage.Searching)
            {
                ShowStopwatch();
            }
        }

        public void Dispose()
        {
            _matchmaker.Changed -= Refresh;
            _party.Changed -= Refresh;
            _playMenu.QuickPlayRequested -= OnSearchRequested;
            _playMenu.CreatePartyRequested -= OnCreatePartyRequested;
            _playMenu.JoinRequested -= OnJoinRequested;
            _partyPanel.RemoveRequested -= OnRemoveRequested;
            _partyPanel.ModeRequested -= OnModeRequested;
            _partyPanel.ReadyRequested -= OnReadyRequested;
            _partyPanel.SearchRequested -= OnSearchRequested;
            _partyPanel.LeaveRequested -= OnLeaveRequested;
            _search.StopRequested -= OnStopRequested;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Refresh()
        {
            var screen = LobbyScreens.For(_party.Stage, _matchmaker.Stage);
            _playMenu.Show(screen == LobbyScreen.Solo, _party.Failure ?? _matchmaker.Failure);
            _partyPanel.SetVisible(screen == LobbyScreen.Party);
            _search.SetVisible(screen == LobbyScreen.Search);
            if (screen == LobbyScreen.Party)
            {
                RefreshParty();
            }
            else if (screen == LobbyScreen.Search)
            {
                RefreshSearch();
            }

            RefreshPodium();
        }

        private void RefreshParty()
        {
            var state = PartyPanelState.For(_party.Stage, _party.Code, _party.Party, _party.LocalId, _matchmaker.CanSearch);
            _partyPanel.Show(state);
            for (var i = 0; i < _partyPanel.RowCount; i++)
            {
                if (i < state.Rows.Count)
                {
                    var row = state.Rows[i];
                    _partyPanel.ShowRow(i, row, _looks.LookOf(row.MemberId).Color);
                }
                else
                {
                    _partyPanel.HideRow(i);
                }
            }
        }

        private void RefreshSearch()
        {
            _shownSeconds = -1;
            _search.Show(LobbyText.SearchTitle(), LobbyText.StopSearch(), _matchmaker.CanStopSearch);
            _search.ShowStopwatch(LobbyText.SearchStatus(_matchmaker.Stage, _matchmaker.SearchSeconds));
        }

        private void RefreshPodium()
        {
            _lineup.Clear();
            foreach (var member in _party.Party.Members)
            {
                _lineup.Add(_looks.LookOf(member.Id));
            }

            if (_lineup.Count == 0)
            {
                _lineup.Add(_looks.LookOf(MemberLooks.SoloId));
            }

            _podium.Show(_lineup);
        }

        private void ShowStopwatch()
        {
            var seconds = LobbyText.WholeSeconds(_matchmaker.SearchSeconds);
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            _search.ShowStopwatch(LobbyText.Stopwatch(_matchmaker.SearchSeconds));
        }

        private void OnSearchRequested()
        {
            _matchmaker.Search();
        }

        private void OnStopRequested()
        {
            _matchmaker.StopSearch();
        }

        private void OnCreatePartyRequested()
        {
            _party.CreateAsync(_lifetime.Token).Forget();
        }

        private void OnJoinRequested(string code)
        {
            _party.JoinAsync(code, _lifetime.Token).Forget();
        }

        private void OnRemoveRequested(int memberId)
        {
            _party.Remove(memberId);
        }

        private void OnModeRequested(PartyMode mode)
        {
            _party.SetMode(mode);
        }

        private void OnReadyRequested()
        {
            _party.SetReady(!_party.IsReady);
        }

        private void OnLeaveRequested()
        {
            _party.LeaveAsync().Forget();
        }
    }
}
