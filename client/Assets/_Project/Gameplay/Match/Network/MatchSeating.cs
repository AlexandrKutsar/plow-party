using System;
using System.Collections.Generic;
using Fusion;
using PlowParty.Gameplay.Match.Config;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Participants.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Session;
using PlowParty.Shared;
using UnityEngine;
using VContainer.Unity;
using Random = System.Random;

namespace PlowParty.Gameplay.Match.Network
{
    public sealed class MatchSeating : IStartable, IDisposable
    {
        private readonly NetworkRunnerEvents _events;
        private readonly VehicleSpawner _spawner;
        private readonly ParticipantRoster _roster;
        private readonly MatchmakingResultStore _matchmakingResults;
        private readonly ParticipantsConfig _participants;
        private readonly WaitingRules _rules;
        private readonly Dictionary<PlayerRef, int> _playerSlots = new Dictionary<PlayerRef, int>();
        private readonly List<PlayerRef> _arrivals = new List<PlayerRef>();
        private readonly bool[] _botSlots = new bool[ParticipantRoster.MaxSlots];
        private readonly Random _random = new Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        private SeatPlan _plan;
        private float[] _botArrivals = Array.Empty<float>();
        private bool _isPlanned;
        private bool _isClosed;
        private bool _isHurried;

        public MatchSeating(
            NetworkRunnerEvents events,
            VehicleSpawner spawner,
            ParticipantRoster roster,
            MatchmakingResultStore matchmakingResults,
            MatchConfig config,
            ParticipantsConfig participants)
        {
            _events = events;
            _spawner = spawner;
            _roster = roster;
            _matchmakingResults = matchmakingResults;
            _participants = participants;
            _rules = new WaitingRules(config.ToSettings());
        }

        public int PlayerCount => _playerSlots.Count;

        public int BotCount
        {
            get
            {
                var count = 0;
                for (var slot = 0; slot < _botSlots.Length; slot++)
                {
                    count += _botSlots[slot] ? 1 : 0;
                }

                return count;
            }
        }

        public bool AllSlotsFilled => _isPlanned && WaitingRules.AllSlotsFilled(_plan, PlayerCount, BotCount);

        public void Start()
        {
            _events.ConnectRequested += OnConnectRequested;
            _events.PlayerJoined += OnPlayerJoined;
            _events.PlayerLeft += OnPlayerLeft;
        }

        public void Dispose()
        {
            _events.ConnectRequested -= OnConnectRequested;
            _events.PlayerJoined -= OnPlayerJoined;
            _events.PlayerLeft -= OnPlayerLeft;
        }

        public void Begin()
        {
            var hasMatchmakingResult = _matchmakingResults.TryGet(out var matchmakingResult);
            _plan = _rules.PlanSeats(hasMatchmakingResult, matchmakingResult, _arrivals.Count + PlayerCount, _spawner.SlotCount);
            _botArrivals = _rules.ScheduleBotArrivals(_plan.PlannedBots, _random);
            _isPlanned = true;
            Debug.Log($"[Match] Waiting for {_plan.ExpectedPlayers} Player(s) in {_plan.SlotCount} Slots ({(hasMatchmakingResult ? "matchmaking result" : "fallback")}); {_plan.PlannedBots} Bot(s) planned");
        }

        public void Tick(NetworkRunner runner, MatchPhase phase, float phaseElapsed)
        {
            if (!_isPlanned || !_roster.IsReady)
            {
                return;
            }

            if (_roster.SlotCount != _plan.SlotCount)
            {
                _roster.OpenSeats(_plan.SlotCount);
            }

            SeatArrivals(runner);
            if (phase != MatchPhase.WaitingForPlayers)
            {
                return;
            }

            HurryBotsOnceEveryoneIsIn(phaseElapsed);
            var bots = _rules.BotsToSeat(_plan, PlayerCount, BotCount, phaseElapsed, _botArrivals);
            for (var i = 0; i < bots; i++)
            {
                SeatBot(runner);
            }
        }

        public void Close()
        {
            _isClosed = true;
            Debug.Log($"[Match] Countdown starts with {PlayerCount} Player(s) and {BotCount} Bot(s)");
        }

