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

        public UniTask StartAsync(CancellationToken cancellation)
        {
            var sessionName = DevSessionName.For(
                Application.dataPath,
                Application.isEditor,
                Environment.GetEnvironmentVariable(DevSessionName.OverrideVariable));
            return _session.StartHostOrClientAsync(sessionName, cancellation);
        }
    }
}
