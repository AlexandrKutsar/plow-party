using System.Collections.Generic;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using Random = System.Random;

namespace PlowParty.Gameplay.Bots.Simulation
{
    public sealed class BotBrain
    {
        private const int NoSlot = -1;
        private const float ZoneMargin = 0.3f;
        private const float ArrivalSlowdownDistance = 3f;
        private const float MinArrivalThrottle = 0.25f;
        private const float GoalMovedDistance = 1.5f;
        private const float UnstuckThrottle = 0.8f;
        private const float UnstuckSpreadDegrees = 45f;
        private const int OpenDirectionRadius = 2;
        private const float DetourFactor = 1.3f;
        private const float KeptSnowTargetRichness = 0.3f;
        private const float SnowTargetPatience = 3f;

        private readonly BotSettings _settings;
        private readonly BotUtility _utility;
        private readonly BotStuckWatch _stuck;
        private readonly Random _random;
        private readonly List<Vector2> _path = new List<Vector2>();
        private readonly Vector2[] _others;
        private readonly int[] _switchesTo = new int[(int)BotAction.Evade + 1];

        private BotAction _pendingAction;
        private float _pendingAt;
        private bool _hasPending;
        private Vector2 _goal;
        private bool _isGoalReached;
        private float _goalSetTime;
        private int _targetSlot = NoSlot;
        private float _nextDecisionTime;
        private float _noiseDegrees;
        private float _unstuckUntil = float.MinValue;
        private Vector2 _unstuckDirection;
        private int _pathIndex;
        private Vector2 _pathGoal;
        private float _lastPlanTime = float.MinValue;
        private Perception _perception;

        public BotBrain(BotProfile profile, BotSettings settings, int seed, int maxVehicles)
        {
            Profile = profile;
            _settings = settings;
            _utility = new BotUtility(settings);
            _stuck = new BotStuckWatch(settings);
            _random = new Random(seed);
            _others = new Vector2[maxVehicles];
        }

        public BotProfile Profile { get; }

        public BotAction Action { get; private set; }

        public int StuckEvents { get; private set; }

        public int SwitchesTo(BotAction action)
        {
            return _switchesTo[(int)action];
        }

        public float StillFor(float time)
        {
            return _stuck.StillFor(time);
        }

        public void Reset(float time)
        {
            Action = BotAction.Collect;
            _hasPending = false;
            _isGoalReached = true;
            _targetSlot = NoSlot;
            _path.Clear();
            _unstuckUntil = float.MinValue;
            _lastPlanTime = float.MinValue;
            _nextDecisionTime = time;
            StuckEvents = 0;
            _stuck.Stop();
            System.Array.Clear(_switchesTo, 0, _switchesTo.Length);
        }

        public VehicleInput Step(BotWorld world, int slot, float time)
        {
            if (!world.TryGetVehicle(slot, out var self))
            {
                return VehicleInput.Idle;
            }

            if (IsUnloading(world, self, time))
            {
                _stuck.Observe(self.Position, time, false);
                return VehicleInput.Idle;
            }

            if (time >= _nextDecisionTime)
            {
                Decide(world, self, time);
                _nextDecisionTime = time + Mathf.Lerp(Profile.DecisionIntervalMin, Profile.DecisionIntervalMax, (float)_random.NextDouble());
            }

            if (_hasPending && time >= _pendingAt)
            {
                Commit(world, self, _pendingAction, time);
            }

            if (_stuck.Observe(self.Position, time, true))
            {
                BeginUnstuck(world, self, time);
            }

            if (time < _unstuckUntil)
            {
                return VehicleInput.Stick(_unstuckDirection * UnstuckThrottle);
            }

            return VehicleInput.Stick(Drive(world, self, time));
        }

        private Vector2 Drive(BotWorld world, BotVehicle self, float time)
        {
            var goal = CurrentGoal(world);
            var aim = NextAim(world, self, goal, time);
            var ignored = Action == BotAction.Ram ? _targetSlot : NoSlot;
            var stick = BotSteering.Steer(self, aim, world.Vehicles, world.VehicleCount, ignored, _settings, _noiseDegrees, Profile.ThrottleCap);
            var remaining = Vector2.Distance(self.Position, goal);
            if (Action == BotAction.Deliver && remaining < ArrivalSlowdownDistance)
            {
                stick *= Mathf.Max(MinArrivalThrottle, remaining / ArrivalSlowdownDistance);
            }

            if (!_isGoalReached && HasArrived(remaining))
            {
                _isGoalReached = true;
                _nextDecisionTime = time;
            }

            return stick;
        }

        private bool HasArrived(float remaining)
        {
            switch (Action)
            {
                case BotAction.Collect:
                    return remaining <= _settings.SnowTargetReach;
                case BotAction.Evade:
                    return remaining <= _settings.WaypointReach;
                default:
                    return false;
            }
        }

