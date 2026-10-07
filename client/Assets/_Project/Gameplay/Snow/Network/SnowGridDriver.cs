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
        public const int MaxWords = 1024;

        [SerializeField] private Transform _arenaRoot;

        private readonly VehicleScrape[] _scrapes = new VehicleScrape[VehicleWorldDriver.MaxVehicles];
        private readonly bool[] _plowingSlots = new bool[VehicleWorldDriver.MaxVehicles];

        private VehicleRegistry _registry;
        private IScrapeLimit _limit;
        private ISnowFreeArea _snowFree;
        private ISnowClock _clock;
        private SnowConfig _config;
        private SnowSettings _settings;
        private SnowGrid _grid;
        private int _scrapeCount;

        [Networked, Capacity(MaxWords)] private NetworkArray<int> Words { get; }

        [Networked, Capacity(VehicleWorldDriver.MaxVehicles)] private NetworkArray<NetworkBool> PlowingPileBySlot { get; }

        public event Action<VehicleScrape> Scraped;

        public bool IsReady => _grid != null;

        [Inject]
        public void Construct(VehicleRegistry registry, SnowConfig config, IScrapeLimit limit, ISnowFreeArea snowFree, ISnowClock clock)
        {
            _clock = clock;
            _registry = registry;
            _config = config;
            _limit = limit;
            _snowFree = snowFree;
        }

        public override void Spawned()
        {
            _settings = _config.ToSettings();
            _grid = new SnowGrid(_settings, VehicleArenaReader.Read(_arenaRoot), _snowFree, NewSeed());
            if (_grid.WordCount > MaxWords)
            {
                throw new InvalidOperationException(
                    $"Snow Grid of {_grid.Width} x {_grid.Height} Cells needs {_grid.WordCount} words, but {nameof(SnowGridDriver)} holds at most {MaxWords}; shrink {nameof(SnowConfig)} size or grow its cell size");
            }

            if (HasStateAuthority)
            {
                CopyGridToWords();
                _clock.MatchRestarted += OnMatchRestarted;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _clock.MatchRestarted -= OnMatchRestarted;
            _grid = null;
        }

        public bool IsPlowingPile(NetworkVehicle vehicle)
        {
            return IsReady && HasSlot(vehicle) && PlowingPileBySlot[vehicle.Slot];
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
            if (_clock.IsPlaying)
            {
                _grid.Tick(_clock.PlayingElapsed);
            }

            CopyGridToWords();
            CopyPlowingSlots();
            ReportScrapes();
        }

        private void ScrapeUnderBlades()
        {
            var vehicles = _registry.Vehicles;
            _scrapeCount = 0;
            Array.Clear(_plowingSlots, 0, _plowingSlots.Length);
            for (var i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var blade = SnowBlade.Ahead(vehicle.Position, vehicle.Forward, _settings);
                var overPile = _grid.IsOverPile(blade);
                vehicle.SetSpeedFactor(VehicleSpeedSource.SnowPile, _grid.SpeedMultiplierUnder(blade));
                var steps = _grid.Scrape(blade, _limit.LimitFor(vehicle));
                if (steps <= 0)
                {
                    continue;
                }

                _scrapes[_scrapeCount++] = new VehicleScrape(vehicle, steps);
                if (overPile && HasSlot(vehicle))
                {
                    _plowingSlots[vehicle.Slot] = true;
                }
            }
        }

        private void OnMatchRestarted()
        {
            _grid.Reset(NewSeed());
            CopyGridToWords();
        }

        private int NewSeed()
        {
            return HasStateAuthority ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : 0;
        }

        private static bool HasSlot(NetworkVehicle vehicle)
        {
            return vehicle.Slot >= 0 && vehicle.Slot < VehicleWorldDriver.MaxVehicles;
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

        private void CopyPlowingSlots()
        {
            for (var slot = 0; slot < _plowingSlots.Length; slot++)
            {
                PlowingPileBySlot.Set(slot, _plowingSlots[slot]);
            }
        }
    }
}
