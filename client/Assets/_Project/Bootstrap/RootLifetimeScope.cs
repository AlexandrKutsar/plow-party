using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.DropOff.Config;
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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_vehicleConfig);
            builder.RegisterInstance(_snowConfig);
            builder.RegisterInstance(_bucketConfig);
            builder.RegisterInstance(_dropOffConfig);
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}
