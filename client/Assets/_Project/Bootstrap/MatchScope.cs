using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.CameraRig.Network;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Snow.View;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Infrastructure.Network;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<NetworkRunnerEvents>(Lifetime.Singleton);
            builder.Register<NetworkSession>(Lifetime.Singleton);
            builder.Register<VehicleRegistry>(Lifetime.Singleton);
            builder.RegisterEntryPoint<VehicleInputPoller>();
            builder.RegisterComponentInHierarchy<VehicleSpawner>();
            builder.RegisterComponentInHierarchy<VehicleWorldDriver>();
            builder.RegisterComponentInHierarchy<SnowGridDriver>();
            builder.RegisterComponentInHierarchy<SnowGridView>();
            builder.Register<BucketRegistry>(Lifetime.Singleton).AsSelf().As<IScrapeLimit>();
            builder.RegisterEntryPoint<BucketHost>().AsSelf();
            builder.RegisterComponentInHierarchy<DropOffZone>();
            builder.Register<DropOffSnowFreeArea>(Lifetime.Singleton).As<ISnowFreeArea>();
            builder.Register<DeliveryRegistry>(Lifetime.Singleton).AsSelf().As<IScoreReader>();
            builder.RegisterEntryPoint<DropOffHost>();
            builder.Register<CameraShake>(Lifetime.Singleton).AsSelf().As<ICameraShake>();
            builder.RegisterEntryPoint<CameraRamShake>();            builder.RegisterEntryPoint<MatchSceneQuickStart>();
        }
    }
}
