using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Infrastructure.Session;
using VContainer.Unity;

namespace PlowParty.Meta.Session
{
    public sealed class SessionExit : IStartable, IDisposable
    {
        private readonly NetworkSession _session;
        private readonly SceneLoader _scenes;
        private readonly MatchLineupStore _lineups;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _leaving;

        public SessionExit(NetworkSession session, SceneLoader scenes, MatchLineupStore lineups)
        {
            _session = session;
            _scenes = scenes;
            _lineups = lineups;
        }

        public void Start()
        {
            _session.Ended += OnSessionEnded;
        }

        public void LeaveToMenu()
        {
            LeaveToMenuAsync().Forget();
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
                ReturnToMenuAsync().Forget();
            }
        }

        private UniTask ReturnToMenuAsync()
        {
            _lineups.Clear();
            return _scenes.LoadAsync(SceneNames.Menu, _lifetime.Token);
        }
    }
}
