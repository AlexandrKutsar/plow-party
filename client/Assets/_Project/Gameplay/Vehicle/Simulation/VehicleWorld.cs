using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public sealed class VehicleWorld
    {
        private readonly VehicleSettings _settings;
        private readonly VehicleState[] _vehicles;
        private readonly VehicleInput[] _inputs;
        private readonly VehicleModifiers[] _modifiers;

        public VehicleWorld(VehicleSettings settings, int capacity)
        {
            _settings = settings;
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
