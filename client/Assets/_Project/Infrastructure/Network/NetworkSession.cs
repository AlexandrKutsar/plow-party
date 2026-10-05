using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Infrastructure.Network
{
    public sealed class NetworkSession : INetworkSession, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly NetworkRunnerEvents _events;
        private NetworkRunner _runner;

        public NetworkSession(IObjectResolver resolver, NetworkRunnerEvents events)
        {
            _resolver = resolver;
            _events = events;
        }

        public async UniTask StartHostOrClientAsync(string sessionName, CancellationToken cancellationToken)
        {
            var runnerObject = new GameObject(nameof(NetworkRunner));
            _runner = runnerObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            var sceneManager = runnerObject.AddComponent<NetworkSceneManagerDefault>();
            var objectProvider = runnerObject.AddComponent<ResolverNetworkObjectProvider>();
            _resolver.InjectGameObject(runnerObject);
            _runner.AddCallbacks(_events);

            var scene = new NetworkSceneInfo();
            scene.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), LoadSceneMode.Additive);

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = sessionName,
                Scene = scene,
                SceneManager = sceneManager,
                ObjectProvider = objectProvider,
            }).AsUniTask().AttachExternalCancellation(cancellationToken);

            if (!result.Ok)
            {
                throw new InvalidOperationException($"Fusion session failed to start: {result.ShutdownReason} {result.ErrorMessage}");
            }
        }

        public void Dispose()
        {
            if (_runner != null)
            {
                _runner.Shutdown();
            }
        }
    }
}
