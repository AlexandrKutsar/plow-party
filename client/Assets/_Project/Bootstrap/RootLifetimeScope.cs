using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Match.Config;
using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Vehicle.Config;
using PlowParty.Infrastructure.Scenes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private VehicleConfig _vehicleConfig;
        [SerializeField] private SnowConfig _snowConfig;
        [SerializeField] private BucketConfig _bucketConfig;
        [SerializeField] private DropOffConfig _dropOffConfig;
        [SerializeField] private MatchConfig _matchConfig;
        [SerializeField] private HudConfig _hudConfig;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField, Min(30)] private int _targetFrameRate = 60;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_vehicleConfig);
            builder.RegisterInstance(_snowConfig);
            builder.RegisterInstance(_bucketConfig);
            builder.RegisterInstance(_dropOffConfig);
            builder.RegisterInstance(_matchConfig);
            builder.RegisterInstance(_hudConfig);
            builder.RegisterInstance(_cameraConfig);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.RegisterBuildCallback(_ => Application.targetFrameRate = _targetFrameRate);
        }
    }
}
