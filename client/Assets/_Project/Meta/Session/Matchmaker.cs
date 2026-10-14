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
        private readonly Dictionary<int, string> _arrivedParties = new Dictionary<int, string>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private CancellationTokenSource _search;
        private bool _hostsLobby;
        private bool _followsLeader;
        private bool _inLeaderLobby;
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
            _lobbyLinks.PartyArrived += OnPartyArrived;
            _lobbyLinks.PlayerLeft += OnLobbyPlayerLeft;
        }

        public event Action Changed;

        public LobbyStage Stage { get; private set; }

        public SessionNotice Failure { get; private set; }

        public float SearchSeconds => (float)_searchClock.Elapsed.TotalSeconds;

        public bool CanSearch => Stage == LobbyStage.Idle && (_party.Stage == PartyStage.None || _party.CanStart);

        public bool CanStopSearch => Stage == LobbyStage.Searching;

        public int MaxSlots => _config.MaxSlots;

        public bool IsCustomGame => _party.Party.Mode == PartyMode.CustomGame;

        private int PartySize => _party.Stage == PartyStage.InParty ? Math.Max(1, _party.Party.Count) : 1;

        public void Search()
        {
            if (!CanSearch)
            {
                return;
            }

            if (IsCustomGame)
            {
                StartCustomGameAsync().Forget();
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
                    EndSearch(SessionNotice.None);
                    _session.LeaveAsync().Forget();
                    break;
            }
        }

        public void Tick()
        {
            FollowPartySearch();
            CheckLeaderStillHere();
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
            _lobbyLinks.PartyArrived -= OnPartyArrived;
            _lobbyLinks.PlayerLeft -= OnLobbyPlayerLeft;
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
                EndSearch(SessionNotice.None);
            }
        }

        private void CheckLeaderStillHere()
        {
            if (!_inLeaderLobby || Stage != LobbyStage.Searching || !_session.IsRunning)
            {
                return;
            }

            if (_session.TryGetProperty(LobbyProperties.Parties, out var parties) && parties.IsString && LobbyParties.Contains((string)parties.PropertyValue, _party.Code))
            {
                return;
            }

            Debug.LogWarning($"[Session] The Leader of Party {_party.Code} is not in this Lobby: back to the Party");
            ReturnToPartyAsync().Forget();
        }

        private async UniTaskVoid RunSearchAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!await _feed.JoinAsync(cancellationToken))
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        EndSearch(SessionNotice.ConnectFailed);
                    }

                    return;
                }

                while (true)
                {
                    if (_feed.Version == 0)
                    {
                        await _feed.WaitForUpdateAsync(0, _config.ListWaitSeconds, cancellationToken);
                    }

                    if (await JoinBestLobbyAsync(cancellationToken))
                    {
                        return;
                    }

                    if (_party.LeadsAwayParty)
                    {
                        CallPartyHome();
                        return;
                    }

                    if (!await OpenOwnLobbyAsync(cancellationToken))
                    {
                        return;
                    }

                    var olderLobby = await WaitForOlderLobbyAsync(cancellationToken);
                    if (olderLobby == null || await MoveAsync(olderLobby, cancellationToken))
                    {
                        return;
                    }

                    if (_party.LeadsAwayParty)
                    {
                        CallPartyHome();
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask<bool> JoinBestLobbyAsync(CancellationToken cancellationToken)
        {
            foreach (var candidate in LobbyPick.Candidates(Listings(), PartySize, Now(), _config.MinSecondsBeforeStart))
            {
                if (await MoveAsync(candidate.Name, cancellationToken))
                {
                    return true;
                }
            }

            return false;
        }

        private async UniTask<bool> MoveAsync(string sessionName, CancellationToken cancellationToken)
        {
            Debug.Log($"[Session] Joining Lobby {sessionName} with {PartySize} Players");
            HideOwnLobby();
            if (_party.Stage == PartyStage.InParty)
            {
                _party.MoveTo(sessionName);
                await UniTask.Delay(TimeSpan.FromSeconds(_config.MoveGraceSeconds), cancellationToken: cancellationToken);
                _party.LeaveForLobby();
            }

            var outcome = await JoinAsync(sessionName, cancellationToken);
            if (outcome != SessionStartOutcome.Started)
            {
                Debug.LogWarning($"[Session] Could not join Lobby {sessionName} ({outcome}); trying the next one");
                return false;
            }

            _feed.Leave();
            Debug.Log($"[Session] Joined Lobby {sessionName}");
            if (_party.LeadsAwayParty)
            {
                AnnouncePartyAsync(cancellationToken).Forget();
            }

            return true;
        }

        private async UniTaskVoid AnnouncePartyAsync(CancellationToken cancellationToken)
        {
            var deadline = TimeSpan.FromSeconds(_config.ListWaitSeconds);
            var clock = Stopwatch.StartNew();
            while (_lobbyLinks.Current == null && clock.Elapsed < deadline)
            {
                if (await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow())
                {
                    return;
                }
            }

            if (_lobbyLinks.Current == null)
            {
                Debug.LogWarning($"[Session] The Lobby has no Lobby link: Party {_party.Code} cannot call its members");
                return;
            }

            Debug.Log($"[Session] Party Leader is in the Lobby: calling the members of Party {_party.Code}");
            _lobbyLinks.Current.RequestPartyArrived(_party.Code);
        }

        private async UniTask<bool> OpenOwnLobbyAsync(CancellationToken cancellationToken)
        {
            var openedAt = Now();
            var startsAt = LobbyRules.StartsAt(openedAt, _config.SearchSeconds);
            var properties = LobbyProperties.Open(_pool.Name, startsAt, openedAt, string.Empty);
            if (_party.Stage == PartyStage.InParty)
            {
                _session.OpenAsLobby(properties);
            }
            else
            {
                var outcome = await StartSessionAsync(GameMode.Host, null, properties, cancellationToken);
                if (outcome != SessionStartOutcome.Started)
                {
                    EndSearch(SessionNotice.ConnectFailed);
                    return false;
                }
            }

            _hostedOpenedAt = openedAt;
            _hostedStartsAt = startsAt;
            _hostsLobby = true;
            _arrivedParties.Clear();
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
            EndSearch(SessionNotice.None);
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
            EndSearch(SessionNotice.None);
            await _session.LeaveAsync();
            _party.ReturnFromLobby();
        }

        private void CallPartyHome()
        {
            Debug.LogWarning($"[Session] No Lobby took Party {_party.Code}: the Party returns");
            EndSearch(SessionNotice.None);
            _party.ReturnFromLobby();
        }

        private async UniTaskVoid FollowLeaderAsync()
        {
            if (Stage == LobbyStage.Idle)
            {
                BeginSearch();
            }

            var cancellationToken = _search.Token;
            _followsLeader = true;
            _party.LeaveForLobby();
            await _session.LeaveAsync();
            try
            {
                var lobby = await WaitForLeaderLobbyAsync(cancellationToken);
                if (lobby == null)
                {
                    Debug.LogWarning($"[Session] The Leader of Party {_party.Code} got into no Lobby: back to the Party");
                    _followsLeader = false;
                    CallPartyHome();
                    return;
                }

                var outcome = await JoinAsync(lobby, cancellationToken);
                _followsLeader = false;
                if (outcome == SessionStartOutcome.Started)
                {
                    _feed.Leave();
                    _inLeaderLobby = true;
                    Debug.Log($"[Session] Followed the Party Leader into Lobby {lobby}");
                    return;
                }

                Debug.LogWarning($"[Session] Could not follow the Party into Lobby {lobby} ({outcome})");
                CallPartyHome();
            }
            catch (OperationCanceledException)
            {
                _followsLeader = false;
            }
        }

        private async UniTask<string> WaitForLeaderLobbyAsync(CancellationToken cancellationToken)
        {
            if (!await _feed.JoinAsync(cancellationToken))
            {
                return null;
            }

            var deadline = TimeSpan.FromSeconds(_config.FollowSeconds);
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < deadline)
            {
                if (LobbyParties.TryFindLobbyOf(Listings(), _party.Code, out var lobby))
                {
                    return lobby.Name;
                }

                await _feed.WaitForUpdateAsync(_feed.Version, (float)(deadline - clock.Elapsed).TotalSeconds, cancellationToken);
            }

            return null;
        }

        private void StartMatch()
        {
            var players = _session.PlayerCount;
            Debug.Log($"[Session] Lobby Host starts the Match with {players} Players");
            CancelSearch();
            _hostsLobby = false;
            Enter(LobbyStage.Starting, SessionNotice.None);
            LoadMatch(LobbyRules.MatchmakingResultFor(players, MaxSlots, PartyMode.QuickPlay));
        }

        private async UniTaskVoid StartCustomGameAsync()
        {
            Enter(LobbyStage.Starting, SessionNotice.None);
            if (_party.Stage != PartyStage.InParty)
            {
                var outcome = await _session.StartAsync(new SessionStart
                {
                    Mode = GameMode.Host,
                    ConnectionToken = _account.ToParticipantToken().ToBytes(),
                    MaxPlayers = 1,
                    IsVisible = false,
                }, _lifetime.Token);
                if (_disposed)
                {
                    return;
                }

                if (outcome != SessionStartOutcome.Started)
                {
                    Enter(LobbyStage.Idle, SessionNotice.ConnectFailed);
                    return;
                }
            }

            var setup = _party.CustomGameSetupFor(_session.Runner.LocalPlayer.RawEncoded);
            Debug.Log($"[Session] Custom Game starts with {setup.PlayerCount} Player(s) and {setup.BotCount} Bot(s)");
            LoadMatch(new MatchmakingResult(setup));
        }

        private void LoadMatch(MatchmakingResult result)
        {
            _matchmakingResults.Set(result);
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
            _arrivedParties.Clear();
            _lobbyLinks.Despawn(_session.Runner);
            _session.Hide(LobbyProperties.Hidden(_pool.Name));
        }

        private void PublishArrivedParties()
        {
            _session.OpenAsLobby(LobbyProperties.Open(_pool.Name, _hostedStartsAt, _hostedOpenedAt, LobbyParties.Format(_arrivedParties.Values)));
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
                    var parties = LobbyProperties.ReadString(session.Properties, LobbyProperties.Parties);
                    _listings.Add(new LobbyListing(session.Name, session.PlayerCount, session.MaxPlayers, startsAt, openedAt, parties));
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
            Enter(LobbyStage.Searching, SessionNotice.None);
        }

        private void EndSearch(SessionNotice failure)
        {
            CancelSearch();
            _hostsLobby = false;
            _inLeaderLobby = false;
            _arrivedParties.Clear();
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

        private void Enter(LobbyStage stage, SessionNotice failure)
        {
            Stage = stage;
            Failure = failure;
            Changed?.Invoke();
        }

        private void OnMoveRequested(string sessionName)
        {
            Debug.Log($"[Session] The Party Leader is moving to Lobby {sessionName}: waiting until the Leader is in");
            FollowLeaderAsync().Forget();
        }

        private void OnPartyStopped(string partyCode)
        {
            if (Stage == LobbyStage.Searching && _party.Stage == PartyStage.Away && partyCode == _party.Code)
            {
                Debug.Log($"[Session] A member of Party {partyCode} stopped the search");
                ReturnToPartyAsync().Forget();
            }
        }

        private void OnPartyArrived(int leaderId, string partyCode)
        {
            if (!_hostsLobby || !_session.IsHost)
            {
                return;
            }

            _arrivedParties[leaderId] = partyCode;
            Debug.Log($"[Session] The Leader of Party {partyCode} joined this Lobby");
            PublishArrivedParties();
        }

        private void OnLobbyPlayerLeft(int playerId)
        {
            if (_arrivedParties.Remove(playerId) && _hostsLobby && _session.IsHost)
            {
                PublishArrivedParties();
            }
        }

        private void OnSessionEnded(SessionEnd end)
        {
            if (_disposed || end != SessionEnd.Lost || Stage != LobbyStage.Searching || _followsLeader)
            {
                return;
            }

            switch (_party.Stage)
            {
                case PartyStage.Away:
                    EndSearch(SessionNotice.None);
                    _party.ReturnFromLobby();
                    break;
                case PartyStage.None when !_hostsLobby:
                    Debug.Log("[Session] The Lobby closed: searching again");
                    RestartSearch();
                    break;
                case PartyStage.None:
                    EndSearch(SessionNotice.ConnectionLost);
                    break;
                default:
                    EndSearch(SessionNotice.None);
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
