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
using PlowParty.Meta.Party;
using PlowParty.Meta.Session.Config;
using PlowParty.Meta.Session.Simulation;
using UnityEngine;
using VContainer.Unity;
using Random = System.Random;

namespace PlowParty.Meta.Session
{
    public sealed class Matchmaker : ITickable, IDisposable
    {
        public const string ConnectFailedText = "Не удалось подключиться";
        public const string SessionLostText = "Связь с хостом потеряна";
        public const string MatchStartedText = "Матч уже начался";

        private const string PoolProperty = "pool";
        private const string DeadlineProperty = "deadline";
        private const float MillisecondsPerSecond = 1000f;

        private readonly NetworkSession _session;
        private readonly AccountService _account;
        private readonly MatchmakingResultStore _matchmakingResults;
        private readonly MatchmakingConfig _config;
        private readonly MatchmakingPool _pool;
        private readonly PartyService _party;
        private readonly Random _random = new Random();
        private long _deadline;

        public Matchmaker(NetworkSession session, AccountService account, MatchmakingResultStore matchmakingResults, MatchmakingConfig config, MatchmakingPool pool, PartyService party, SessionExit exit)
        {
            _session = session;
            _account = account;
            _matchmakingResults = matchmakingResults;
            _config = config;
            _pool = pool;
            _party = party;
            Failure = exit.TakeNotice();
            _session.Ended += OnSessionEnded;
        }

        public event Action Changed;

        public LobbyStage Stage { get; private set; }

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

        public async UniTask LeaveAsync()
        {
            Enter(LobbyStage.Idle);
            await _session.LeaveAsync();
        }

        public void StartPartyMatch()
        {
            if (Stage != LobbyStage.Idle || !_party.CanStart || !_session.IsHost)
            {
                return;
            }

            Debug.Log($"[Session] Party Leader starts the Match with {PlayerCount} Players");
            _party.CloseForMatch();
            StartMatch();
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
            if (IsHost && LobbyRules.ShouldStart(PlayerCount, MaxSlots, SecondsLeft))
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
            ReadSessionProperties();
            Enter(LobbyStage.Gathering);
        }

        private void Enter(LobbyStage stage, string failure = null)
        {
            if (stage != LobbyStage.Gathering && stage != LobbyStage.Starting)
            {
                _deadline = 0;
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
