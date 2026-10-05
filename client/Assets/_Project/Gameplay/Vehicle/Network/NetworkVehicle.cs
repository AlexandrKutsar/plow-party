using Fusion;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class NetworkVehicle : NetworkBehaviour
    {
        private VehicleRegistry _registry;

        [Networked] public Vector2 Position { get; set; }

        [Networked] public Vector2 Velocity { get; set; }

        [Networked] public Vector2 Forward { get; set; }

        [Networked] public Vector2 LastMove { get; set; }

        [Inject]
        public void Construct(VehicleRegistry registry)
        {
            _registry = registry;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Position = ToPlane(transform.position);
                Forward = ToPlane(transform.forward).normalized;
            }

            Runner.SetIsSimulated(Object, true);
            _registry.Add(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(this);
        }

        public VehicleState ReadState()
        {
            return new VehicleState(Position, Velocity, Forward);
        }

        public void WriteState(VehicleState state)
        {
            Position = state.Position;
            Velocity = state.Velocity;
            Forward = state.Forward;
        }

        public override void Render()
        {
            var interpolator = new NetworkBehaviourBufferInterpolator(this);
            var position = interpolator.Vector2(nameof(Position));
            var forward = interpolator.Vector2(nameof(Forward));
            transform.position = new Vector3(position.x, transform.position.y, position.y);
            if (forward.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.y));
            }
        }

        private static Vector2 ToPlane(Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }
    }
}
