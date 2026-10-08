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
        private int _shownPlayers = -1;
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
            if (InParty || _matchmaker.Stage != LobbyStage.Gathering)
            {
                return;
            }

            ShowPlayers(_matchmaker.PlayerCount);
            ShowSearchTimer();
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
            var idle = !InParty && _matchmaker.Stage == LobbyStage.Idle;
            _playMenu.Show(idle, _party.Failure ?? _matchmaker.Failure);
            _lobbyPanel.SetVisible(!idle);
            if (idle)
            {
                return;
            }

            _shownPlayers = -1;
            _shownSeconds = -1;
            if (InParty)
            {
                RefreshParty();
            }
            else
            {
                RefreshSearch();
            }
        }

        private void RefreshParty()
        {
            var connecting = _party.Stage == PartyStage.Connecting;
            var starting = _matchmaker.Stage == LobbyStage.Starting;
            _lobbyPanel.ShowTitle(LobbyText.PartyTitle(connecting, _party.Code), !starting);
            _lobbyPanel.ShowStatus(starting ? LobbyText.PartyStarting() : LobbyText.PartyMembers(_party.Party));
            _lobbyPanel.ShowAction(!connecting && !starting, LobbyText.PartyAction(_party.IsLeader, _party.IsReady), !_party.IsLeader || _party.CanStart);
            ShowPlayers(connecting ? -1 : _party.Party.Count);
        }

        private void RefreshSearch()
        {
            var stage = _matchmaker.Stage;
            _lobbyPanel.ShowTitle(LobbyText.Title(stage), stage != LobbyStage.Starting);
            _lobbyPanel.ShowStatus(LobbyText.Status(stage, _matchmaker.SecondsLeft));
            _lobbyPanel.ShowAction(false, string.Empty, false);
            ShowPlayers(stage == LobbyStage.Connecting ? -1 : _matchmaker.PlayerCount);
        }

        private void ShowPlayers(int players)
        {
            if (players == _shownPlayers)
            {
                return;
            }

            _shownPlayers = players;
            _lobbyPanel.ShowPlayers(players < 0 ? string.Empty : LobbyText.Players(players, _matchmaker.MaxSlots));
        }

        private void ShowSearchTimer()
        {
            var seconds = LobbyText.WholeSeconds(_matchmaker.SecondsLeft);
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            _lobbyPanel.ShowStatus(LobbyText.SearchTimer(seconds));
        }

        private void OnQuickPlayRequested()
        {
            _matchmaker.QuickPlayAsync(_lifetime.Token).Forget();
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
            if (_party.IsLeader)
            {
                _matchmaker.StartPartyMatch();
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
            else
            {
                _matchmaker.LeaveAsync().Forget();
            }
        }
    }
}
