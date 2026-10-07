using System;
using Fusion;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Match.Config;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Match.Network
{
    [DefaultExecutionOrder(-100)]
    public sealed class MatchDriver : NetworkBehaviour, IMatchClock, IMatchResults, ISnowClock
    {
        private const int MaxPlacements = VehicleWorldDriver.MaxVehicles;

        private readonly int[] _rankSlots = new int[MaxPlacements];
        private readonly int[] _rankScores = new int[MaxPlacements];
        private readonly MatchPlacement[] _placements = new MatchPlacement[MaxPlacements];

        private MatchConfig _config;
        private VehicleRegistry _vehicles;
        private VehicleSpawner _spawner;
        private IScoreReader _scoreReader;
        private MatchRules _rules;

        [Networked] public MatchPhase Phase { get; private set; }

        [Networked] public int MatchNumber { get; private set; }

        [Networked] private int PhaseStartTick { get; set; }

        [Networked] private NetworkBool RestartRequested { get; set; }

        [Networked] public int PlacementCount { get; private set; }

        [Networked, Capacity(MaxPlacements)] private NetworkArray<int> PlacementSlots { get; }

        [Networked, Capacity(MaxPlacements)] private NetworkArray<int> PlacementScores { get; }

        [Networked, Capacity(MaxPlacements)] private NetworkArray<int> PlacementPlaces { get; }

        public event Action MatchRestarted;

        public bool IsRunning => _rules != null;

        public bool IsPlaying => IsRunning && Phase == MatchPhase.Playing;

        public float PhaseRemaining => IsRunning ? _rules.PhaseRemaining(Phase, PhaseElapsed) : 0f;

        public float PlayingElapsed => IsRunning ? _rules.PlayingElapsed(Phase, PhaseElapsed) : 0f;

        public float PlayingRemaining => IsRunning ? _rules.PlayingRemaining(Phase, PhaseElapsed) : 0f;

        public bool WaitsForHost => IsRunning && _rules.WaitsForRestartRequest;

        public bool CanRequestRestart => IsRunning && Runner.IsServer && Phase == MatchPhase.Results;

        private float PhaseElapsed => MatchRules.PhaseElapsed(Runner.Tick, PhaseStartTick, Runner.DeltaTime);

        [Inject]
        public void Construct(MatchConfig config, VehicleRegistry vehicles, VehicleSpawner spawner, IScoreReader scores)
        {
            _config = config;
            _vehicles = vehicles;
            _spawner = spawner;
            _scoreReader = scores;
        }

        public override void Spawned()
        {
            _rules = new MatchRules(_config.ToSettings());
            if (HasStateAuthority)
            {
                MatchNumber = 1;
                Enter(MatchPhase.Countdown);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _rules = null;
        }

        public MatchPlacement GetPlacement(int index)
        {
            return new MatchPlacement(PlacementSlots[index], PlacementScores[index], PlacementPlaces[index]);
        }

        public void RequestRestart()
        {
            if (CanRequestRestart)
            {
                RestartRequested = true;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer)
            {
                return;
            }

            var next = _rules.NextPhase(Phase, PhaseElapsed, RestartRequested);
            if (next != Phase)
            {
                Enter(next);
            }

            LockInput(MatchRules.IsInputLocked(Phase));
        }

        private void Enter(MatchPhase phase)
        {
            if (phase == MatchPhase.Results)
            {
                RecordPlacements();
            }
            else if (MatchRules.StartsNextMatch(Phase, phase))
            {
                StartNextMatch();
            }

            Phase = phase;
            PhaseStartTick = Runner.Tick;
        }

        private void StartNextMatch()
        {
            MatchNumber++;
            RestartRequested = false;
            PlacementCount = 0;
            _spawner.RespawnAll(Runner);
            MatchRestarted?.Invoke();
        }

        private void RecordPlacements()
        {
            var vehicles = _vehicles.Vehicles;
            var count = Math.Min(vehicles.Count, MaxPlacements);
            for (var i = 0; i < count; i++)
            {
                _rankSlots[i] = vehicles[i].Slot;
                _rankScores[i] = _scoreReader.ScoreOf(vehicles[i]);
            }

            PlacementCount = MatchPlacementRules.Rank(_rankSlots, _rankScores, count, _placements);
            for (var i = 0; i < PlacementCount; i++)
            {
                PlacementSlots.Set(i, _placements[i].Slot);
                PlacementScores.Set(i, _placements[i].Score);
                PlacementPlaces.Set(i, _placements[i].Place);
            }
        }

        private void LockInput(bool locked)
        {
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                vehicles[i].IsImmobilised = locked;
            }
        }
    }
}
