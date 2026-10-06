using PlowParty.Gameplay.Bucket.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Simulation
{
    public sealed class DropOffRules
    {
        private const float RoundingTolerance = 0.001f;

        private readonly DropOffSettings _settings;
        private readonly int _stepsPerLoad;
        private readonly float _unloadStepsPerSecond;

        public DropOffRules(DropOffSettings settings, BucketSettings bucket)
        {
            _settings = settings;
            _stepsPerLoad = bucket.StepsPerLoad;
            var capacitySteps = bucket.Capacity * bucket.StepsPerLoad;
            _unloadStepsPerSecond = settings.FullUnloadDuration > 0f ? capacitySteps / settings.FullUnloadDuration : float.PositiveInfinity;
        }

        public bool IsInZone(Vector2 zoneCentre, Vector2 point)
        {
            return IsWithin(zoneCentre, point, _settings.ZoneRadius);
        }

        public bool IsSnowFree(Vector2 zoneCentre, Vector2 point)
        {
            return IsWithin(zoneCentre, point, _settings.SnowFreeRadius);
        }

        public float MultiplierFor(int loadSteps)
        {
            if (loadSteps <= 0)
            {
                return 0f;
            }

            var load = loadSteps / _stepsPerLoad;
            var multiplier = _settings.BaseMultiplier;
            var reachedMinLoad = int.MinValue;
            foreach (var tier in _settings.MultiplierTiers)
            {
                if (load >= tier.MinLoad && tier.MinLoad > reachedMinLoad)
                {
                    reachedMinLoad = tier.MinLoad;
                    multiplier = tier.Multiplier;
                }
            }

            return multiplier;
        }

        public DeliveryTick Tick(Delivery delivery, bool inZone, int loadSteps, float deltaTime)
        {
            if (!inZone)
            {
                return DeliveryTick.Idle;
            }

            var current = delivery.IsActive ? delivery : new Delivery(MultiplierFor(loadSteps), 0, 0f);
            if (!current.IsActive)
            {
                return DeliveryTick.Idle;
            }

            var elapsed = current.Elapsed + deltaTime;
            var unloaded = Mathf.Clamp(StepsDueBy(elapsed) - current.DeliveredSteps, 0, loadSteps);
            var delivered = current.DeliveredSteps + unloaded;
            var scoreGained = ScoreFor(delivered, current.Multiplier) - ScoreFor(current.DeliveredSteps, current.Multiplier);
            var next = unloaded >= loadSteps ? Delivery.None : new Delivery(current.Multiplier, delivered, elapsed);
            return new DeliveryTick(next, unloaded, scoreGained);
        }

        private int StepsDueBy(float elapsed)
        {
            if (float.IsPositiveInfinity(_unloadStepsPerSecond))
            {
                return int.MaxValue;
            }

            return Mathf.FloorToInt(elapsed * _unloadStepsPerSecond + RoundingTolerance);
        }

        private int ScoreFor(int deliveredSteps, float multiplier)
        {
            return Mathf.FloorToInt(deliveredSteps * multiplier / _stepsPerLoad + RoundingTolerance);
        }

        private static bool IsWithin(Vector2 centre, Vector2 point, float radius)
        {
            return (point - centre).sqrMagnitude <= radius * radius;
        }
    }
}
