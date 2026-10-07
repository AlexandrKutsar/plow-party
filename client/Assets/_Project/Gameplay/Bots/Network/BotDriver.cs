using Fusion;
using PlowParty.Gameplay.Bots.Config;
using PlowParty.Gameplay.Bots.Simulation;
using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Match.Simulation;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;
using Random = System.Random;

namespace PlowParty.Gameplay.Bots.Network
{
    [DefaultExecutionOrder(-50)]
    public sealed class BotDriver : NetworkBehaviour
    {
        private const int MaxSlots = VehicleWorldDriver.MaxVehicles;

        [SerializeField] private Transform _arenaRoot;

        private readonly BotBrain[] _brains = new BotBrain[MaxSlots];
        private readonly bool[] _stuckReported = new bool[MaxSlots];
        private readonly int[] _stuckReports = new int[MaxSlots];

        private BotConfig _config;
        private VehicleRegistry _vehicles;
        private ParticipantRoster _roster;
        private BucketRegistry _buckets;
        private BucketConfig _bucketConfig;
        private DropOffZone _dropOffZone;
        private DropOffConfig _dropOffConfig;
        private SnowGridDriver _snow;
        private SnowConfig _snowConfig;
        private IMatchClock _clock;
        private IMatchResults _results;
        private BotSettings _settings;
        private BotWorld _world;
        private SnowDepthsReader _depths;
        private Random _random;
        private float _nextSnowRefresh;
        private MatchPhase _observedPhase;
        private int _observedMatch;

        [Inject]
        public void Construct(
            BotConfig config,
            VehicleRegistry vehicles,
            ParticipantRoster roster,
            BucketRegistry buckets,
            BucketConfig bucketConfig,
            DropOffZone dropOffZone,
            DropOffConfig dropOffConfig,
            SnowGridDriver snow,
            SnowConfig snowConfig,
            IMatchClock clock,
            IMatchResults results)
        {
            _config = config;
            _vehicles = vehicles;
            _roster = roster;
            _buckets = buckets;
            _bucketConfig = bucketConfig;
            _dropOffZone = dropOffZone;
            _dropOffConfig = dropOffConfig;
            _snow = snow;
            _snowConfig = snowConfig;
            _clock = clock;
            _results = results;
        }

        public override void Spawned()
        {
            if (!Runner.IsServer)
            {
                return;
            }

            _settings = _config.ToSettings();
            _random = new Random(UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            var snowSettings = _snowConfig.ToSettings();
            var grid = NavGrid.Build(VehicleArenaReader.Read(_arenaRoot), snowSettings.Origin, snowSettings.Size, _settings.NavCellSize, _settings.NavClearance);
            var dropOff = _dropOffConfig.ToSettings();
            var tiers = new int[dropOff.MultiplierTiers.Length];
            for (var i = 0; i < tiers.Length; i++)
            {
                tiers[i] = dropOff.MultiplierTiers[i].MinLoad;
            }

            _world = new BotWorld(grid, _dropOffZone.Centre, dropOff.ZoneRadius, _bucketConfig.ToSettings().Capacity, tiers, MaxSlots);
            _depths = new SnowDepthsReader(_snow, snowSettings);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _world = null;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer || _world == null || !_clock.IsRunning || !_snow.IsReady)
            {
                return;
            }

            var time = Runner.Tick * Runner.DeltaTime;
            SeatNewBots(time);
            ObservePhase(time);
            if (_clock.Phase != MatchPhase.Playing)
            {
                IdleBots();
                return;
            }

            TakeSnapshot(time);
            DriveBots(time);
        }

        private void SeatNewBots(float time)
        {
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var slot = vehicles[i].Slot;
                if (_brains[slot] != null || !vehicles[i].IsDrivenByHost || !_roster.IsBot(slot))
                {
                    continue;
                }

                var difficulty = BotDifficultyRules.Choose(HasStrongBot(), _random.NextDouble(), _settings.WeakShare);
                _brains[slot] = new BotBrain(_config.ProfileFor(difficulty), _settings, _random.Next(), MaxSlots);
                _brains[slot].Reset(time);
                Debug.Log($"[Bots] {_roster.NicknameOf(slot)} takes Slot {slot} as a {difficulty} Bot");
            }
        }

