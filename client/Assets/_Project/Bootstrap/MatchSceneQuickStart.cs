using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using UnityEngine;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchSceneQuickStart : IAsyncStartable
    {
        private readonly NetworkSession _session;

        public MatchSceneQuickStart(NetworkSession session)
        {
            _session = session;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var sessionName = DevSessionName.For(
                Application.dataPath,
                Application.isEditor,
                Environment.GetEnvironmentVariable(DevSessionName.OverrideVariable));
            try
            {
                await _session.StartHostOrClientAsync(sessionName, cancellation);
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogWarning($"Could not join the Match: {exception.Message}. The Match may have started already; Meta will route back to the Menu.");
            }
        }
    }
}
