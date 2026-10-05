using PlowParty.Gameplay.Vehicle.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public static class VehicleArenaReader
    {
        public static VehicleArena Read(Transform root)
        {
            var arena = VehicleArena.Empty;
            foreach (var box in root.GetComponentsInChildren<BoxCollider>())
            {
                var bounds = box.bounds;
                arena.AddBox(new Vector2(bounds.center.x, bounds.center.z), new Vector2(bounds.extents.x, bounds.extents.z));
            }

            foreach (var capsule in root.GetComponentsInChildren<CapsuleCollider>())
            {
                var bounds = capsule.bounds;
                arena.AddCircle(new Vector2(bounds.center.x, bounds.center.z), bounds.extents.x);
            }

            foreach (var sphere in root.GetComponentsInChildren<SphereCollider>())
            {
                var bounds = sphere.bounds;
                arena.AddCircle(new Vector2(bounds.center.x, bounds.center.z), bounds.extents.x);
            }

            return arena;
        }
    }
}
