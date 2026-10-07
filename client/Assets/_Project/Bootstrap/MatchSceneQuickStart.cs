using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Account;
using PlowParty.Meta.Session;
using UnityEngine;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchSceneQuickStart : IAsyncStartable
    {
        private readonly NetworkSession _session;
        private readonly AccountService _account;
        private readonly SessionExit _exit;

        public MatchSceneQuickStart(NetworkSession session, AccountService account, SessionExit exit)
        {
            _session = session;
            _account = account;
            _exit = exit;
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
            if (outcome == SessionStartOutcome.Started)
            {
                return;
            }

            Debug.LogWarning($"Dev session failed to start: {outcome}; returning to the Menu");
            _exit.LeaveToMenu(outcome == SessionStartOutcome.Refused ? Matchmaker.MatchStartedText : Matchmaker.ConnectFailedText);
        }
    }
}
