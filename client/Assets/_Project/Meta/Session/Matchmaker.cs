using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Infrastructure.Session;
using PlowParty.Meta.Account;
using PlowParty.Meta.Party;
using PlowParty.Meta.Session.Config;
using PlowParty.Meta.Session.Network;
using PlowParty.Meta.Session.Simulation;
using PlowParty.Shared;
using VContainer.Unity;
using Debug = UnityEngine.Debug;

namespace PlowParty.Meta.Session
{
    public sealed class Matchmaker : ITickable, IDisposable
    {
        public const string ConnectFailedText = "Не удалось подключиться";
        public const string SessionLostText = "Связь с хостом потеряна";
        public const string MatchStartedText = "Матч уже начался";

        private readonly NetworkSession _session;
        private readonly SessionListFeed _feed;
        private readonly AccountService _account;
        private readonly MatchmakingResultStore _matchmakingResults;
        private readonly MatchmakingConfig _config;
        private readonly MatchmakingPool _pool;
        private readonly PartyService _party;
        private readonly LobbyLinks _lobbyLinks;
        private readonly Stopwatch _searchClock = new Stopwatch();
        private readonly List<LobbyListing> _listings = new List<LobbyListing>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private CancellationTokenSource _search;
        private bool _hostsLobby;
        private long _hostedOpenedAt;
        private long _hostedStartsAt;
        private bool _disposed;

        public Matchmaker(NetworkSession session, SessionListFeed feed, AccountService account, MatchmakingResultStore matchmakingResults, MatchmakingConfig config, MatchmakingPool pool, PartyService party, LobbyLinks lobbyLinks, SessionExit exit)
        {
            _session = session;
            _feed = feed;
            _account = account;
            _matchmakingResults = matchmakingResults;
            _config = config;
            _pool = pool;
            _party = party;
            _lobbyLinks = lobbyLinks;
            Failure = exit.TakeNotice();
            _session.Ended += OnSessionEnded;
            _party.MoveRequested += OnMoveRequested;
            _lobbyLinks.PartyStopped += OnPartyStopped;
        }

        private enum MoveOutcome
        {
            Joined,
            Failed,
            PartyReturned,
        }

        public event Action Changed;

        public LobbyStage Stage { get; private set; }

        public string Failure { get; private set; }

        public float SearchSeconds => (float)_searchClock.Elapsed.TotalSeconds;

        public bool CanSearch => Stage == LobbyStage.Idle && (_party.Stage == PartyStage.None || _party.CanStart);

        public bool CanStopSearch => Stage == LobbyStage.Searching;

        public int MaxSlots => _config.MaxSlots;

        private int PartySize => _party.Stage == PartyStage.InParty ? Math.Max(1, _party.Party.Count) : 1;

        public void Search()
        {
            if (!CanSearch)
            {
                return;
            }

            if (_party.Stage == PartyStage.InParty)
            {
                _party.StartSearch();
                return;
            }

            BeginSearch();
            RunSearchAsync(_search.Token).Forget();
        }

        public void StopSearch()
        {
            if (!CanStopSearch)
            {
                return;
            }

            Debug.Log("[Session] Search stopped");
            switch (_party.Stage)
            {
                case PartyStage.InParty:
                    _party.StopSearch();
                    break;
                case PartyStage.Away:
                    StopPartyAwayAsync().Forget();
                    break;
                default:
                    EndSearch(null);
                    _session.LeaveAsync().Forget();
                    break;
            }
        }

        public void Tick()
        {
            FollowPartySearch();
            if (Stage == LobbyStage.Searching && _hostsLobby && _session.IsHost && LobbyRules.ShouldStart(_session.PlayerCount, MaxSlots, LobbyRules.SecondsLeft(_hostedStartsAt, Now())))
            {
                StartMatch();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _session.Ended -= OnSessionEnded;
            _party.MoveRequested -= OnMoveRequested;
            _lobbyLinks.PartyStopped -= OnPartyStopped;
            _search?.Cancel();
            _search?.Dispose();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void FollowPartySearch()
        {
            if (_party.Stage != PartyStage.InParty)
            {
                return;
            }

            if (_party.IsSearching && Stage == LobbyStage.Idle)
            {
                BeginSearch();
                if (_party.IsLeader)
                {
                    RunSearchAsync(_search.Token).Forget();
                }
            }
            else if (!_party.IsSearching && Stage == LobbyStage.Searching)
            {
                HideOwnLobby();
                EndSearch(null);
            }
        }

        private async UniTaskVoid RunSearchAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!await _feed.JoinAsync(cancellationToken))
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        EndSearch(ConnectFailedText);
                    }

                    return;
                }

