using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Meta.Account.View;
using VContainer.Unity;

namespace PlowParty.Meta.Account
{
    public sealed class NicknamePresenter : IStartable, IDisposable
    {
        private readonly AccountService _account;
        private readonly NicknameView _view;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public NicknamePresenter(AccountService account, NicknameView view)
        {
            _account = account;
            _view = view;
        }

        public void Start()
        {
            _account.Changed += Refresh;
            _view.EditRequested += OnEditRequested;
            _view.SaveRequested += OnSaveRequested;
            _view.CancelRequested += _view.CloseEditor;
            Refresh();
            _account.EnsureSignedInAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _account.Changed -= Refresh;
            _view.EditRequested -= OnEditRequested;
            _view.SaveRequested -= OnSaveRequested;
            _view.CancelRequested -= _view.CloseEditor;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Refresh()
        {
            _view.ShowAccount(_account.Nickname, _account.IsOnline);
        }

        private void OnEditRequested()
        {
            _view.OpenEditor(_account.Nickname);
        }

        private void OnSaveRequested(string nickname)
        {
            SaveAsync(nickname).Forget();
        }

        private async UniTask SaveAsync(string nickname)
        {
            _view.SetBusy(true);
            var result = await _account.RenameAsync(nickname, _lifetime.Token);
            _view.SetBusy(false);
            if (result.Renamed)
            {
                _view.CloseEditor();
            }
            else
            {
                _view.ShowError(result.Error);
            }
        }
    }
}
