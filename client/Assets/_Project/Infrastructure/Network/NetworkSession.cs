using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Object = UnityEngine.Object;

namespace PlowParty.Infrastructure.Network
{
    public sealed class NetworkSession : IDisposable
    {
        private readonly NetworkRunnerEvents _sessionEvents = new NetworkRunnerEvents();
        private NetworkRunner _runner;
        private ResolverNetworkObjectProvider _provider;
        private IObjectResolver _scopeResolver;
        private NetworkRunnerEvents _scopeEvents;
        private CancellationTokenSource _startCancellation;
        private bool _replayJoinsOnSceneLoad;
        private bool _starting;
        private bool _leaving;

        public NetworkSession()
        {
            _sessionEvents.SceneLoadDone += OnSceneLoadDone;
            _sessionEvents.ShutDown += OnShutDown;
        }

        public event Action<SessionEnd> Ended;

        public bool IsRunning => _runner != null && _runner.IsRunning;

        public bool IsHost => IsRunning && _runner.IsServer;

        public NetworkRunner Runner => _runner;

        public int PlayerCount => IsRunning && _runner.SessionInfo.IsValid ? _runner.SessionInfo.PlayerCount : 0;

        public void Bind(IObjectResolver resolver, NetworkRunnerEvents events)
        {
            if (_scopeEvents != null && _runner != null)
            {
                _runner.RemoveCallbacks(_scopeEvents);
            }

            _scopeResolver = resolver;
            _scopeEvents = events;
            if (_provider != null)
            {
                _provider.Use(resolver);
            }

            if (_runner != null)
            {
                _runner.AddCallbacks(events);
                _replayJoinsOnSceneLoad = _runner.IsRunning;
            }
        }

        public void Unbind(NetworkRunnerEvents events)
        {
            if (_scopeEvents != events)
            {
                return;
            }

            if (_runner != null)
            {
                _runner.RemoveCallbacks(events);
            }

            _scopeEvents = null;
            _scopeResolver = null;
            _replayJoinsOnSceneLoad = false;
        }

        public async UniTask<SessionStartOutcome> StartAsync(SessionStart start, CancellationToken cancellationToken)
        {
            if (_runner != null)
            {
                await LeaveAsync();
            }

            CreateRunner();
            var args = new StartGameArgs
            {
                GameMode = start.Mode,
                SessionName = start.SessionName,
                SessionProperties = start.Properties,
                ConnectionToken = start.ConnectionToken,
                SceneManager = _runner.GetComponent<NetworkSceneManagerDefault>(),
                ObjectProvider = _provider,
                StartGameCancellationToken = _startCancellation.Token,
                IsVisible = start.IsVisible,
            };
            if (start.MaxPlayers > 0)
            {
                args.PlayerCount = start.MaxPlayers;
            }

            if (start.IncludeActiveScene)
            {
                var scene = new NetworkSceneInfo();
                scene.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), LoadSceneMode.Additive);
                args.Scene = scene;
            }

            _starting = true;
            StartGameResult result;
            using (cancellationToken.Register(_startCancellation.Cancel))
            {
                result = await _runner.StartGame(args).AsUniTask();
            }

            _starting = false;
            if (result.Ok)
            {
                return SessionStartOutcome.Started;
            }

            Debug.LogWarning($"Fusion session failed to start: {result.ShutdownReason} {result.ErrorMessage}");
            DestroyRunner();
            return OutcomeOf(result.ShutdownReason);
        }

        public bool TryGetProperty(string key, out SessionProperty value)
        {
            value = default;
            return IsRunning && _runner.SessionInfo.IsValid && _runner.SessionInfo.Properties.TryGetValue(key, out value);
        }

        public void Close()
        {
            if (!IsHost)
            {
                return;
            }

            _runner.SessionInfo.IsOpen = false;
            _runner.SessionInfo.IsVisible = false;
        }

        public void OpenAsLobby(Dictionary<string, SessionProperty> properties)
        {
            if (!IsHost)
            {
                return;
            }

            _runner.SessionInfo.UpdateCustomProperties(properties);
            _runner.SessionInfo.IsOpen = true;
            _runner.SessionInfo.IsVisible = true;
        }

        public void Hide(Dictionary<string, SessionProperty> properties)
        {
            if (!IsHost)
            {
                return;
            }

            _runner.SessionInfo.IsVisible = false;
            _runner.SessionInfo.UpdateCustomProperties(properties);
        }

        public void LoadScene(string sceneName)
        {
            if (IsHost)
            {
                _runner.LoadScene(sceneName);
            }
        }

        public async UniTask LeaveAsync()
        {
            if (_runner == null)
            {
                return;
            }

            if (_starting)
            {
                _startCancellation.Cancel();
                return;
            }

            _leaving = true;
            await _runner.Shutdown().AsUniTask();
            ReleaseRunner();
            _leaving = false;
        }

        public void Dispose()
        {
            if (_runner != null)
            {
                _leaving = true;
                _runner.Shutdown();
            }
        }

        private void CreateRunner()
        {
            var runnerObject = new GameObject(nameof(NetworkRunner));
            Object.DontDestroyOnLoad(runnerObject);
            _runner = runnerObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            runnerObject.AddComponent<NetworkSceneManagerDefault>();
            _provider = runnerObject.AddComponent<ResolverNetworkObjectProvider>();
            _provider.Use(_scopeResolver);
            _runner.AddCallbacks(_sessionEvents);
            _startCancellation = new CancellationTokenSource();
            if (_scopeEvents != null)
            {
                _runner.AddCallbacks(_scopeEvents);
            }

            _replayJoinsOnSceneLoad = false;
        }

        private void DestroyRunner()
        {
            if (_runner != null)
            {
                Object.Destroy(_runner.gameObject);
            }

            ReleaseRunner();
        }

        private void ReleaseRunner()
        {
            _runner = null;
            _provider = null;
            _startCancellation?.Dispose();
            _startCancellation = null;
        }

        private void OnSceneLoadDone(NetworkRunner runner)
        {
            if (!_replayJoinsOnSceneLoad || _scopeEvents == null)
            {
                return;
            }

            _replayJoinsOnSceneLoad = false;
            INetworkRunnerCallbacks callbacks = _scopeEvents;
            foreach (var player in runner.ActivePlayers)
            {
                callbacks.OnPlayerJoined(runner, player);
            }
        }

        private void OnShutDown(NetworkRunner runner, ShutdownReason reason)
        {
            if (runner != _runner || _starting)
            {
                return;
            }

            var end = _leaving ? SessionEnd.Left : SessionEnd.Lost;
            ReleaseRunner();
            Debug.Log($"Fusion session ended: {end} ({reason})");
            Ended?.Invoke(end);
        }

        private static SessionStartOutcome OutcomeOf(ShutdownReason reason)
        {
            switch (reason)
            {
                case ShutdownReason.GameIsFull:
                    return SessionStartOutcome.Full;
                case ShutdownReason.GameNotFound:
                    return SessionStartOutcome.NotFound;
                case ShutdownReason.GameIdAlreadyExists:
                    return SessionStartOutcome.NameTaken;
                case ShutdownReason.ConnectionRefused:
                case ShutdownReason.GameClosed:
                    return SessionStartOutcome.Refused;
                default:
                    return SessionStartOutcome.Failed;
            }
        }
    }
}
