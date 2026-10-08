using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlowParty.Infrastructure.Network
{
    public sealed class SessionListFeed : IDisposable
    {
        private readonly NetworkRunnerEvents _events = new NetworkRunnerEvents();
        private readonly List<SessionInfo> _sessions = new List<SessionInfo>();
        private NetworkRunner _runner;
        private UniTaskCompletionSource _joining;

        public SessionListFeed()
        {
            _events.SessionListUpdated += OnSessionListUpdated;
        }

        public IReadOnlyList<SessionInfo> Sessions => _sessions;

        public int Version { get; private set; }

        public bool IsJoined => _runner != null && _joining == null;

        public async UniTask<bool> JoinAsync(CancellationToken cancellationToken)
        {
            if (_joining != null)
            {
                await _joining.Task.AttachExternalCancellation(cancellationToken);
                return IsJoined;
            }

            if (_runner != null)
            {
                return true;
            }

            _joining = new UniTaskCompletionSource();
            var runnerObject = new GameObject(nameof(SessionListFeed));
            Object.DontDestroyOnLoad(runnerObject);
            _runner = runnerObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(_events);
            var result = await _runner.JoinSessionLobby(SessionLobby.ClientServer, cancellationToken: cancellationToken).AsUniTask().SuppressCancellationThrow();
            var joined = !result.IsCanceled && result.Result.Ok;
            if (!joined)
            {
                Debug.LogWarning($"[Session] Session list unavailable: {(result.IsCanceled ? "cancelled" : result.Result.ShutdownReason.ToString())}");
                Destroy();
            }

            var joining = _joining;
            _joining = null;
            joining.TrySetResult();
            return joined;
        }

        public async UniTask<bool> WaitForUpdateAsync(int seenVersion, float timeoutSeconds, CancellationToken cancellationToken)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (Version == seenVersion && Time.realtimeSinceStartup < deadline)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            return Version != seenVersion;
        }

        public void Leave()
        {
            if (_runner == null || _joining != null)
            {
                return;
            }

            _runner.Shutdown().AsUniTask().Forget();
            _runner = null;
            _sessions.Clear();
        }

        public void Dispose()
        {
            Leave();
        }

        private void Destroy()
        {
            if (_runner != null)
            {
                Object.Destroy(_runner.gameObject);
            }

            _runner = null;
            _sessions.Clear();
        }

        private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessions)
        {
            if (runner != _runner)
            {
                return;
            }

            _sessions.Clear();
            _sessions.AddRange(sessions);
            Version++;
        }
    }
}
