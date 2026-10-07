using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Account;
using UnityEngine;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchSceneQuickStart : IAsyncStartable
    {
        private readonly NetworkSession _session;
        private readonly AccountService _account;

        public MatchSceneQuickStart(NetworkSession session, AccountService account)
        {
            _session = session;
            _account = account;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            if (_session.IsRunning)
            {
                return;
            }

            await _account.EnsureSignedInAsync(cancellation);
            var outcome = await _session.StartAsync(new SessionStart
            {
                SessionName = DevSessionName.For(
                    Application.dataPath,
                    Application.isEditor,
                    Environment.GetEnvironmentVariable(DevSessionName.OverrideVariable)),
                ConnectionToken = _account.ToParticipantToken().ToBytes(),
                IncludeActiveScene = true,
            }, cancellation);
            if (outcome != SessionStartOutcome.Started)
            {
                throw new InvalidOperationException($"Dev session failed to start: {outcome}");
            }
        }
    }
}
