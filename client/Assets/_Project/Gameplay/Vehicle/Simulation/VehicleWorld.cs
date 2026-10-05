using System.Collections.Generic;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class VehicleWorld
    {
        private const int MaxContactIterations = 12;
        private const float ContactTolerance = 1e-4f;

        private readonly VehicleSettings _settings;
        private readonly VehicleArena _arena;
        private readonly VehicleState[] _vehicles;
        private readonly VehicleInput[] _inputs;
        private readonly VehicleModifiers[] _modifiers;
        private readonly float[] _ramCooldowns;
        private readonly List<RamEvent> _rams = new List<RamEvent>();

        public VehicleWorld(VehicleSettings settings, VehicleArena arena, int capacity)
        {
            _settings = settings;
            _arena = arena;
            _vehicles = new VehicleState[capacity];
            _inputs = new VehicleInput[capacity];
            _modifiers = new VehicleModifiers[capacity];
            _ramCooldowns = new float[VehiclePairs.Count(capacity)];
        }

        public int Count { get; private set; }

        public IReadOnlyList<RamEvent> Rams => _rams;

        public void Clear()
        {
            Count = 0;
        }

        public int Add(VehicleState state)
        {
            var index = Count;
            _vehicles[index] = state;
            _inputs[index] = VehicleInput.Idle;
            _modifiers[index] = VehicleModifiers.None;
            Count++;
            return index;
        }

        public VehicleState GetVehicle(int index)
        {
            return _vehicles[index];
        }

        public void SetVehicle(int index, VehicleState state)
        {
            _vehicles[index] = state;
        }

        public void SetControl(int index, VehicleInput input, VehicleModifiers modifiers)
        {
            _inputs[index] = input;
            _modifiers[index] = modifiers;
        }

        public float GetRamCooldown(int first, int second)
        {
            return _ramCooldowns[VehiclePairs.Index(first, second)];
        }

        public void SetRamCooldown(int first, int second, float remaining)
        {
            _ramCooldowns[VehiclePairs.Index(first, second)] = remaining;
        }

        public void Tick(float deltaTime)
        {
            _rams.Clear();
            for (var i = 0; i < _ramCooldowns.Length; i++)
            {
                _ramCooldowns[i] = Mathf.Max(0f, _ramCooldowns[i] - deltaTime);
            }

            for (var i = 0; i < Count; i++)
            {
                _vehicles[i] = Integrate(_vehicles[i], _inputs[i], _modifiers[i], deltaTime);
                _modifiers[i] = _modifiers[i].WithoutImpulse();
            }

            ResolveVehicleContacts(true);
            for (var iteration = 0; iteration < MaxContactIterations; iteration++)
            {
                var obstacleContacts = ResolveAllObstacles();
                var vehicleContacts = ResolveVehicleContacts(false);
                if (!obstacleContacts && !vehicleContacts)
                {
                    break;
                }
            }

            ResolveAllObstacles();
        }

        private bool ResolveAllObstacles()
        {
            var touched = false;
            for (var i = 0; i < Count; i++)
            {
                var before = _vehicles[i].Position;
                _vehicles[i] = ResolveObstacles(_vehicles[i]);
                touched |= (before - _vehicles[i].Position).sqrMagnitude > ContactTolerance * ContactTolerance;
            }

            return touched;
        }

        private bool ResolveVehicleContacts(bool allowRams)
        {
            var touched = false;
            for (var i = 0; i < Count; i++)
            {
                for (var j = i + 1; j < Count; j++)
                {
                    touched |= ResolvePair(i, j, allowRams);
                }
            }

            return touched;
        }

        private VehicleState ResolveObstacles(VehicleState state)
        {
            var boxes = _arena.Boxes;
            for (var i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                AxisOf(state, out var start, out var end);
                SegmentMath.ClosestPoints(start, end, box, out var onAxis, out var onBox);
                state = onAxis == onBox
                    ? PushOut(state, InsideBoxNormal(onAxis, box), SegmentMath.DepthInsideBox(onAxis, box) + _settings.Radius)
                    : PushOutOfPoint(state, onAxis, onBox, _settings.Radius);
            }

            var circles = _arena.Circles;
            for (var i = 0; i < circles.Count; i++)
            {
                AxisOf(state, out var start, out var end);
                var onAxis = SegmentMath.ClosestPoint(start, end, circles[i].Center);
                state = PushOutOfPoint(state, onAxis, circles[i].Center, circles[i].Radius + _settings.Radius);
            }

            return state;
        }

        private void AxisOf(VehicleState state, out Vector2 start, out Vector2 end)
        {
            var half = state.Forward * _settings.HalfLength;
            start = state.Position - half;
            end = state.Position + half;
        }

        private VehicleState PushOutOfPoint(VehicleState state, Vector2 onAxis, Vector2 point, float minDistance)
        {
            var offset = onAxis - point;
            var distance = offset.magnitude;
            if (distance >= minDistance)
            {
                return state;
            }

            var normal = distance > 0f ? offset / distance : -state.Forward;
            return PushOut(state, normal, minDistance - distance);
        }

        private VehicleState PushOut(VehicleState state, Vector2 normal, float depth)
        {
            var velocity = state.Velocity;
            var normalSpeed = Vector2.Dot(velocity, normal);
            if (normalSpeed < 0f)
            {
                velocity -= (1f + _settings.Restitution) * normalSpeed * normal;
            }

            return new VehicleState(state.Position + normal * depth, velocity, state.Forward);
        }

        private void TryRam(int first, int second, Vector2 normal, float firstApproach, float secondApproach)
        {
            var pair = VehiclePairs.Index(first, second);
            var firstRams = firstApproach >= secondApproach;
            var rammer = firstRams ? first : second;
            var victim = firstRams ? second : first;
            var approachSpeed = firstRams ? firstApproach : secondApproach;
            var towardVictim = firstRams ? normal : -normal;
            if (_ramCooldowns[pair] > 0f || approachSpeed < _settings.RamMinSpeed)
            {
                return;
            }

            if (Vector2.Angle(_vehicles[victim].Forward, -towardVictim) < _settings.RamMinAngleDegrees)
            {
                return;
            }

            var multiplier = _modifiers[rammer].RamStrengthMultiplier;
            _vehicles[victim] = _vehicles[victim].WithVelocity(_vehicles[victim].Velocity + towardVictim * (_settings.RamKnockback * multiplier));
            _vehicles[rammer] = _vehicles[rammer].WithVelocity(_vehicles[rammer].Velocity - towardVictim * _settings.RamRecoil);
            _ramCooldowns[pair] = _settings.RamCooldown;
            _rams.Add(new RamEvent(rammer, victim, approachSpeed * multiplier));
        }

        private static Vector2 InsideBoxNormal(Vector2 position, BoxObstacle box)
        {
            var local = position - box.Center;
            var gapX = box.HalfExtents.x - Mathf.Abs(local.x);
            var gapY = box.HalfExtents.y - Mathf.Abs(local.y);
            return gapX < gapY ? new Vector2(Mathf.Sign(local.x), 0f) : new Vector2(0f, Mathf.Sign(local.y));
        }

        private bool ResolvePair(int first, int second, bool allowRams)
        {
            var a = _vehicles[first];
            var b = _vehicles[second];
            AxisOf(a, out var firstStart, out var firstEnd);
            AxisOf(b, out var secondStart, out var secondEnd);
            SegmentMath.ClosestPoints(firstStart, firstEnd, secondStart, secondEnd, out var onFirst, out var onSecond);
            var offset = onSecond - onFirst;
            var distance = offset.magnitude;
            var minDistance = _settings.Radius * 2f;
            if (distance >= minDistance - ContactTolerance)
            {
                return false;
            }

            var normal = distance > 0f ? offset / distance : FallbackNormal(a, b);
            var push = normal * ((minDistance - distance) * 0.5f);
            var velocityA = a.Velocity;
            var velocityB = b.Velocity;
            var closingSpeed = Vector2.Dot(velocityA - velocityB, normal);
            if (closingSpeed > 0f)
            {
                var impulse = normal * ((1f + _settings.Restitution) * closingSpeed * 0.5f);
                velocityA -= impulse;
                velocityB += impulse;
            }

            _vehicles[first] = new VehicleState(a.Position - push, velocityA, a.Forward);
            _vehicles[second] = new VehicleState(b.Position + push, velocityB, b.Forward);
            if (allowRams && closingSpeed > 0f)
            {
                TryRam(first, second, normal, Vector2.Dot(a.Velocity, normal), Vector2.Dot(b.Velocity, -normal));
            }

            return true;
        }

        private static Vector2 FallbackNormal(VehicleState a, VehicleState b)
        {
            var centres = b.Position - a.Position;
            return centres.sqrMagnitude > 0f ? centres.normalized : a.Forward;
        }

        private VehicleState Integrate(VehicleState state, VehicleInput input, VehicleModifiers modifiers, float deltaTime)
        {
            if (modifiers.IsImmobilised)
            {
                return new VehicleState(state.Position, Vector2.zero, state.Forward);
            }

            var throttle = input.Move.magnitude;
            var forward = throttle > 0f ? TurnTowards(state.Forward, input.Move, _settings.TurnRateDegrees * deltaTime) : state.Forward;
            var targetVelocity = forward * (_settings.MaxSpeed * modifiers.SpeedMultiplier * throttle);
            var rate = throttle > 0f ? _settings.Acceleration : _settings.Deceleration;
            var velocity = Vector2.MoveTowards(state.Velocity + modifiers.Impulse, targetVelocity, rate * deltaTime);
            return new VehicleState(state.Position + velocity * deltaTime, velocity, forward);
        }

        private static Vector2 TurnTowards(Vector2 forward, Vector2 desired, float maxDegrees)
        {
            var angle = Vector2.SignedAngle(forward, desired);
            var step = Mathf.Clamp(angle, -maxDegrees, maxDegrees) * Mathf.Deg2Rad;
            var cos = Mathf.Cos(step);
            var sin = Mathf.Sin(step);
            return new Vector2(forward.x * cos - forward.y * sin, forward.x * sin + forward.y * cos).normalized;
        }
    }
}
