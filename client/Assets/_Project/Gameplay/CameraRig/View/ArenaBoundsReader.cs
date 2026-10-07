using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.View
{
    public static class ArenaBoundsReader
    {
        public static Bounds Read(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(root.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }
}
