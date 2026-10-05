using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class VehicleWorld
    {
        private readonly VehicleSettings _settings;
        private readonly VehicleArena _arena;
        private readonly VehicleState[] _vehicles;
        private readonly VehicleInput[] _inputs;
        private readonly VehicleModifiers[] _modifiers;

        public VehicleWorld(VehicleSettings settings, VehicleArena arena, int capacity)
        {
            _settings = settings;
            _arena = arena;
            _vehicles = new VehicleState[capacity];
            _inputs = new VehicleInput[capacity];
            _modifiers = new VehicleModifiers[capacity];
        }

        public int Count { get; private set; }

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

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                _vehicles[i] = Integrate(_vehicles[i], _inputs[i], _modifiers[i], deltaTime);
                _modifiers[i] = _modifiers[i].WithoutImpulse();
            }

            for (var i = 0; i < Count; i++)
            {
                for (var j = i + 1; j < Count; j++)
                {
                    ResolvePair(i, j);
                }
            }

            for (var i = 0; i < Count; i++)
            {
                _vehicles[i] = ResolveObstacles(_vehicles[i]);
            }
        }

        private VehicleState ResolveObstacles(VehicleState state)
        {
            var boxes = _arena.Boxes;
            for (var i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                var min = box.Center - box.HalfExtents;
                var max = box.Center + box.HalfExtents;
                var closest = new Vector2(Mathf.Clamp(state.Position.x, min.x, max.x), Mathf.Clamp(state.Position.y, min.y, max.y));
                state = closest == state.Position
                    ? PushOut(state, InsideBoxNormal(state.Position, box), DistanceToBoxEdge(state.Position, box) + _settings.Radius)
                    : PushOutOfPoint(state, closest, _settings.Radius);
            }

            var circles = _arena.Circles;
            for (var i = 0; i < circles.Count; i++)
            {
                state = PushOutOfPoint(state, circles[i].Center, circles[i].Radius + _settings.Radius);
            }

            return state;
        }

        private VehicleState PushOutOfPoint(VehicleState state, Vector2 point, float minDistance)
        {
            var offset = state.Position - point;
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

        private static Vector2 InsideBoxNormal(Vector2 position, BoxObstacle box)
        {
            var local = position - box.Center;
            var gapX = box.HalfExtents.x - Mathf.Abs(local.x);
            var gapY = box.HalfExtents.y - Mathf.Abs(local.y);
            return gapX < gapY ? new Vector2(Mathf.Sign(local.x), 0f) : new Vector2(0f, Mathf.Sign(local.y));
        }

        private static float DistanceToBoxEdge(Vector2 position, BoxObstacle box)
        {
            var local = position - box.Center;
            return Mathf.Min(box.HalfExtents.x - Mathf.Abs(local.x), box.HalfExtents.y - Mathf.Abs(local.y));
        }

        private void ResolvePair(int first, int second)
        {
            var a = _vehicles[first];
            var b = _vehicles[second];
            var offset = b.Position - a.Position;
            var distance = offset.magnitude;
            var minDistance = _settings.Radius * 2f;
            if (distance >= minDistance)
            {
                return;
            }

            var normal = distance > 0f ? offset / distance : a.Forward;
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
