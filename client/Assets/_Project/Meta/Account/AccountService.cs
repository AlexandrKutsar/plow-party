using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Backend;
using PlowParty.Infrastructure.Storage;
using PlowParty.Meta.Account.Api;
using PlowParty.Meta.Account.Simulation;
using PlowParty.Shared;

namespace PlowParty.Meta.Account
{
    public sealed class AccountService
    {
        public const string GuestNickname = "Гость";
        public const string OfflineError = "Нет связи с сервером";
        public const string RejectedError = "Сервер не принял ник";

        private const string RecordName = "account";

        private readonly BackendClient _backend;
        private readonly LocalFileStore _store;
        private readonly AccountRecord _record;
        private bool _signInAttempted;
        private UniTask<bool> _signIn;

        public AccountService(BackendClient backend, LocalFileStore store)
        {
            _backend = backend;
            _store = store;
            _record = store.TryRead<AccountRecord>(RecordName, out var saved) ? saved : new AccountRecord();
            if (string.IsNullOrEmpty(_record.DeviceId))
            {
                _record.DeviceId = Guid.NewGuid().ToString();
                Save();
            }
        }

        public event Action Changed;

        public bool IsOnline { get; private set; }

        public string AccountId => _record.AccountId ?? string.Empty;

        public string Nickname => string.IsNullOrEmpty(_record.Nickname) ? GuestNickname : _record.Nickname;

        public ParticipantToken ToParticipantToken()
        {
            return new ParticipantToken(AccountId, Nickname);
        }

        public UniTask EnsureSignedInAsync(CancellationToken cancellationToken)
        {
            return _signInAttempted ? _signIn.AsUniTask() : SignInAsync(cancellationToken).AsUniTask();
        }

        public UniTask<bool> SignInAsync(CancellationToken cancellationToken)
        {
            if (!_signInAttempted || _signIn.Status != UniTaskStatus.Pending)
            {
                _signInAttempted = true;
                _signIn = LoginAsync(cancellationToken).Preserve();
            }

            return _signIn;
        }

        public async UniTask<RenameResult> RenameAsync(string nickname, CancellationToken cancellationToken)
        {
            var check = NicknameRules.Check(nickname);
            if (!check.IsValid)
            {
                return RenameResult.Failure(check.Error);
            }

            var response = await SendAsync(BackendRequest.Patch("/accounts/me", new RenameRequest { Nickname = check.Nickname }), cancellationToken);
            switch (response.Outcome)
            {
                case BackendOutcome.Ok:
                    _record.Nickname = response.Read<AccountResponse>().Nickname;
                    Save();
                    Changed?.Invoke();
                    return RenameResult.Success();
                case BackendOutcome.Rejected:
                    var message = response.ValidationMessage();
                    return RenameResult.Failure(string.IsNullOrEmpty(message) ? RejectedError : message);
                default:
                    return RenameResult.Failure(OfflineError);
            }
        }

        public async UniTask<BackendResponse> SendAsync(BackendRequest request, CancellationToken cancellationToken)
        {
            if (!IsOnline && !await SignInAsync(cancellationToken))
            {
                return new BackendResponse(BackendOutcome.Offline, 0, null);
            }

            var response = await _backend.SendAsync(request.WithAuthToken(_record.AuthToken), cancellationToken);
            if (response.Outcome == BackendOutcome.Unauthorized && await SignInAsync(cancellationToken))
            {
                response = await _backend.SendAsync(request.WithAuthToken(_record.AuthToken), cancellationToken);
            }

            if (response.Outcome == BackendOutcome.Offline)
            {
                SetOnline(false);
            }

            return response;
        }

        private async UniTask<bool> LoginAsync(CancellationToken cancellationToken)
        {
            var request = BackendRequest.Post("/accounts/login", new LoginRequest { DeviceId = _record.DeviceId });
            var response = await _backend.SendAsync(request, cancellationToken);
            if (!response.IsOk)
            {
                SetOnline(false);
                return false;
            }

            var login = response.Read<LoginResponse>();
            _record.AccountId = login.AccountId;
            _record.Nickname = login.Nickname;
            _record.AuthToken = login.Token;
            Save();
            SetOnline(true);
            return true;
        }

        private void SetOnline(bool online)
        {
            IsOnline = online;
            Changed?.Invoke();
        }

        private void Save()
        {
            _store.Write(RecordName, _record);
        }
    }
}
