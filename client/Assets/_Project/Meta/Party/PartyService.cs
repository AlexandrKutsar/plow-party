using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Session;
using PlowParty.Meta.Account;
using PlowParty.Meta.Party.Config;
using PlowParty.Meta.Party.Network;
using PlowParty.Meta.Party.Simulation;
using PlowParty.Shared;
using VContainer.Unity;
using Random = System.Random;

namespace PlowParty.Meta.Party
{
    public sealed class PartyService : IStartable, ITickable, IDisposable
    {
        private readonly NetworkSession _session;
        private readonly AccountService _account;
        private readonly MatchmakingPool _pool;
        private readonly PartyConfig _config;
        private readonly PartyMemory _memory;
        private readonly PartyLinks _links;
        private readonly Random _random = new Random();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly PartyLooks _looks;
        private readonly SoloPartyEditor _solo;
        private readonly LinkedPartyEditor _linked;
        private int _shownRevision = -1;
        private bool _restoreReady;
        private bool _restoreSetup;
        private bool _disposed;

        public PartyService(NetworkSession session, AccountService account, MatchmakingPool pool, PartyConfig config, PartyMemory memory, PartyLinks links, IParticipantPalette palette)
        {
            _session = session;
            _account = account;
            _pool = pool;
            _config = config;
            _memory = memory;
            _links = links;
            _looks = new PartyLooks(palette.ColorCount, palette.CritterCount);
            _solo = new SoloPartyEditor(memory, _looks, config.Capacity, _random);
            _linked = new LinkedPartyEditor(links);
            Party = _solo.Reset(account.Nickname);
        }

        public event Action Changed;

        public event Action<string> MoveRequested;

        public PartyStage Stage { get; private set; }

        public string Code { get; private set; }

        public PartyNotice Failure { get; private set; }

        public PartyState Party { get; private set; }

        public int LocalId { get; private set; } = -1;

        public bool IsLeader => Stage == PartyStage.InParty && Party.IsLeader(LocalId);

        public bool LeadsAwayParty => Stage == PartyStage.Away && _memory.Role == PartyRole.Leader;

        public bool CanStart => IsLeader && Party.CanStart;

        public bool IsSearching => Stage == PartyStage.InParty && Party.IsSearching;

        public bool IsReady
        {
            get
            {
                foreach (var member in Party.Members)
                {
                    if (member.Id == LocalId)
                    {
                        return member.IsReady;
                    }
                }

                return false;
            }
        }

        public void Start()
        {
            _session.Ended += OnSessionEnded;
            _solo.Changed += OnSoloChanged;
            _links.Removed += OnRemoved;
            _links.MoveRequested += OnMoveRequested;
            if (_memory.HasParty)
            {
                ReturnAsync(_memory.Code, _memory.Role).Forget();
            }
        }

        public void Tick()
        {
            var link = _links.Current;
            if (Stage != PartyStage.InParty || link == null || link.Revision == _shownRevision)
            {
                return;
            }

            _shownRevision = link.Revision;
            Party = link.Read(_config.Capacity, _looks);
            RestoreReady(link);
            if (!RestoreSetup(link))
            {
                RememberSetup();
            }

            Remember();
            Changed?.Invoke();
        }

