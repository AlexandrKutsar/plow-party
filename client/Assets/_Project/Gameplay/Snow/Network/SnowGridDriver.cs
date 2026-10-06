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

        private readonly VehicleScrape[] _scrapes = new VehicleScrape[VehicleWorldDriver.MaxVehicles];

        private VehicleRegistry _registry;
        private IBladeRoom _room;
        private SnowConfig _config;
        private SnowSettings _settings;
        private SnowGrid _grid;
        private int _scrapeCount;

        [Networked, Capacity(MaxWords)] private NetworkArray<int> Words { get; }

        [Networked] private int StartTick { get; set; }

        public event Action<VehicleScrape> Scraped;

        public bool IsReady => _grid != null;

        [Inject]
        public void Construct(VehicleRegistry registry, SnowConfig config, IBladeRoom room)
        {
            _registry = registry;
            _config = config;
            _room = room;
        }

        public override void Spawned()
        {
            _settings = _config.ToSettings();
            var seed = HasStateAuthority ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : 0;
            _grid = new SnowGrid(_settings, VehicleArenaReader.Read(_arenaRoot), seed);
            if (_grid.WordCount > MaxWords)
            {
                throw new InvalidOperationException($"Snow Grid needs {_grid.WordCount} words, {nameof(MaxWords)} is {MaxWords}");
            }

            if (HasStateAuthority)
            {
                StartTick = Runner.Tick;
                CopyGridToWords();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _grid = null;
        }

        public void Spill(Vector2 point, int steps)
        {
            if (IsReady && Runner.IsServer)
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
            ReportScrapes();
        }

        private void ScrapeUnderBlades()
        {
            var vehicles = _registry.Vehicles;
            _scrapeCount = 0;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var steps = _grid.Scrape(SnowBlade.Ahead(vehicle.Position, vehicle.Forward, _settings), _room.RoomFor(vehicle));
                if (steps > 0)
                {
                    _scrapes[_scrapeCount++] = new VehicleScrape(vehicle, steps);
                }
            }
        }

        private void ReportScrapes()
        {
            for (var i = 0; i < _scrapeCount; i++)
            {
                Scraped?.Invoke(_scrapes[i]);
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
