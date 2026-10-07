using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Simulation
{
    public sealed class CameraFollow
    {
        private Vector2 _focusVelocity;
        private float _yawVelocity;
        private Vector2 _lastTargetPosition;
        private bool _hasTarget;

        public Vector2 Focus { get; private set; }

        public float Yaw { get; private set; }

        public void Release()
        {
            _hasTarget = false;
        }

        public CameraPose Step(CameraTarget target, FollowSettings settings, Rect arena, float aspect, float deltaTime)
        {
            var targetYaw = TargetYaw(target, settings);
            if (ShouldSnap(target, settings))
            {
                Snap(target, settings, targetYaw, arena, aspect);
            }
            else
            {
                Yaw = Mathf.SmoothDampAngle(Yaw, targetYaw, ref _yawVelocity, settings.YawSmoothTime, Mathf.Infinity, deltaTime);
                var desired = DesiredFocus(target, settings, Yaw, arena, aspect);
                Focus = Vector2.SmoothDamp(Focus, desired, ref _focusVelocity, settings.FollowSmoothTime, Mathf.Infinity, deltaTime);
            }

            _lastTargetPosition = target.Position;
            _hasTarget = true;
            return settings.ViewAt(Yaw).PoseAt(new Vector3(Focus.x, target.Height, Focus.y));
        }

        private bool ShouldSnap(CameraTarget target, FollowSettings settings)
        {
            return !_hasTarget || (target.Position - _lastTargetPosition).sqrMagnitude > settings.SnapDistance * settings.SnapDistance;
        }

        private void Snap(CameraTarget target, FollowSettings settings, float targetYaw, Rect arena, float aspect)
        {
            Yaw = targetYaw;
            _yawVelocity = 0f;
            Focus = DesiredFocus(target, settings, Yaw, arena, aspect);
            _focusVelocity = Vector2.zero;
        }

        private float TargetYaw(CameraTarget target, FollowSettings settings)
        {
            if (!settings.RotateWithVehicle)
            {
                return settings.Yaw;
            }

            if (target.Forward.sqrMagnitude <= 0f)
            {
                return Yaw;
            }

            return Mathf.Atan2(target.Forward.x, target.Forward.y) * Mathf.Rad2Deg;
        }

        private static Vector2 DesiredFocus(CameraTarget target, FollowSettings settings, float yaw, Rect arena, float aspect)
        {
            var lookAhead = Vector2.ClampMagnitude(target.Velocity * settings.LookAheadTime, settings.MaxLookAhead);
            var footprint = settings.ViewAt(yaw).GroundFootprint(aspect);
            var focus = target.Position + lookAhead;
            return new Vector2(
                ClampAxis(focus.x, arena.xMin, arena.xMax, footprint.xMin, footprint.xMax, settings.EdgeOverscan),
                ClampAxis(focus.y, arena.yMin, arena.yMax, footprint.yMin, footprint.yMax, settings.EdgeOverscan));
        }

        private static float ClampAxis(float focus, float arenaMin, float arenaMax, float footprintMin, float footprintMax, float overscan)
        {
            var lowest = arenaMin - footprintMin - overscan;
            var highest = arenaMax - footprintMax + overscan;
            if (lowest > highest)
            {
                return (lowest + highest) * 0.5f;
            }

            return Mathf.Clamp(focus, lowest, highest);
        }
    }
}