        public void Dispose()
        {
            _disposed = true;
            _session.Ended -= OnSessionEnded;
            _solo.Changed -= OnSoloChanged;
            _links.Removed -= OnRemoved;
            _links.MoveRequested -= OnMoveRequested;
            _memory.ForgetReady();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        public async UniTask CreateAsync(CancellationToken cancellationToken)
        {
            Enter(PartyStage.Connecting, PartyNotice.None);
            for (var attempt = 0; attempt < _config.CodeAttempts; attempt++)
            {
                var code = PartyCode.Generate(_random);
                var outcome = await StartAsync(PartyReturnStep.Host, code, cancellationToken);
                if (await AbandonedAsync(outcome))
                {
                    return;
                }

                if (outcome == SessionStartOutcome.Started)
                {
                    EnterParty(code, true);
                    return;
                }

                if (outcome != SessionStartOutcome.NameTaken)
                {
                    break;
                }
            }

            Fail(PartyNotice.ConnectFailed);
        }

        public async UniTask JoinAsync(string input, CancellationToken cancellationToken)
        {
            if (!PartyCode.TryParse(input, out var code))
            {
                Fail(PartyNotice.NotFound);
                return;
            }

            Enter(PartyStage.Connecting, PartyNotice.None);
            var outcome = await StartAsync(PartyReturnStep.Join, code, cancellationToken);
            if (await AbandonedAsync(outcome))
            {
                return;
            }

            switch (outcome)
            {
                case SessionStartOutcome.Started:
                    EnterParty(code, false);
                    break;
                case SessionStartOutcome.NotFound:
                    Fail(PartyNotice.NotFound);
                    break;
                case SessionStartOutcome.Full:
                    Fail(PartyNotice.Full);
                    break;
                case SessionStartOutcome.Refused:
                    Fail(PartyNotice.MatchStarted);
                    break;
                default:
                    Fail(PartyNotice.ConnectFailed);
                    break;
            }
        }

        public async UniTask LeaveAsync()
        {
            _memory.Forget();
            Enter(PartyStage.None, PartyNotice.None);
            await _session.LeaveAsync();
        }

        public void SetReady(bool isReady)
        {
            if (Stage == PartyStage.InParty)
            {
                _links.Current?.RequestReady(isReady);
            }
        }

        public void StartSearch()
        {
            if (CanStart)
            {
                _links.Current?.RequestStartSearch();
            }
        }

        public void StopSearch()
        {
            if (Stage == PartyStage.InParty)
            {
                _links.Current?.RequestStopSearch();
            }
            else if (Stage == PartyStage.Away)
            {
                _memory.ForgetReady();
            }
        }

        public void MoveTo(string sessionName)
        {
            if (IsLeader)
            {
                _links.Current?.RequestMoveTo(sessionName);
            }
        }

        public void LeaveForLobby()
        {
            if (Stage != PartyStage.InParty)
            {
                return;
            }

            Remember();
            UnityEngine.Debug.Log($"[Party] Party {Code} moves to another Lobby");
            Enter(PartyStage.Away, PartyNotice.None);
        }

        public void ReturnFromLobby()
        {
            if (Stage == PartyStage.Away && _memory.HasParty)
            {
                ReturnAsync(_memory.Code, _memory.Role).Forget();
            }
        }

        public void Remove(int memberId)
        {
            if (IsLeader)
            {
                _links.Current?.RequestRemove(memberId);
            }
        }

        public void SetMode(PartyMode mode)
        {
            EditorFor(true)?.SetMode(mode);
        }

        public void SetBot(int slot, BotDifficulty difficulty)
        {
            EditorFor(true)?.SetBot(slot, difficulty);
        }

        public void RemoveBot(int slot)
        {
            EditorFor(true)?.RemoveBot(slot);
        }

        public void PickColor(int color)
        {
            EditorFor(false)?.PickColor(color);
        }

        public void PickCritter(int critter)
        {
            EditorFor(false)?.PickCritter(critter);
        }

        public CustomGameSetup CustomGameSetupFor(int localPlayerId)
        {
            return Party.CustomGameSetup(LocalId, localPlayerId);
        }

        public void CloseForMatch()
        {
            var link = _links.Current;
            if (_session.IsHost && link != null)
            {
                Remember();
                _session.Runner.Despawn(link.Object);
            }
        }

        private async UniTask ReturnAsync(string code, PartyRole role)
        {
            Enter(PartyStage.Connecting, PartyNotice.None);
            Code = code;
            UnityEngine.Debug.Log($"[Party] Returning to Party {code} as {role}");
            var lifetime = _lifetime.Token;
            var clock = Stopwatch.StartNew();
            var step = PartyReturnRules.First(role);
            while (true)
            {
                var outcome = await StartAsync(step, code, lifetime);
                if (lifetime.IsCancellationRequested || await AbandonedAsync(outcome))
                {
                    return;
                }

                if (outcome == SessionStartOutcome.Started)
                {
                    EnterParty(code, step == PartyReturnStep.Host);
                    return;
                }

                if (PartyReturnRules.RestartsWait(outcome))
                {
                    clock.Restart();
                }

                step = PartyReturnRules.Next(role, step, outcome, (float)clock.Elapsed.TotalSeconds, _config.RejoinSeconds, _config.GiveUpSeconds);
                if (step == PartyReturnStep.Stop)
                {
                    _memory.Forget();
                    Fail(outcome == SessionStartOutcome.Full ? PartyNotice.Full : PartyNotice.NotFound);
                    return;
                }

                var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_config.RetryIntervalSeconds), cancellationToken: lifetime).SuppressCancellationThrow();
                if (cancelled || Stage != PartyStage.Connecting)
                {
                    return;
                }
            }
        }

