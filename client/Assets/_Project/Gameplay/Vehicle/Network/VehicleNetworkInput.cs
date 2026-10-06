using Fusion;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public struct VehicleNetworkInput : INetworkInput
    {
        public Vector2 Move { get; set; }

        public NetworkBool GadgetPressed { get; set; }
    }
}