                while (true)
                {
                    if (_feed.Version == 0)
                    {
                        await _feed.WaitForUpdateAsync(0, _config.ListWaitSeconds, cancellationToken);
                    }

                    if (await JoinBestLobbyAsync(cancellationToken) != MoveOutcome.Failed || !await OpenOwnLobbyAsync(cancellationToken))
                    {
                        return;
                    }

                    var olderLobby = await WaitForOlderLobbyAsync(cancellationToken);
                    if (olderLobby == null || await MoveAsync(olderLobby, cancellationToken) != MoveOutcome.Failed)
                    {
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask<MoveOutcome> JoinBestLobbyAsync(CancellationToken cancellationToken)
        {
            foreach (var candidate in LobbyPick.Candidates(Listings(), PartySize, Now(), _config.MinSecondsBeforeStart))
            {
                var outcome = await MoveAsync(candidate.Name, cancellationToken);
                if (outcome != MoveOutcome.Failed)
                {
                    return outcome;
                }
            }

            return MoveOutcome.Failed;
        }

        private async UniTask<MoveOutcome> MoveAsync(string sessionName, CancellationToken cancellationToken)
        {
            Debug.Log($"[Session] Joining Lobby {sessionName} with {PartySize} Players");
            var movingParty = _party.Stage == PartyStage.InParty;
            HideOwnLobby();
            if (movingParty)
            {
                _party.MoveTo(sessionName);
                await UniTask.Delay(TimeSpan.FromSeconds(_config.MoveGraceSeconds), cancellationToken: cancellationToken);
                _party.LeaveForLobby();
            }

            var outcome = await JoinAsync(sessionName, cancellationToken);
            if (outcome == SessionStartOutcome.Started)
            {
                _feed.Leave();
                Debug.Log($"[Session] Joined Lobby {sessionName}");
                return MoveOutcome.Joined;
            }

            if (!movingParty)
            {
                return MoveOutcome.Failed;
            }

            Debug.LogWarning($"[Session] The Party could not join Lobby {sessionName} ({outcome}) and returns");
            EndSearch(null);
            _party.ReturnFromLobby();
            return MoveOutcome.PartyReturned;
        }

        private async UniTask<bool> OpenOwnLobbyAsync(CancellationToken cancellationToken)
        {
            var openedAt = Now();
            var startsAt = LobbyRules.StartsAt(openedAt, _config.SearchSeconds);
            var properties = LobbyProperties.Open(_pool.Name, startsAt, openedAt);
            if (_party.Stage == PartyStage.InParty)
            {
                _session.OpenAsLobby(properties);
            }
            else
            {
                var outcome = await StartSessionAsync(GameMode.Host, null, properties, cancellationToken);
                if (outcome != SessionStartOutcome.Started)
                {
                    EndSearch(ConnectFailedText);
                    return false;
                }
            }

            _hostedOpenedAt = openedAt;
            _hostedStartsAt = startsAt;
            _hostsLobby = true;
            _lobbyLinks.Spawn(_session.Runner, _config.LobbyLinkPrefab);
            Debug.Log($"[Session] No Lobby fits {PartySize} Players: hosting a Lobby for {_config.SearchSeconds} s");
            return true;
        }

        private async UniTask<string> WaitForOlderLobbyAsync(CancellationToken cancellationToken)
        {
            while (_hostsLobby && _session.IsRunning)
            {
                await _feed.WaitForUpdateAsync(_feed.Version, _config.MergeCheckSeconds, cancellationToken);
                if (!_hostsLobby || !_session.IsRunning)
                {
                    break;
                }

                var own = new LobbyListing(_session.Runner.SessionInfo.Name, _session.PlayerCount, MaxSlots, _hostedStartsAt, _hostedOpenedAt);
                if (LobbyPick.TryMergeTarget(Listings(), own, PartySize, Now(), _config.MinSecondsBeforeStart, out var olderLobby))
                {
                    Debug.Log($"[Session] Merging into the older Lobby {olderLobby.Name}");
                    return olderLobby.Name;
                }
            }

            return null;
        }

        private async UniTask<SessionStartOutcome> JoinAsync(string sessionName, CancellationToken cancellationToken)
        {
            return await StartSessionAsync(GameMode.Client, sessionName, null, cancellationToken);
        }

        private async UniTask<SessionStartOutcome> StartSessionAsync(GameMode mode, string sessionName, Dictionary<string, SessionProperty> properties, CancellationToken cancellationToken)
        {
            var outcome = await _session.StartAsync(new SessionStart
            {
                Mode = mode,
                SessionName = sessionName,
                Properties = properties,
                ConnectionToken = _account.ToParticipantToken().ToBytes(),
                MaxPlayers = _config.MaxSlots,
            }, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                if (outcome == SessionStartOutcome.Started)
                {
                    await _session.LeaveAsync();
                }

                throw new OperationCanceledException(cancellationToken);
            }

            return outcome;
        }

        private async UniTaskVoid StopPartyAwayAsync()
        {
            EndSearch(null);
            _party.StopSearch();
            _lobbyLinks.Current?.RequestPartyStop(_party.Code);
            await UniTask.Delay(TimeSpan.FromSeconds(_config.MoveGraceSeconds), cancellationToken: _lifetime.Token).SuppressCancellationThrow();
            if (!_disposed)
            {
                await ReturnToPartyAsync();
            }
        }

        private async UniTask ReturnToPartyAsync()
        {
            EndSearch(null);
            await _session.LeaveAsync();
            _party.ReturnFromLobby();
        }

        private async UniTaskVoid FollowPartyAsync(string sessionName)
        {
            if (Stage == LobbyStage.Idle)
            {
                BeginSearch();
            }

            _party.LeaveForLobby();
            try
            {
                var outcome = await JoinAsync(sessionName, _search.Token);
                if (outcome == SessionStartOutcome.Started)
                {
                    Debug.Log($"[Session] Followed the Party Leader into Lobby {sessionName}");
                    return;
                }

                Debug.LogWarning($"[Session] Could not follow the Party into Lobby {sessionName} ({outcome})");
                EndSearch(null);
                _party.ReturnFromLobby();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void StartMatch()
        {
            var players = _session.PlayerCount;
            Debug.Log($"[Session] Lobby Host starts the Match with {players} Players");
            CancelSearch();
            _hostsLobby = false;
            Enter(LobbyStage.Starting, null);
            _matchmakingResults.Set(LobbyRules.MatchmakingResultFor(players, MaxSlots, PartyMode.QuickPlay));
            _party.CloseForMatch();
            _lobbyLinks.Despawn(_session.Runner);
            _session.Close();
            _session.LoadScene(SceneNames.Match);
        }

        private void HideOwnLobby()
        {
            if (!_hostsLobby)
            {
                return;
            }

            _hostsLobby = false;
            _lobbyLinks.Despawn(_session.Runner);
            _session.Hide(LobbyProperties.Hidden(_pool.Name));
        }

        private List<LobbyListing> Listings()
        {
            _listings.Clear();
            foreach (var session in _feed.Sessions)
            {
                if (session.IsValid && session.IsOpen && session.IsVisible && LobbyProperties.IsInPool(session.Properties, _pool.Name))
                {
                    var startsAt = LobbyProperties.ReadMilliseconds(session.Properties, LobbyProperties.StartsAt);
                    var openedAt = LobbyProperties.ReadMilliseconds(session.Properties, LobbyProperties.OpenedAt);
                    _listings.Add(new LobbyListing(session.Name, session.PlayerCount, session.MaxPlayers, startsAt, openedAt));
                }
            }

            return _listings;
        }

        private void BeginSearch()
        {
            CancelSearch();
            _search = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            _searchClock.Restart();
            Debug.Log($"[Session] Searching for a Lobby for {PartySize} Players");
            Enter(LobbyStage.Searching, null);
        }

        private void EndSearch(string failure)
        {
            CancelSearch();
            _hostsLobby = false;
            _searchClock.Reset();
            Enter(LobbyStage.Idle, failure);
        }

        private void CancelSearch()
        {
            if (_search != null)
            {
                _search.Cancel();
                _search.Dispose();
                _search = null;
            }

            _feed.Leave();
        }

        private void Enter(LobbyStage stage, string failure)
        {
            Stage = stage;
            Failure = failure;
            Changed?.Invoke();
        }

        private void OnMoveRequested(string sessionName)
        {
            FollowPartyAsync(sessionName).Forget();
        }

        private void OnPartyStopped(string partyCode)
        {
            if (Stage == LobbyStage.Searching && _party.Stage == PartyStage.Away && partyCode == _party.Code)
            {
                Debug.Log($"[Session] A member of Party {partyCode} stopped the search");
                ReturnToPartyAsync().Forget();
            }
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (_disposed || end != SessionEnd.Lost || Stage != LobbyStage.Searching)
            {
                return;
            }

            switch (_party.Stage)
            {
                case PartyStage.Away:
                    EndSearch(null);
                    _party.ReturnFromLobby();
                    break;
                case PartyStage.None when !_hostsLobby:
                    Debug.Log("[Session] The Lobby closed: searching again");
                    RestartSearch();
                    break;
                case PartyStage.None:
                    EndSearch(SessionLostText);
                    break;
                default:
                    EndSearch(null);
                    break;
            }
        }

        private void RestartSearch()
        {
            CancelSearch();
            _search = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            RunSearchAsync(_search.Token).Forget();
        }

        private static long Now()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
