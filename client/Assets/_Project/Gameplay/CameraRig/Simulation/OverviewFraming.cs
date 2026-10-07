using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public static class OverviewFraming
    {
        private const int CornerCount = 8;

        public static CameraPose Fit(ArenaVolume arena, OverviewSettings settings, float aspect)
        {
            var rotation = new CameraView(settings.Pitch, settings.Yaw, 0f, settings.Lens).Rotation;
            var right = rotation * Vector3.right;
            var up = rotation * Vector3.up;
            var forward = rotation * Vector3.forward;
            var area = arena.Area;
            var centre = new Vector3(area.center.x, arena.GroundHeight, area.center.y);
            var padded = Rect.MinMaxRect(
                area.xMin - settings.Padding,
                area.yMin - settings.Padding,
                area.xMax + settings.Padding,
                area.yMax + settings.Padding);
            var padding = new ArenaVolume(padded, arena.GroundHeight, arena.TopHeight);

            if (settings.Lens.Orthographic)
            {
                var size = OrthographicSize(padding, centre, right, up, aspect);
                return new CameraPose(
                    centre - forward * settings.OrthographicDistance,
                    rotation,
                    settings.Lens.WithOrthographicSize(size));
            }

            var distance = PerspectiveDistance(padding, centre, right, up, forward, settings.Lens.TanHalfFieldOfView, aspect);
            return new CameraPose(centre - forward * distance, rotation, settings.Lens);
        }

        private static float OrthographicSize(ArenaVolume padded, Vector3 centre, Vector3 right, Vector3 up, float aspect)
        {
            var size = 0f;
            for (var corner = 0; corner < CornerCount; corner++)
            {
                var offset = Corner(padded, corner) - centre;
                size = Mathf.Max(size, Mathf.Abs(Vector3.Dot(offset, up)));
                size = Mathf.Max(size, Mathf.Abs(Vector3.Dot(offset, right)) / aspect);
            }

            return size;
        }

        private static float PerspectiveDistance(
            ArenaVolume padded,
            Vector3 centre,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            float tanVertical,
            float aspect)
        {
            var tanHorizontal = tanVertical * aspect;
            var distance = 0f;
            for (var corner = 0; corner < CornerCount; corner++)
            {
                var offset = Corner(padded, corner) - centre;
                var depth = Vector3.Dot(offset, forward);
                distance = Mathf.Max(distance, Mathf.Abs(Vector3.Dot(offset, right)) / tanHorizontal - depth);
                distance = Mathf.Max(distance, Mathf.Abs(Vector3.Dot(offset, up)) / tanVertical - depth);
            }

            return distance;
        }

        private static Vector3 Corner(ArenaVolume volume, int corner)
        {
            var area = volume.Area;
            var x = (corner & 1) == 0 ? area.xMin : area.xMax;
            var z = (corner & 2) == 0 ? area.yMin : area.yMax;
            var y = (corner & 4) == 0 ? volume.GroundHeight : volume.TopHeight;
            return new Vector3(x, y, z);
        }
    }
}
