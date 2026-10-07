using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Infrastructure.Session;
using PlowParty.Meta.Account;
using PlowParty.Meta.Session.Config;
using PlowParty.Meta.Session.Simulation;
using VContainer.Unity;

namespace PlowParty.Meta.Session
{
    public sealed class Matchmaker : ITickable, IDisposable
    {
        public const string ConnectFailedText = "Не удалось подключиться";
        public const string RoomNotFoundText = "Комната не найдена";
        public const string BadCodeText = "Код — 5 символов";
        public const string SessionLostText = "Связь с хостом потеряна";
        public const string MatchStartedText = "Матч уже начался";

        private const string PoolProperty = "pool";
        private const string CodeProperty = "code";
        private const string DeadlineProperty = "deadline";
        private const float MillisecondsPerSecond = 1000f;

        private readonly NetworkSession _session;
        private readonly AccountService _account;
        private readonly MatchmakingResultStore _matchmakingResults;
        private readonly MatchmakingConfig _config;
        private readonly MatchmakingPool _pool;
        private readonly Random _random = new Random();
        private long _deadline;
        private bool _startRequested;

        public Matchmaker(NetworkSession session, AccountService account, MatchmakingResultStore matchmakingResults, MatchmakingConfig config, MatchmakingPool pool, SessionExit exit)
        {
            _session = session;
            _account = account;
            _matchmakingResults = matchmakingResults;
            _config = config;
            _pool = pool;
            Failure = exit.TakeNotice();
            _session.Ended += OnSessionEnded;
        }

        public event Action Changed;

        public LobbyStage Stage { get; private set; }

        public LobbyMode Mode { get; private set; }

        public string RoomCode { get; private set; }

        public string Failure { get; private set; }

        public bool IsHost => _session.IsHost;

        public int PlayerCount => _session.PlayerCount;

        public int MaxSlots => _config.MaxSlots;

        public float SecondsLeft => _deadline > 0 ? LobbyRules.SecondsLeft(_deadline, NowMilliseconds()) : _config.SearchSeconds;

        public async UniTask QuickPlayAsync(CancellationToken cancellationToken)
        {
            Enter(LobbyStage.Connecting);
            var outcome = await StartAsync(GameMode.Client, null, PoolProperties(), cancellationToken);
            if (LobbyRules.FoundNothingToJoin(outcome) && Stage == LobbyStage.Connecting)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(HostJitterSeconds()), cancellationToken: cancellationToken);
                if (Stage == LobbyStage.Connecting)
                {
                    outcome = await StartAsync(GameMode.Client, null, PoolProperties(), cancellationToken);
                }
            }

            if (LobbyRules.FoundNothingToJoin(outcome) && Stage == LobbyStage.Connecting)
            {
                var properties = PoolProperties();
                properties[DeadlineProperty] = SearchDeadline().ToString(CultureInfo.InvariantCulture);
                outcome = await StartAsync(GameMode.Host, null, properties, cancellationToken);
            }

            if (await AbandonedAsync(outcome))
            {
                return;
            }

            if (outcome != SessionStartOutcome.Started)
            {
                Fail(ConnectFailedText);
                return;
            }

