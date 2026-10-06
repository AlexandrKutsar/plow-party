using UnityEngine;

namespace PlowParty.Gameplay.Bucket.Simulation
{
    public sealed class BucketRules
    {
        private readonly BucketSettings _settings;

        public BucketRules(BucketSettings settings)
        {
            _settings = settings;
            CapacitySteps = settings.Capacity * settings.StepsPerLoad;
        }

        public int CapacitySteps { get; }

        public int RoomFor(int loadSteps)
        {
            return Mathf.Max(CapacitySteps - loadSteps, 0);
        }

        public int Collect(int loadSteps, int steps)
        {
            return Mathf.Min(loadSteps + steps, CapacitySteps);
        }

        public int SpillSteps(int loadSteps)
        {
            var exact = loadSteps * _settings.SpillShare;
            var nearest = Mathf.RoundToInt(exact);
            var steps = Mathf.Approximately(exact, nearest) ? nearest : Mathf.CeilToInt(exact);
            return Mathf.Clamp(steps, 0, loadSteps);
        }

        public float SpeedMultiplier(int loadSteps)
        {
            var fill = Mathf.Clamp01((float)loadSteps / CapacitySteps);
            return 1f - _settings.MaxSpeedPenalty * fill;
        }

        public int LoadUnits(int loadSteps)
        {
            return loadSteps / _settings.StepsPerLoad;
        }

        public bool IsFull(int loadSteps)
        {
            return loadSteps >= CapacitySteps;
        }
    }
}