        private bool IsUnloading(BotWorld world, BotVehicle self, float time)
        {
            if (Action != BotAction.Deliver || !IsInZone(world, self.Position))
            {
                return false;
            }

            if (self.Load > 0)
            {
                return true;
            }

            _hasPending = false;
            Action = BotAction.Collect;
            _isGoalReached = true;
            _nextDecisionTime = time;
            return false;
        }

        private static bool IsInZone(BotWorld world, Vector2 position)
        {
            var reach = world.DropOffRadius - ZoneMargin;
            return (position - world.DropOffCentre).sqrMagnitude <= reach * reach;
        }

        private void Decide(BotWorld world, BotVehicle self, float time)
        {
            _perception = Perceive(world, self);
            _noiseDegrees = ((float)_random.NextDouble() * 2f - 1f) * Profile.SteeringNoiseDegrees;
            var current = _hasPending ? _pendingAction : Action;
            var choice = _utility.Choose(_perception.Situation, Profile, current, _random.NextDouble());
            if (choice == Action)
            {
                Commit(world, self, choice, time);
                return;
            }

            if (_hasPending && choice == _pendingAction)
            {
                return;
            }

            _hasPending = true;
            _pendingAction = choice;
            _pendingAt = time + Profile.ReactionDelay;
        }

        private void Commit(BotWorld world, BotVehicle self, BotAction action, float time)
        {
            var keepsSnowTarget = action == BotAction.Collect && Action == BotAction.Collect && IsSnowTargetWorthKeeping(world, time);
            if (action != Action)
            {
                _switchesTo[(int)action]++;
            }

            _hasPending = false;
            Action = action;
            _targetSlot = NoSlot;
            if (keepsSnowTarget)
            {
                return;
            }

            _isGoalReached = false;
            _goalSetTime = time;
            switch (action)
            {
                case BotAction.Collect:
                    _goal = _perception.HasSnow ? _perception.SnowTarget : world.DropOffCentre;
                    break;
                case BotAction.Deliver:
                    _goal = StandPoint(world, self.Position);
                    break;
                case BotAction.ChasePile:
                    _goal = _perception.PileTarget;
                    break;
                case BotAction.Ram:
                    _targetSlot = _perception.RamSlot;
                    break;
                default:
                    _goal = _perception.EvadeTarget;
                    break;
            }
        }

        private bool IsSnowTargetWorthKeeping(BotWorld world, float time)
        {
            return !_isGoalReached
                && time - _goalSetTime < SnowTargetPatience
                && world.Snow.RichnessShare(world.Grid.CellOf(_goal)) >= KeptSnowTargetRichness;
        }

        private Vector2 CurrentGoal(BotWorld world)
        {
            if (Action != BotAction.Ram)
            {
                return _goal;
            }

            if (world.TryGetVehicle(_targetSlot, out var target))
            {
                _goal = target.Position + target.Velocity * _settings.RamLeadTime;
            }
            else
            {
                _nextDecisionTime = float.MinValue;
            }

            return _goal;
        }

        private Vector2 NextAim(BotWorld world, BotVehicle self, Vector2 goal, float time)
        {
            var grid = world.Grid;
            if (grid.HasLineOfSight(self.Position, goal))
            {
                _path.Clear();
                return goal;
            }

            var goalMoved = (goal - _pathGoal).sqrMagnitude > GoalMovedDistance * GoalMovedDistance;
            if ((_path.Count == 0 || goalMoved) && time - _lastPlanTime >= _settings.ReplanInterval)
            {
                _lastPlanTime = time;
                _pathGoal = goal;
                _pathIndex = 0;
                if (!world.Pathfinder.TryFindPath(self.Position, goal, _path))
                {
                    _path.Clear();
                }
            }

            if (_path.Count == 0)
            {
                return goal;
            }

            while (_pathIndex < _path.Count - 1
                && ((self.Position - _path[_pathIndex]).sqrMagnitude <= _settings.WaypointReach * _settings.WaypointReach
                    || grid.HasLineOfSight(self.Position, _path[_pathIndex + 1])))
            {
                _pathIndex++;
            }

            return _path[_pathIndex];
        }

        private void BeginUnstuck(BotWorld world, BotVehicle self, float time)
        {
            StuckEvents++;
            var open = world.Grid.OpenDirection(self.Position, OpenDirectionRadius);
            var direction = open.sqrMagnitude > 0f ? open : -self.Forward;
            var spread = ((float)_random.NextDouble() * 2f - 1f) * UnstuckSpreadDegrees * Mathf.Deg2Rad;
            _unstuckDirection = new Vector2(
                direction.x * Mathf.Cos(spread) - direction.y * Mathf.Sin(spread),
                direction.x * Mathf.Sin(spread) + direction.y * Mathf.Cos(spread));
            _unstuckUntil = time + _settings.UnstuckDuration;
            _stuck.Restart(self.Position, time);
            _path.Clear();
            _lastPlanTime = float.MinValue;
        }

