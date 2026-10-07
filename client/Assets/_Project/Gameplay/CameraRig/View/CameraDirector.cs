using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.Vehicle.Network;
using UnityEngine;
using VContainer;

namespace PlowParty.Gameplay.CameraRig.View
{
    public sealed class CameraDirector : MonoBehaviour
    {
        private static readonly int PresetCount = System.Enum.GetValues(typeof(CameraPreset)).Length;

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _arenaRoot;

        private readonly CameraFollow _follow = new CameraFollow();
        private CameraConfig _config;
        private VehicleRegistry _vehicles;
        private CameraShake _shake;
        private ArenaVolume _arena;
        private bool _hasArena;

        public CameraPreset ActivePreset { get; private set; }

        [Inject]
        public void Construct(CameraConfig config, VehicleRegistry vehicles, CameraShake shake)
        {
            _config = config;
            _vehicles = vehicles;
            _shake = shake;
            ActivePreset = config.DefaultPreset;
        }

        public void CyclePreset()
        {
            ActivePreset = (CameraPreset)(((int)ActivePreset + 1) % PresetCount);
            _follow.Release();
        }

        private void Start()
        {
            _arena = ArenaVolume.FromBounds(ArenaBoundsReader.Read(_arenaRoot));
            _hasArena = true;
        }

        private void LateUpdate()
        {
            if (_config == null || !_hasArena)
            {
                return;
            }

            var pose = CurrentPose();
            var shake = _shake.Step(Time.deltaTime, _config.ToShakeSettings());
            Apply(pose, shake);
        }

        private CameraPose CurrentPose()
        {
            if (ActivePreset != CameraPreset.Overview && _vehicles.TryGetLocal(out var vehicle))
            {
                return _follow.Step(TargetOf(vehicle), _config.ToFollowSettings(ActivePreset), _arena.Area, _camera.aspect, Time.deltaTime);
            }

            _follow.Release();
            return OverviewFraming.Fit(_arena, _config.ToOverviewSettings(), _camera.aspect);
        }

        private static CameraTarget TargetOf(NetworkVehicle vehicle)
        {
            var vehicleTransform = vehicle.transform;
            var position = vehicleTransform.position;
            return new CameraTarget(
                PlaneProjection.ToPlane(position),
                position.y,
                vehicle.Velocity,
                PlaneProjection.ToPlane(vehicleTransform.forward));
        }

        private void Apply(CameraPose pose, CameraShakeSample shake)
        {
            _camera.orthographic = pose.Lens.Orthographic;
            if (pose.Lens.Orthographic)
            {
                _camera.orthographicSize = pose.Lens.OrthographicSize;
            }
            else
            {
                _camera.fieldOfView = pose.Lens.FieldOfView;
            }

            var position = pose.Position + pose.Rotation * new Vector3(shake.Offset.x, shake.Offset.y, 0f);
            var rotation = pose.Rotation * Quaternion.Euler(0f, 0f, shake.Roll);
            _camera.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
