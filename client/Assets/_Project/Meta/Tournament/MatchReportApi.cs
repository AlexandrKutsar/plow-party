using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Backend;
using PlowParty.Meta.Account;
using PlowParty.Meta.Tournament.Api;
using UnityEngine;

namespace PlowParty.Meta.Tournament
{
    public sealed class MatchReportApi : IDisposable
    {
        private readonly AccountService _account;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public MatchReportApi(AccountService account)
        {
            _account = account;
        }

        public string AccountId => _account.AccountId;

        public async UniTask<string> RegisterAsync(RegisterMatchRequest roster)
        {
            var response = await _account.SendAsync(BackendRequest.Post("/matches", roster), _lifetime.Token);
            if (!response.IsOk)
            {
                Debug.LogWarning($"Match not registered: {response.Outcome} {response.ValidationMessage()}");
                return null;
            }

            var match = response.Read<MatchResponse>();
            Debug.Log($"Match {match.MatchId} registered with {roster.Roster.Count} Slots");
            return match.MatchId;
        }

        public async UniTask<bool> ConfirmAsync(string matchId)
        {
            var response = await _account.SendAsync(BackendRequest.Post($"/matches/{matchId}/confirm"), _lifetime.Token);
            Debug.Log($"Match {matchId} confirmation: {response.Outcome} {response.StatusCode}");
            return response.IsOk;
        }

        public async UniTask VoteAsync(string matchId, VoteRequest vote)
        {
            var response = await _account.SendAsync(BackendRequest.Post($"/matches/{matchId}/votes", vote), _lifetime.Token);
            if (!response.IsOk)
            {
                Debug.LogWarning($"Vote for Match {matchId} refused: {response.Outcome} {response.StatusCode} {response.ValidationMessage()}");
                return;
            }

            var match = response.Read<MatchResponse>();
            Debug.Log($"Vote for Match {matchId} submitted: {match.Status} {match.RejectionReason}");
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
    }
}