            EnterLobby();
        }

        public async UniTask CreateRoomAsync(CancellationToken cancellationToken)
        {
            Enter(LobbyStage.Connecting);
            for (var attempt = 0; attempt < _config.RoomCodeAttempts; attempt++)
            {
                var code = Simulation.RoomCode.Generate(_random);
                var properties = PoolProperties();
                properties[CodeProperty] = code;
                var outcome = await StartAsync(GameMode.Host, _pool.RoomSessionName(code), properties, cancellationToken);
                if (await AbandonedAsync(outcome))
                {
                    return;
                }

                if (outcome == SessionStartOutcome.Started)
                {
                    EnterLobby();
                    return;
                }

                if (outcome != SessionStartOutcome.NameTaken)
                {
                    break;
                }
            }

            Fail(ConnectFailedText);
        }

        public async UniTask JoinRoomAsync(string input, CancellationToken cancellationToken)
        {
            if (!Simulation.RoomCode.TryParse(input, out var code))
            {
                Fail(BadCodeText);
                return;
            }

            Enter(LobbyStage.Connecting);
            var outcome = await StartAsync(GameMode.Client, _pool.RoomSessionName(code), null, cancellationToken);
            if (await AbandonedAsync(outcome))
            {
                return;
            }

            switch (outcome)
            {
                case SessionStartOutcome.Started:
                    EnterLobby();
                    break;
                case SessionStartOutcome.NotFound:
                    Fail(RoomNotFoundText);
                    break;
                case SessionStartOutcome.Refused:
                    Fail(MatchStartedText);
                    break;
                default:
                    Fail(ConnectFailedText);
                    break;
            }
        }

        public async UniTask LeaveAsync()
        {
            Enter(LobbyStage.Idle);
            await _session.LeaveAsync();
        }

        public void RequestStart()
        {
            _startRequested = Stage == LobbyStage.Gathering && IsHost;
        }

        public void Tick()
        {
            if (Stage != LobbyStage.Gathering)
            {
                return;
            }

            if (!_session.IsRunning)
            {
                Fail(SessionLostText);
                return;
            }

            ReadSessionProperties();
            if (IsHost && LobbyRules.ShouldStart(Mode, PlayerCount, MaxSlots, SecondsLeft, _startRequested))
            {
                StartMatch();
            }
        }

        public void Dispose()
        {
            _session.Ended -= OnSessionEnded;
        }

        private UniTask<SessionStartOutcome> StartAsync(GameMode mode, string sessionName, Dictionary<string, SessionProperty> properties, CancellationToken cancellationToken)
        {
            return _session.StartAsync(new SessionStart
            {
                Mode = mode,
                SessionName = sessionName,
                Properties = properties,
                ConnectionToken = _account.ToParticipantToken().ToBytes(),
                MaxPlayers = _config.MaxSlots,
            }, cancellationToken);
        }

        private async UniTask<bool> AbandonedAsync(SessionStartOutcome outcome)
        {
            if (Stage == LobbyStage.Connecting)
            {
                return false;
            }

            if (outcome == SessionStartOutcome.Started)
            {
                await _session.LeaveAsync();
            }

            return true;
        }

        private Dictionary<string, SessionProperty> PoolProperties()
        {
            return new Dictionary<string, SessionProperty> { [PoolProperty] = _pool.Name };
        }

        private float HostJitterSeconds()
        {
            return LobbyRules.HostJitterSeconds(_random.NextDouble(), _config.HostJitterMinSeconds, _config.HostJitterMaxSeconds);
        }

        private long SearchDeadline()
        {
            return NowMilliseconds() + (long)(_config.SearchSeconds * MillisecondsPerSecond);
        }

        private void ReadSessionProperties()
        {
            if (RoomCode == null && _session.TryGetProperty(CodeProperty, out var code) && code.IsString)
            {
                RoomCode = code.PropertyValue as string;
                Mode = LobbyMode.Room;
                Changed?.Invoke();
            }

            if (_deadline == 0 && _session.TryGetProperty(DeadlineProperty, out var deadline) && deadline.IsString)
            {
                long.TryParse(deadline.PropertyValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out _deadline);
            }
        }

        private void StartMatch()
        {
            Enter(LobbyStage.Starting);
            _matchmakingResults.Set(LobbyRules.MatchmakingResultFor(PlayerCount, MaxSlots));
            _session.Close();
            _session.LoadScene(SceneNames.Match);
        }

        private void EnterLobby()
        {
            Mode = LobbyMode.QuickPlay;
            ReadSessionProperties();
            Enter(LobbyStage.Gathering);
        }

        private void Enter(LobbyStage stage, string failure = null)
        {
            if (stage != LobbyStage.Gathering && stage != LobbyStage.Starting)
            {
                RoomCode = null;
                _deadline = 0;
                _startRequested = false;
            }

            Stage = stage;
            Failure = failure;
            Changed?.Invoke();
        }

        private void Fail(string failure)
        {
            Enter(LobbyStage.Idle, failure);
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (end == SessionEnd.Lost && Stage != LobbyStage.Idle)
            {
                Fail(SessionLostText);
            }
        }

        private static long NowMilliseconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
