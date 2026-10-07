using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Meta.Account;
using PlowParty.Meta.Tournament.View;
using VContainer.Unity;

namespace PlowParty.Meta.Tournament
{
    public sealed class TournamentPresenter : IStartable, IDisposable
    {
        private readonly TournamentService _tournament;
        private readonly AccountService _account;
        private readonly TournamentView _view;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private bool _loading;

        public TournamentPresenter(TournamentService tournament, AccountService account, TournamentView view)
        {
            _tournament = tournament;
            _account = account;
            _view = view;
        }

        public void Start()
        {
            _view.RefreshRequested += OnRefreshRequested;
            LoadAsync().Forget();
        }

        public void Dispose()
        {
            _view.RefreshRequested -= OnRefreshRequested;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void OnRefreshRequested()
        {
            LoadAsync().Forget();
        }

        private async UniTask LoadAsync()
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            _view.ShowLoading();
            await _account.EnsureSignedInAsync(_lifetime.Token);
            var board = await _tournament.LoadAsync(_lifetime.Token);
            _view.ShowBoard(board);
            _loading = false;
        }
    }
}