        private void HurryBotsOnceEveryoneIsIn(float waitingElapsed)
        {
            if (_isHurried || !WaitingRules.AllExpectedPlayersSeated(_plan, PlayerCount))
            {
                return;
            }

            _isHurried = true;
            _botArrivals = _rules.HurryBotArrivals(_botArrivals, BotCount, waitingElapsed, _random);
            Debug.Log($"[Match] Every expected Player is in after {waitingElapsed:0.0} s; the remaining Bots arrive by {LastArrival():0.0} s");
        }

        private float LastArrival()
        {
            return _botArrivals.Length > 0 ? _botArrivals[_botArrivals.Length - 1] : 0f;
        }

        private void SeatArrivals(NetworkRunner runner)
        {
            for (var i = 0; i < _arrivals.Count; i++)
            {
                var player = _arrivals[i];
                if (_isClosed || !WaitingRules.HasFreeSlot(_plan, PlayerCount, BotCount))
                {
                    TurnAway(runner, player);
                }
                else
                {
                    SeatPlayer(runner, player);
                }
            }

            _arrivals.Clear();
        }

        private void SeatPlayer(NetworkRunner runner, PlayerRef player)
        {
            var slot = FirstFreeSlot();
            var token = ParticipantToken.TryFromBytes(runner.GetPlayerConnectionToken(player), out var decoded) ? decoded : null;
            var nickname = ParticipantProfiles.PlayerNickname(token?.Nickname, slot);
            var profile = new ParticipantProfile(nickname, ParticipantProfiles.PickSpecies(TakenSpecies(), _random), false);
            _playerSlots[player] = slot;
            _roster.Seat(slot, profile, token?.AccountId);
            _spawner.Spawn(runner, slot, player);
            Debug.Log($"[Match] Player {player} seated in Slot {slot} as {nickname}");
        }

        private void SeatBot(NetworkRunner runner)
        {
            var slot = RandomFreeSlot();
            var nickname = ParticipantProfiles.BotNickname(_participants.BotNicknames, TakenNicknames(), slot, _random);
            var profile = new ParticipantProfile(nickname, ParticipantProfiles.PickSpecies(TakenSpecies(), _random), true);
            _botSlots[slot] = true;
            _roster.Seat(slot, profile, null);
            _spawner.Spawn(runner, slot, PlayerRef.None);
        }

        private void TurnAway(NetworkRunner runner, PlayerRef player)
        {
            Debug.LogWarning($"[Match] Player {player} arrived after the Match started or with every Slot taken; disconnecting");
            runner.Disconnect(player);
        }

        private void OnConnectRequested(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request)
        {
            if (_isClosed || (_isPlanned && !WaitingRules.HasFreeSlot(_plan, PlayerCount + _arrivals.Count, BotCount)))
            {
                Debug.LogWarning("[Match] Refused a connection: the Match already started or every Slot is taken");
                request.Refuse();
            }
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                _arrivals.Add(player);
            }
        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer)
            {
                return;
            }

            _arrivals.Remove(player);
            if (!_playerSlots.Remove(player, out var slot))
            {
                return;
            }

            _spawner.Despawn(runner, slot);
            _roster.Vacate(slot);
            Debug.Log($"[Match] Player {player} left Slot {slot}");
        }

        private bool IsSlotFree(int slot)
        {
            return !_botSlots[slot] && !_playerSlots.ContainsValue(slot);
        }

        private int FirstFreeSlot()
        {
            for (var slot = 0; slot < _plan.SlotCount; slot++)
            {
                if (IsSlotFree(slot))
                {
                    return slot;
                }
            }

            return -1;
        }

        private int RandomFreeSlot()
        {
            var free = new List<int>(_plan.SlotCount);
            for (var slot = 0; slot < _plan.SlotCount; slot++)
            {
                if (IsSlotFree(slot))
                {
                    free.Add(slot);
                }
            }

            return free[_random.Next(free.Count)];
        }

        private List<string> TakenNicknames()
        {
            var taken = new List<string>(ParticipantRoster.MaxSlots);
            for (var slot = 0; slot < ParticipantRoster.MaxSlots; slot++)
            {
                if (_roster.TryGetProfile(slot, out var profile))
                {
                    taken.Add(profile.Nickname);
                }
            }

            return taken;
        }

        private List<CritterSpecies> TakenSpecies()
        {
            var taken = new List<CritterSpecies>(ParticipantRoster.MaxSlots);
            for (var slot = 0; slot < ParticipantRoster.MaxSlots; slot++)
            {
                if (_roster.TryGetProfile(slot, out var profile))
                {
                    taken.Add(profile.Species);
                }
            }

            return taken;
        }
    }
}
