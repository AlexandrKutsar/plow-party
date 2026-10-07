using Fusion;
using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class NetworkVehicle : NetworkBehaviour
    {
        private VehicleRegistry _registry;

        [Networked] public int Slot { get; set; }

        [Networked] public Vector2 Position { get; private set; }

        [Networked] public Vector2 Velocity { get; private set; }

        [Networked] public Vector2 Forward { get; private set; }

        [Networked] public Vector2 LastMove { get; private set; }

        [Networked] public NetworkBool GadgetPressed { get; private set; }

        public const int SpeedSourceCount = (int)VehicleSpeedSource.SnowPile + 1;

        [Networked, Capacity(SpeedSourceCount)] private NetworkArray<float> SpeedFactors { get; }

        [Networked] public NetworkBool IsImmobilised { get; set; }

        [Networked] public float RamStrengthMultiplier { get; set; }

        [Networked] private Vector2 PendingImpulse { get; set; }

        [Inject]
        public void Construct(VehicleRegistry registry)
        {
            _registry = registry;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Position = PlaneProjection.ToPlane(transform.position);
                Forward = PlaneProjection.ToPlane(transform.forward).normalized;
                for (var source = 0; source < SpeedSourceCount; source++)
                {
                    SpeedFactors.Set(source, VehicleModifiers.None.SpeedMultiplier);
                }

                RamStrengthMultiplier = VehicleModifiers.None.RamStrengthMultiplier;
            }

            Runner.SetIsSimulated(Object, true);
            _registry.Add(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _registry.Remove(this);
        }

        public float SpeedMultiplier
        {
            get
            {
                var product = 1f;
                for (var source = 0; source < SpeedSourceCount; source++)
                {
                    product *= SpeedFactors[source];
                }

                return product;
            }
        }

        public void SetSpeedFactor(VehicleSpeedSource source, float factor)
        {
            SpeedFactors.Set((int)source, factor);
        }

        public void AddImpulse(Vector2 impulse)
        {
            PendingImpulse += impulse;
        }

        public VehicleState ReadState()
        {
            return new VehicleState(Position, Velocity, Forward);
        }

        public VehicleInput ReadInput()
        {
            if (Runner.TryGetInputForPlayer<VehicleNetworkInput>(Object.InputAuthority, out var input))
            {
                LastMove = input.Move;
                GadgetPressed = input.GadgetPressed;
            }

            return new VehicleInput(LastMove, GadgetPressed);
        }

        public VehicleModifiers ConsumeModifiers()
        {
            var modifiers = new VehicleModifiers(SpeedMultiplier, IsImmobilised, PendingImpulse, RamStrengthMultiplier);
            PendingImpulse = Vector2.zero;
            return modifiers;
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
            transform.position = PlaneProjection.ToWorld(position, transform.position.y);
            if (forward.sqrMagnitude > 0f)
            {
                transform.rotation = Quaternion.LookRotation(PlaneProjection.ToWorld(forward, 0f));
            }
        }
    }
}
