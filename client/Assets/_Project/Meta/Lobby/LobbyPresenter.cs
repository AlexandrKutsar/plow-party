using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Lobby.View;
using PlowParty.Meta.Session;
using PlowParty.Meta.Session.Simulation;
using VContainer.Unity;

namespace PlowParty.Meta.Lobby
{
    public sealed class LobbyPresenter : IStartable, ITickable, IDisposable
    {
        private readonly Matchmaker _matchmaker;
        private readonly PlayMenuView _playMenu;
        private readonly LobbyPanelView _lobbyPanel;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private int _shownPlayers = -1;
        private int _shownSeconds = -1;

        public LobbyPresenter(Matchmaker matchmaker, PlayMenuView playMenu, LobbyPanelView lobbyPanel)
        {
            _matchmaker = matchmaker;
            _playMenu = playMenu;
            _lobbyPanel = lobbyPanel;
        }

        public void Start()
        {
            _matchmaker.Changed += Refresh;
            _playMenu.QuickPlayRequested += OnQuickPlayRequested;
            _playMenu.CreateRoomRequested += OnCreateRoomRequested;
            _playMenu.JoinRequested += OnJoinRequested;
            _lobbyPanel.StartRequested += _matchmaker.RequestStart;
            _lobbyPanel.LeaveRequested += OnLeaveRequested;
            Refresh();
        }

        public void Tick()
        {
            if (_matchmaker.Stage != LobbyStage.Gathering)
            {
                return;
            }

            ShowPlayers();
            if (_matchmaker.Mode == LobbyMode.QuickPlay)
            {
                ShowSearchTimer();
            }
        }

        public void Dispose()
        {
            _matchmaker.Changed -= Refresh;
            _playMenu.QuickPlayRequested -= OnQuickPlayRequested;
            _playMenu.CreateRoomRequested -= OnCreateRoomRequested;
            _playMenu.JoinRequested -= OnJoinRequested;
            _lobbyPanel.StartRequested -= _matchmaker.RequestStart;
            _lobbyPanel.LeaveRequested -= OnLeaveRequested;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Refresh()
        {
            var stage = _matchmaker.Stage;
            var idle = stage == LobbyStage.Idle;
            _playMenu.Show(idle, _matchmaker.Failure);
            _lobbyPanel.SetVisible(!idle);
            if (idle)
            {
                return;
            }

            var gathering = stage == LobbyStage.Gathering;
            var canStart = gathering && _matchmaker.IsHost && _matchmaker.Mode == LobbyMode.Room;
            _lobbyPanel.ShowTitle(LobbyText.Title(stage, _matchmaker.Mode, _matchmaker.RoomCode), canStart, stage != LobbyStage.Starting);
            _lobbyPanel.ShowStatus(LobbyText.Status(stage, _matchmaker.Mode, _matchmaker.IsHost, _matchmaker.SecondsLeft));
            _shownPlayers = -1;
            _shownSeconds = -1;
            ShowPlayers();
        }

        private void ShowPlayers()
        {
            var players = _matchmaker.PlayerCount;
            if (players == _shownPlayers)
            {
                return;
            }

            _shownPlayers = players;
            _lobbyPanel.ShowPlayers(_matchmaker.Stage == LobbyStage.Connecting ? string.Empty : LobbyText.Players(players, _matchmaker.MaxSlots));
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

        private void OnCreateRoomRequested()
        {
            _matchmaker.CreateRoomAsync(_lifetime.Token).Forget();
        }

        private void OnJoinRequested(string code)
        {
            _matchmaker.JoinRoomAsync(code, _lifetime.Token).Forget();
        }

        private void OnLeaveRequested()
        {
            _matchmaker.LeaveAsync().Forget();
        }
    }
}
