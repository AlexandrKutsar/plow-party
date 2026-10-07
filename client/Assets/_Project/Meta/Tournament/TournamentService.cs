using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Backend;
using PlowParty.Meta.Account;
using PlowParty.Meta.Tournament.Api;
using PlowParty.Meta.Tournament.Config;
using PlowParty.Meta.Tournament.Simulation;

namespace PlowParty.Meta.Tournament
{
    public sealed class TournamentService
    {
        private readonly AccountService _account;
        private readonly TournamentConfig _config;

        public TournamentService(AccountService account, TournamentConfig config)
        {
            _account = account;
            _config = config;
        }

        public async UniTask<TournamentBoard> LoadAsync(CancellationToken cancellationToken)
        {
            var top = await _account.SendAsync(BackendRequest.Get($"/tournament/leaderboard?limit={_config.TopCount}"), cancellationToken);
            if (!top.IsOk)
            {
                return null;
            }

            var aroundMe = await _account.SendAsync(BackendRequest.Get($"/tournament/leaderboard/me?radius={_config.AroundMeRadius}"), cancellationToken);
            var medals = await _account.SendAsync(BackendRequest.Get($"/tournament/medals?account_id={_account.AccountId}"), cancellationToken);
            if (!aroundMe.IsOk || !medals.IsOk)
            {
                return null;
            }

            return TournamentMapping.ToBoard(
                top.Read<LeaderboardResponse>(),
                aroundMe.Read<LeaderboardResponse>(),
                medals.Read<MedalsResponse>(),
                _account.AccountId);
        }
    }
}
