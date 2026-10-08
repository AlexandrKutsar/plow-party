using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Lobby.View;
using PlowParty.Meta.Party;
using PlowParty.Meta.Session;
using VContainer.Unity;

namespace PlowParty.Meta.Lobby
{
    public sealed class LobbyPresenter : IStartable, ITickable, IDisposable
    {
        private readonly Matchmaker _matchmaker;
        private readonly PartyService _party;
        private readonly PlayMenuView _playMenu;
        private readonly LobbyPanelView _lobbyPanel;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private int _shownSeconds = -1;

        public LobbyPresenter(Matchmaker matchmaker, PartyService party, PlayMenuView playMenu, LobbyPanelView lobbyPanel)
        {
            _matchmaker = matchmaker;
            _party = party;
            _playMenu = playMenu;
            _lobbyPanel = lobbyPanel;
        }

        private bool InParty => _party.Stage != PartyStage.None;

        public void Start()
        {
            _matchmaker.Changed += Refresh;
            _party.Changed += Refresh;
            _playMenu.QuickPlayRequested += OnQuickPlayRequested;
            _playMenu.CreatePartyRequested += OnCreatePartyRequested;
            _playMenu.JoinRequested += OnJoinRequested;
            _lobbyPanel.ActionRequested += OnActionRequested;
            _lobbyPanel.LeaveRequested += OnLeaveRequested;
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
            _playMenu.QuickPlayRequested -= OnQuickPlayRequested;
            _playMenu.CreatePartyRequested -= OnCreatePartyRequested;
            _playMenu.JoinRequested -= OnJoinRequested;
            _lobbyPanel.ActionRequested -= OnActionRequested;
            _lobbyPanel.LeaveRequested -= OnLeaveRequested;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Refresh()
        {
            var searching = _matchmaker.Stage != LobbyStage.Idle;
            var idle = !InParty && !searching;
            _playMenu.Show(idle, _party.Failure ?? _matchmaker.Failure);
            _lobbyPanel.SetVisible(!idle);
            if (idle)
            {
                return;
            }

            _shownSeconds = -1;
            if (searching)
            {
                RefreshSearch();
            }
            else
            {
                RefreshParty();
            }
        }

        private void RefreshParty()
        {
            var connecting = _party.Stage != PartyStage.InParty;
            _lobbyPanel.ShowTitle(LobbyText.PartyTitle(connecting, _party.Code), true);
            _lobbyPanel.ShowStatus(LobbyText.PartyMembers(_party.Party));
            _lobbyPanel.ShowAction(!connecting, LobbyText.PartyAction(_party.IsLeader, _party.IsReady), !_party.IsLeader || _matchmaker.CanSearch);
            _lobbyPanel.ShowPlayers(connecting ? string.Empty : LobbyText.PartySize(_party.Party.Count, _matchmaker.MaxSlots));
        }

        private void RefreshSearch()
        {
            _lobbyPanel.ShowTitle(LobbyText.SearchTitle(), false);
            _lobbyPanel.ShowStatus(LobbyText.SearchStatus(_matchmaker.Stage, _matchmaker.SearchSeconds));
            _lobbyPanel.ShowAction(_matchmaker.CanStopSearch, LobbyText.StopSearch(), true);
            _lobbyPanel.ShowPlayers(string.Empty);
        }

        private void ShowStopwatch()
        {
            var seconds = LobbyText.WholeSeconds(_matchmaker.SearchSeconds);
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            _lobbyPanel.ShowStatus(LobbyText.Stopwatch(_matchmaker.SearchSeconds));
        }

        private void OnQuickPlayRequested()
        {
            _matchmaker.Search();
        }

        private void OnCreatePartyRequested()
        {
            _party.CreateAsync(_lifetime.Token).Forget();
        }

        private void OnJoinRequested(string code)
        {
            _party.JoinAsync(code, _lifetime.Token).Forget();
        }

        private void OnActionRequested()
        {
            if (_matchmaker.CanStopSearch)
            {
                _matchmaker.StopSearch();
            }
            else if (_party.IsLeader)
            {
                _matchmaker.Search();
            }
            else
            {
                _party.SetReady(!_party.IsReady);
            }
        }

        private void OnLeaveRequested()
        {
            if (InParty)
            {
                _party.LeaveAsync().Forget();
            }
        }
    }
}
