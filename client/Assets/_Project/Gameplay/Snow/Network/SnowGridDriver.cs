using System;
using Fusion;
using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Snow.Network
{
    public sealed class SnowGridDriver : NetworkBehaviour
    {
        public const int MaxWords = 512;

        [SerializeField] private Transform _arenaRoot;

        private VehicleRegistry _registry;
        private SnowConfig _config;
        private IBladeRoom _bladeRoom;
        private SnowSettings _settings;
        private SnowGrid _grid;

        [Networked, Capacity(MaxWords)] private NetworkArray<int> Words { get; }

        [Networked] private int Seed { get; set; }

        [Networked] private int StartTick { get; set; }

        public event Action<VehicleScrape> Scraped;

        public bool IsReady => _grid != null;

        public SnowSettings Settings => _settings;

        [Inject]
        public void Construct(VehicleRegistry registry, SnowConfig config, IBladeRoom bladeRoom)
        {
            _registry = registry;
            _config = config;
            _bladeRoom = bladeRoom;
        }

        public override void Spawned()
        {
            _settings = _config.ToSettings();
            if (HasStateAuthority)
            {
                Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                StartTick = Runner.Tick;
            }

            _grid = new SnowGrid(_settings, VehicleArenaReader.Read(_arenaRoot), Seed);
            if (_grid.WordCount > MaxWords)
            {
                throw new InvalidOperationException($"Snow Grid needs {_grid.WordCount} words, {nameof(MaxWords)} is {MaxWords}");
            }

            if (HasStateAuthority)
            {
                CopyGridToWords();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _grid = null;
        }

        public void Spill(Vector2 point, int steps)
        {
            if (Runner.IsServer)
            {
                _grid.Spill(point, steps);
            }
        }

        public void CopyWordsTo(SnowGrid target)
        {
            for (var i = 0; i < _grid.WordCount; i++)
            {
                target.SetWord(i, Words[i]);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer)
            {
                return;
            }

            ScrapeUnderBlades();
            _grid.Tick((Runner.Tick - StartTick) * Runner.DeltaTime);
            CopyGridToWords();
        }

        private void ScrapeUnderBlades()
        {
            var vehicles = _registry.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var blade = SnowBlade.Ahead(vehicle.Position, vehicle.Forward, _settings);
                var steps = _grid.Scrape(blade, _bladeRoom.GetRoom(vehicle.Slot));
                if (steps > 0)
                {
                    Scraped?.Invoke(new VehicleScrape(vehicle, steps));
                }
            }
        }

        private void CopyGridToWords()
        {
            for (var i = 0; i < _grid.WordCount; i++)
            {
                Words.Set(i, _grid.GetWord(i));
            }
        }
    }
}
