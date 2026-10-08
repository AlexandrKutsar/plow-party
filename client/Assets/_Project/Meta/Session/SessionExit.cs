using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Infrastructure.Session;
using PlowParty.Meta.Session.Simulation;
using VContainer.Unity;

namespace PlowParty.Meta.Session
{
    public sealed class SessionExit : IStartable, IDisposable
    {
        private readonly NetworkSession _session;
        private readonly SceneLoader _scenes;
        private readonly MatchmakingResultStore _matchmakingResults;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _leaving;
        private string _notice;
        private MatchStanding _matchStanding;

        public SessionExit(NetworkSession session, SceneLoader scenes, MatchmakingResultStore matchmakingResults)
        {
            _session = session;
            _scenes = scenes;
            _matchmakingResults = matchmakingResults;
        }

        public void Start()
        {
            _session.Ended += OnSessionEnded;
        }

        public void LeaveToMenu()
        {
            LeaveToMenuAsync().Forget();
        }

        public void LeaveToMenu(string notice)
        {
            _notice = notice;
            LeaveToMenuAsync().Forget();
        }

        public void ObserveMatch(MatchStanding standing)
        {
            _matchStanding = standing;
        }

        public string TakeNotice()
        {
            var notice = _notice;
            _notice = null;
            return notice;
        }

        public void Dispose()
        {
            _session.Ended -= OnSessionEnded;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTask LeaveToMenuAsync()
        {
            if (_leaving)
            {
                return;
            }

            _leaving = true;
            await _session.LeaveAsync();
            await ReturnToMenuAsync();
            _leaving = false;
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (end == SessionEnd.Lost && _scenes.ActiveSceneName == SceneNames.Match)
            {
                _notice = SessionLossNotice.For(_matchStanding);
                ReturnToMenuAsync().Forget();
            }
        }

        private UniTask ReturnToMenuAsync()
        {
            _matchmakingResults.Clear();
            _matchStanding = MatchStanding.NotStarted;
            return _scenes.LoadAsync(SceneNames.Menu, _lifetime.Token);
        }
    }
}