        private UniTask<SessionStartOutcome> StartAsync(PartyReturnStep step, string code, CancellationToken cancellationToken)
        {
            var hosting = step == PartyReturnStep.Host;
            return _session.StartAsync(new SessionStart
            {
                Mode = hosting ? GameMode.Host : GameMode.Client,
                SessionName = PartyCode.SessionName(_pool.Name, code),
                Properties = hosting ? LobbyProperties.Hidden(_pool.Name) : null,
                ConnectionToken = _account.ToParticipantToken().ToBytes(),
                MaxPlayers = _config.Capacity,
                IsVisible = false,
            }, cancellationToken);
        }

        private async UniTask<bool> AbandonedAsync(SessionStartOutcome outcome)
        {
            if (Stage == PartyStage.Connecting)
            {
                return false;
            }

            if (outcome == SessionStartOutcome.Started)
            {
                await _session.LeaveAsync();
            }

            return true;
        }

        private void EnterParty(string code, bool hosting)
        {
            Code = code;
            LocalId = _session.Runner.LocalPlayer.RawEncoded;
            Party = new PartyState(_config.Capacity, _memory.HasParty ? _memory.Mode : PartyMode.QuickPlay, _looks);

            _shownRevision = -1;
            _restoreReady = !hosting && _memory.HasParty && _memory.IsReady;
            _restoreSetup = true;
            if (hosting)
            {
                _links.Spawn(_session.Runner, _config.PartyLinkPrefab);
            }

            _memory.Remember(code, hosting ? PartyRole.Leader : PartyRole.Member, Party.Mode, _restoreReady);
            UnityEngine.Debug.Log($"[Party] In Party {code} as {(hosting ? PartyRole.Leader : PartyRole.Member)}");
            Enter(PartyStage.InParty, PartyNotice.None);
        }

        private void Remember()
        {
            if (Code == null)
            {
                return;
            }

            _memory.Remember(Code, IsLeader ? PartyRole.Leader : PartyRole.Member, Party.Mode, IsReady);
            _memory.RememberJoinOrder(PartyJoinOrder.Merge(_memory.JoinOrder, Party.JoinOrder));
        }

        private bool RestoreSetup(PartyLink link)
        {
            if (!_restoreSetup || !Party.TryGetMember(LocalId, out var local))
            {
                return false;
            }

            _restoreSetup = false;
            if (!_memory.HasLook)
            {
                return false;
            }

            if (local.Color != _memory.Color)
            {
                link.RequestColor(_memory.Color);
            }

            if (local.Critter != _memory.Critter)
            {
                link.RequestCritter(_memory.Critter);
            }

            if (IsLeader && Party.Bots.Count == 0)
            {
                foreach (var bot in _memory.Bots)
                {
                    link.RequestBot(bot.Slot, bot.Difficulty);
                }
            }

            return true;
        }

        private void RememberSetup()
        {
            if (Party.TryGetMember(LocalId, out var local))
            {
                _memory.RememberSetup(local.Color, local.Critter, IsLeader ? Party.Bots : _memory.Bots);
            }
        }

        private IPartyEditor EditorFor(bool leaderOnly)
        {
            if (Stage == PartyStage.None)
            {
                return _solo;
            }

            return Stage == PartyStage.InParty && (!leaderOnly || IsLeader) ? _linked : null;
        }

        private void OnSoloChanged()
        {
            Changed?.Invoke();
        }

        private void RestoreReady(PartyLink link)
        {
            if (!_restoreReady || !Party.Contains(LocalId))
            {
                return;
            }

            _restoreReady = false;
            if (!IsReady && !IsLeader)
            {
                link.RequestReady(true);
            }
        }

        private void OnMoveRequested(string sessionName)
        {
            if (Stage == PartyStage.InParty && !IsLeader)
            {
                MoveRequested?.Invoke(sessionName);
            }
        }

        private void Enter(PartyStage stage, PartyNotice failure)
        {
            if (stage == PartyStage.None)
            {
                Code = null;
                LocalId = PartyState.SoloId;
                Party = _solo.Reset(_account.Nickname);
            }

            Stage = stage;
            Failure = failure;
            Changed?.Invoke();
        }

        private void Fail(PartyNotice failure)
        {
            Enter(PartyStage.None, failure);
        }

        private void OnRemoved()
        {
            UnityEngine.Debug.Log("[Party] Removed from the Party by its Leader");
            _memory.Forget();
            Enter(PartyStage.None, PartyNotice.Removed);
            _session.LeaveAsync().Forget();
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (_disposed || end != SessionEnd.Lost || Stage != PartyStage.InParty)
            {
                return;
            }

            var role = PartyReturnRules.RoleAfterLeaderLeft(Party, LocalId);
            ReturnAsync(Code, role).Forget();
        }
    }
}
