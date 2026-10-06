using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public static class OverviewFraming
    {
        public static CameraPose Fit(Rect arena, float groundHeight, OverviewSettings settings, float aspect)
        {
            var rotation = Quaternion.Euler(settings.Pitch, settings.Yaw, 0f);
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            var forward = rotation * Vector3.forward;
            var centre = new Vector3(arena.center.x, groundHeight, arena.center.y);
            var padded = Rect.MinMaxRect(
                arena.xMin - settings.Padding,
                arena.yMin - settings.Padding,
                arena.xMax + settings.Padding,
                arena.yMax + settings.Padding);

            if (settings.Lens.Orthographic)
            {
                var size = OrthographicSize(padded, groundHeight, centre, right, up, aspect);
                return new CameraPose(
                    centre - forward * settings.OrthographicDistance,
                    rotation,
                    settings.Lens.WithOrthographicSize(size));
            }

            var distance = PerspectiveDistance(padded, groundHeight, centre, right, up, forward, settings.Lens.FieldOfView, aspect);
            return new CameraPose(centre - forward * distance, rotation, settings.Lens);
        }

        private static float OrthographicSize(Rect padded, float groundHeight, Vector3 centre, Vector3 right, Vector3 up, float aspect)
        {
            var size = 0f;
            for (var corner = 0; corner < 4; corner++)
            {
                var offset = Corner(padded, groundHeight, corner) - centre;
                size = Mathf.Max(size, Mathf.Abs(Vector3.Dot(offset, up)));
                size = Mathf.Max(size, Mathf.Abs(Vector3.Dot(offset, right)) / aspect);
            }

            return size;
        }

        private static float PerspectiveDistance(
            Rect padded,
            float groundHeight,
            Vector3 centre,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            float fieldOfView,
            float aspect)
        {
            var tanVertical = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            var tanHorizontal = tanVertical * aspect;
            var distance = 0f;
            for (var corner = 0; corner < 4; corner++)
            {
                var offset = Corner(padded, groundHeight, corner) - centre;
                var depth = Vector3.Dot(offset, forward);
                distance = Mathf.Max(distance, Mathf.Abs(Vector3.Dot(offset, right)) / tanHorizontal - depth);
                distance = Mathf.Max(distance, Mathf.Abs(Vector3.Dot(offset, up)) / tanVertical - depth);
            }

            return distance;
        }

        private static Vector3 Corner(Rect rect, float height, int corner)
        {
            var x = corner % 2 == 0 ? rect.xMin : rect.xMax;
            var z = corner < 2 ? rect.yMin : rect.yMax;
            return new Vector3(x, height, z);
        }
    }
}
