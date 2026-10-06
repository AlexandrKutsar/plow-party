using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;

namespace PlowParty.Gameplay.DropOff.Network
{
    public sealed class DropOffZone : MonoBehaviour
    {
        public Vector2 Centre => PlaneProjection.ToPlane(transform.position);
    }
}
