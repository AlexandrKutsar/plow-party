using PlowParty.Gameplay.CameraRig.Simulation;
using UnityEngine;

namespace PlowParty.Gameplay.CameraRig.Tests
{
    internal static class CameraTestSettings
    {
        public const float TopDownPitch = 90f;
        public const float FootprintHalfSize = 5f;

        public static readonly Rect Arena = Rect.MinMaxRect(-20f, -20f, 20f, 20f);

        public static FollowSettings TopDownFollow()
        {
            return new FollowSettings
            {
                Pitch = TopDownPitch,
                Yaw = 0f,
                RotateWithVehicle = false,
                Distance = 20f,
                Lens = new CameraLens(true, 40f, FootprintHalfSize),
                FollowSmoothTime = 0.3f,
                YawSmoothTime = 0.3f,
                LookAheadTime = 0.5f,
                MaxLookAhead = 3f,
                EdgeOverscan = 1f,
                SnapDistance = 3f,
            };
        }

        public static FollowSettings TopDownRotatingFollow()
        {
            var settings = TopDownFollow();
            settings.RotateWithVehicle = true;
            return settings;
        }

        public static OverviewSettings PerspectiveOverview()
        {
            return new OverviewSettings
            {
                Pitch = 60f,
                Yaw = 0f,
                Lens = new CameraLens(false, 40f, 0f),
                Padding = 0f,
                OrthographicDistance = 50f,
            };
        }

        public static OverviewSettings TopDownOrthographicOverview(float padding)
        {
            return new OverviewSettings
            {
                Pitch = TopDownPitch,
                Yaw = 0f,
                Lens = new CameraLens(true, 40f, 1f),
                Padding = padding,
                OrthographicDistance = 50f,
            };
        }

        public static ShakeSettings Shake()
        {
            return new ShakeSettings
            {
                MaxOffset = 0.5f,
                MaxRoll = 3f,
                Frequency = 20f,
                DecayPerSecond = 2f,
            };
        }

        public static CameraTarget StillTarget(Vector2 position)
        {
            return new CameraTarget(position, 0f, Vector2.zero, Vector2.up);
        }
    }
}