        private Vector2 StandPoint(BotWorld world, Vector2 position)
        {
            var offset = position - world.DropOffCentre;
            var direction = offset.sqrMagnitude > 0f ? offset.normalized : Vector2.up;
            var stand = world.DropOffCentre + direction * (world.DropOffRadius - _settings.DropOffStandDepth);
            return world.Grid.NearestWalkablePoint(stand);
        }

        private Perception Perceive(BotWorld world, BotVehicle self)
        {
            var otherCount = 0;
            for (var i = 0; i < world.VehicleCount; i++)
            {
                if (world.Vehicles[i].Slot != self.Slot)
                {
                    _others[otherCount++] = world.Vehicles[i].Position;
                }
            }

            var perception = new Perception();
            perception.HasSnow = world.Snow.TryFindSnow(
                self.Position, self.Forward, _others, otherCount, _settings, Profile.MistakeChance, _random, out var snowTarget, out var richness);
            perception.SnowTarget = snowTarget;
            var hasPile = world.Snow.TryFindPile(self.Position, _settings, out var pile, out _);
            perception.PileTarget = world.Grid.NearestWalkablePoint(pile);
            var ramSlot = FindRamTarget(world, self, out var ramLoad, out var ramDistance);
            perception.RamSlot = ramSlot;
            var threat = FindThreat(world, self, out var evadeTarget);
            perception.EvadeTarget = evadeTarget;
            perception.Situation = new BotSituation
            {
                Load = self.Load,
                Capacity = world.Capacity,
                NextTierLoad = world.NextTierLoad(self.Load),
                SnowRichness = richness,
                DropOffTravelTime = Vector2.Distance(self.Position, StandPoint(world, self.Position)) * DetourFactor / _settings.CruiseSpeed,
                PlayingRemaining = world.PlayingRemaining,
                HasPile = hasPile,
                PileDistance = Vector2.Distance(self.Position, pile),
                HasRamTarget = ramSlot != NoSlot,
                RamTargetLoad = ramLoad,
                RamTargetDistance = ramDistance,
                ThreatLevel = threat,
            };
            return perception;
        }

        private int FindRamTarget(BotWorld world, BotVehicle self, out int load, out float distance)
        {
            var best = NoSlot;
            var bestScore = 0f;
            load = 0;
            distance = 0f;
            for (var i = 0; i < world.VehicleCount; i++)
            {
                var other = world.Vehicles[i];
                var gap = Vector2.Distance(self.Position, other.Position);
                if (other.Slot == self.Slot || other.Load < _settings.RamMinVictimLoad || gap > _settings.RamRange)
                {
                    continue;
                }

                if (IsInZone(world, other.Position) || !world.Grid.HasLineOfSight(self.Position, other.Position))
                {
                    continue;
                }

                var score = other.Load / (1f + gap / _settings.RamFalloff);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = other.Slot;
                    load = other.Load;
                    distance = gap;
                }
            }

            return best;
        }

        private float FindThreat(BotWorld world, BotVehicle self, out Vector2 evadeTarget)
        {
            var level = 0f;
            evadeTarget = self.Position;
            for (var i = 0; i < world.VehicleCount; i++)
            {
                var other = world.Vehicles[i];
                var away = self.Position - other.Position;
                var gap = away.magnitude;
                if (other.Slot == self.Slot || gap <= 0f || gap > _settings.ThreatRange)
                {
                    continue;
                }

                var closing = Vector2.Dot(other.Velocity, away / gap);
                if (closing < _settings.ThreatClosingSpeed || Vector2.Angle(other.Forward, away) > _settings.ThreatConeDegrees)
                {
                    continue;
                }

                var threat = Mathf.Clamp01(closing / (_settings.CruiseSpeed + 1f)) * (1f - gap / _settings.ThreatRange);
                if (threat > level)
                {
                    level = threat;
                    evadeTarget = world.Grid.NearestWalkablePoint(self.Position + Sidestep(other.Velocity, away) * _settings.EvadeDistance);
                }
            }

            return level;
        }

        private static Vector2 Sidestep(Vector2 threatVelocity, Vector2 away)
        {
            var side = new Vector2(-threatVelocity.y, threatVelocity.x).normalized;
            return Vector2.Dot(side, away) >= 0f ? side : -side;
        }

        private struct Perception
        {
            public BotSituation Situation { get; set; }

            public bool HasSnow { get; set; }

            public Vector2 SnowTarget { get; set; }

            public Vector2 PileTarget { get; set; }

            public int RamSlot { get; set; }

            public Vector2 EvadeTarget { get; set; }
        }
    }
}
