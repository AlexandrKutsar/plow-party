using System.Collections.Generic;
using UnityEngine;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotWorld
    {
        private readonly BotVehicle[] _vehicles;
        private readonly int[] _tierLoads;

        public BotWorld(NavGrid grid, Vector2 dropOffCentre, float dropOffRadius, int capacity, IReadOnlyList<int> tierLoads, int maxVehicles)
        {
            Grid = grid;
            Pathfinder = new NavPathfinder(grid);
            Snow = new BotSnowMap(grid);
            DropOffCentre = dropOffCentre;
            DropOffRadius = dropOffRadius;
            Capacity = capacity;
            _vehicles = new BotVehicle[maxVehicles];
            _tierLoads = new int[tierLoads.Count];
            for (var i = 0; i < _tierLoads.Length; i++)
            {
                _tierLoads[i] = tierLoads[i];
            }
        }

        public NavGrid Grid { get; }

        public NavPathfinder Pathfinder { get; }

        public BotSnowMap Snow { get; }

        public Vector2 DropOffCentre { get; }

        public float DropOffRadius { get; }

        public int Capacity { get; }

        public float PlayingRemaining { get; set; }

        public BotVehicle[] Vehicles => _vehicles;

        public int VehicleCount { get; private set; }

        public void ClearVehicles()
        {
            VehicleCount = 0;
        }

        public void AddVehicle(BotVehicle vehicle)
        {
            if (VehicleCount < _vehicles.Length)
            {
                _vehicles[VehicleCount++] = vehicle;
            }
        }

        public bool TryGetVehicle(int slot, out BotVehicle vehicle)
        {
            for (var i = 0; i < VehicleCount; i++)
            {
                if (_vehicles[i].Slot == slot)
                {
                    vehicle = _vehicles[i];
                    return true;
                }
            }

            vehicle = default;
            return false;
        }

        public int NextTierLoad(int load)
        {
            var next = 0;
            for (var i = 0; i < _tierLoads.Length; i++)
            {
                if (_tierLoads[i] > load && (next == 0 || _tierLoads[i] < next))
                {
                    next = _tierLoads[i];
                }
            }

            return next;
        }
    }
}
