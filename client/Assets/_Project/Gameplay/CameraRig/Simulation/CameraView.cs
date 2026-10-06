using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public readonly struct CameraView
    {
        private const float MinGroundRayDip = 0.05f;

        public CameraView(float pitch, float yaw, float distance, CameraLens lens)
        {
            Pitch = pitch;
            Yaw = yaw;
            Distance = distance;
            Lens = lens;
        }

        public float Pitch { get; }

        public float Yaw { get; }

        public float Distance { get; }

        public CameraLens Lens { get; }

        public Quaternion Rotation => Quaternion.Euler(Pitch, Yaw, 0f);

        public CameraPose PoseAt(Vector3 focus)
        {
            var rotation = Rotation;
            return new CameraPose(focus - rotation * Vector3.forward * Distance, rotation, Lens);
        }

        public Rect GroundFootprint(float aspect)
        {
            var rotation = Rotation;
            var forward = rotation * Vector3.forward;
            var eye = -forward * Distance;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (var corner = 0; corner < 4; corner++)
            {
                var horizontal = corner % 2 == 0 ? -1f : 1f;
                var vertical = corner < 2 ? -1f : 1f;
                var hit = GroundHit(rotation, eye, forward, horizontal, vertical, aspect);
                min = Vector2.Min(min, hit);
                max = Vector2.Max(max, hit);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private Vector2 GroundHit(Quaternion rotation, Vector3 eye, Vector3 forward, float horizontal, float vertical, float aspect)
        {
            Vector3 origin;
            Vector3 direction;
            if (Lens.Orthographic)
            {
                var halfHeight = Lens.OrthographicSize;
                origin = eye + rotation * new Vector3(horizontal * halfHeight * aspect, vertical * halfHeight, 0f);
                direction = forward;
            }
            else
            {
                var tanVertical = Lens.TanHalfFieldOfView;
                origin = eye;
                direction = (rotation * new Vector3(horizontal * tanVertical * aspect, vertical * tanVertical, 1f)).normalized;
            }

            var dip = Mathf.Max(-direction.y, MinGroundRayDip);
            var hit = origin + direction * (origin.y / dip);
            return new Vector2(hit.x, hit.z);
        }
    }
}
