using System;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer.Unity;

namespace PlowParty.Gameplay.Bucket.Network
{
    public sealed class BucketHost : IStartable, IDisposable
    {
        private readonly BucketRegistry _buckets;
        private readonly VehicleRegistry _vehicles;
        private readonly SnowGridDriver _snow;

        public BucketHost(BucketRegistry buckets, VehicleRegistry vehicles, SnowGridDriver snow)
        {
            _buckets = buckets;
            _vehicles = vehicles;
            _snow = snow;
        }

        public void Start()
        {
            _snow.Scraped += OnScraped;
            _vehicles.Rammed += OnRammed;
        }

        public void Dispose()
        {
            _snow.Scraped -= OnScraped;
            _vehicles.Rammed -= OnRammed;
        }

        public void Spill(NetworkVehicle vehicle, Vector2 point)
        {
            if (!_buckets.TryGet(vehicle, out var bucket))
            {
                return;
            }

            var steps = bucket.TakeSpill();
            if (steps > 0)
            {
                _snow.Spill(point, steps);
            }
        }

        private void OnScraped(VehicleScrape scrape)
        {
            if (_buckets.TryGet(scrape.Vehicle, out var bucket))
            {
                bucket.Collect(scrape.Steps);
            }
        }

        private void OnRammed(VehicleRam ram)
        {
            Spill(ram.Victim, (ram.Rammer.Position + ram.Victim.Position) * 0.5f);
        }
    }
}
