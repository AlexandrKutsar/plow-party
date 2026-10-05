using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public static class PlaneProjection
    {
        public static Vector2 ToPlane(Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }

        public static Vector3 ToWorld(Vector2 value, float height)
        {
            return new Vector3(value.x, height, value.y);
        }
    }
}
