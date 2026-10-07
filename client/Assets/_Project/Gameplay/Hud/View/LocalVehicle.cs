using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;

namespace PlowParty.Gameplay.Hud.View
{
    public static class LocalVehicle
    {
        public static bool TryFind(VehicleRegistry registry, out NetworkVehicle local)
        {
            var vehicles = registry.Vehicles;
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].HasInputAuthority)
                {
                    local = vehicles[i];
                    return true;
                }
            }

            local = null;
            return false;
        }

        public static bool TryProjectAbove(Camera camera, NetworkVehicle vehicle, float height, out Vector2 screenPoint)
        {
            var projected = camera.WorldToScreenPoint(vehicle.transform.position + Vector3.up * height);
            screenPoint = projected;
            return projected.z > 0f;
        }
    }
}
