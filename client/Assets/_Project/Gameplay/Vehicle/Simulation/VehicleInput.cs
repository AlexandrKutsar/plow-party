using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Simulation
{
    public readonly struct VehicleInput
    {
        public VehicleInput(Vector2 move, bool gadgetPressed)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            GadgetPressed = gadgetPressed;
        }

        public Vector2 Move { get; }

        public bool GadgetPressed { get; }

        public static VehicleInput Idle => default;

        public static VehicleInput Stick(Vector2 move)
        {
            return new VehicleInput(move, false);
        }
    }
}
