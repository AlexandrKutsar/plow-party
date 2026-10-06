using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public static class VehicleArenaReader
    {
        public static VehicleArena Read(Transform root)
        {
            var arena = VehicleArena.Create();
            foreach (var collider in root.GetComponentsInChildren<Collider>())
            {
                var bounds = collider.bounds;
                var center = PlaneProjection.ToPlane(bounds.center);
                switch (collider)
                {
                    case BoxCollider:
                        arena.AddBox(center, PlaneProjection.ToPlane(bounds.extents));
                        break;
                    case CapsuleCollider:
                    case SphereCollider:
                        arena.AddCircle(center, bounds.extents.x);
                        break;
                }
            }

            return arena;
        }
    }
}