        private bool HasStrongBot()
        {
            for (var slot = 0; slot < MaxSlots; slot++)
            {
                if (_brains[slot] != null && _brains[slot].Profile.Difficulty == BotDifficulty.Strong)
                {
                    return true;
                }
            }

            return false;
        }

        private void ObservePhase(float time)
        {
            var phase = _clock.Phase;
            if (phase == _observedPhase && _clock.MatchNumber == _observedMatch)
            {
                return;
            }

            if (phase == MatchPhase.Results)
            {
                LogMatchSummary();
            }

            if (phase == MatchPhase.Playing)
            {
                ResetBrains(time);
            }

            _observedPhase = phase;
            _observedMatch = _clock.MatchNumber;
        }

        private void ResetBrains(float time)
        {
            for (var slot = 0; slot < MaxSlots; slot++)
            {
                _brains[slot]?.Reset(time);
                _stuckReported[slot] = false;
                _stuckReports[slot] = 0;
            }
        }

        private void IdleBots()
        {
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (_brains[vehicles[i].Slot] != null)
                {
                    vehicles[i].SetHostInput(VehicleInput.Idle);
                }
            }
        }

        private void TakeSnapshot(float time)
        {
            _world.PlayingRemaining = _clock.PlayingRemaining;
            _world.ClearVehicles();
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var load = _buckets.TryGet(vehicle, out var bucket) ? bucket.Load : 0;
                _world.AddVehicle(new BotVehicle(vehicle.Slot, vehicle.Position, vehicle.Velocity, vehicle.Forward, load));
            }

            if (time >= _nextSnowRefresh)
            {
                _nextSnowRefresh = time + _config.SnowRefreshInterval;
                _world.Snow.Refresh(_depths);
            }
        }

        private void DriveBots(float time)
        {
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var brain = _brains[vehicle.Slot];
                if (brain == null || !vehicle.IsDrivenByHost)
                {
                    continue;
                }

                vehicle.SetHostInput(brain.Step(_world, vehicle.Slot, time));
                WatchForStuck(vehicle, brain, time);
            }
        }

        private void WatchForStuck(NetworkVehicle vehicle, BotBrain brain, float time)
        {
            var slot = vehicle.Slot;
            var stillFor = brain.StillFor(time);
            if (stillFor <= 0f)
            {
                _stuckReported[slot] = false;
                return;
            }

            if (stillFor < _config.StuckReportTime || _stuckReported[slot])
            {
                return;
            }

            _stuckReported[slot] = true;
            _stuckReports[slot]++;
            Debug.LogWarning($"[Bots] {_roster.NicknameOf(slot)} has been stuck for {stillFor:0.0} s at {vehicle.Position} while trying to {brain.Action}");
        }

        private string DescribeBot(BotBrain brain, int slot)
        {
            return $"{brain.Profile.Difficulty} Bot; switched to Collect {brain.SwitchesTo(BotAction.Collect)}, Deliver {brain.SwitchesTo(BotAction.Deliver)}, "
                + $"ChasePile {brain.SwitchesTo(BotAction.ChasePile)}, Ram {brain.SwitchesTo(BotAction.Ram)}, Evade {brain.SwitchesTo(BotAction.Evade)}; "
                + $"Rams dealt {RamsDealtBy(slot)}; {brain.StuckEvents} unstuck manoeuvres, {_stuckReports[slot]} stuck reports";
        }

        private int RamsDealtBy(int slot)
        {
            var vehicles = _vehicles.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].Slot == slot)
                {
                    return vehicles[i].RamsDealt;
                }
            }

            return 0;
        }

        private void LogMatchSummary()
        {
            for (var i = 0; i < _results.PlacementCount; i++)
            {
                var placement = _results.GetPlacement(i);
                var brain = _brains[placement.Slot];
                var role = brain != null ? DescribeBot(brain, placement.Slot) : "Player";
                Debug.Log($"[Bots] Match {_clock.MatchNumber} place {placement.Place}: {_roster.NicknameOf(placement.Slot)} (Slot {placement.Slot}, {role}) scored {placement.Score}");
            }
        }
    }
}
