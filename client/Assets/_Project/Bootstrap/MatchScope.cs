using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.CameraRig.Network;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.CameraRig.View;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.View;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Snow.View;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Infrastructure.Network;
using UnityEngine;
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
            builder.RegisterComponentInHierarchy<MatchDriver>().As<IMatchClock>().As<IMatchResults>().As<ISnowClock>().AsSelf();
            RegisterHud(builder);
            RegisterCamera(builder);
            builder.RegisterEntryPoint<MatchSceneQuickStart>();
        }

        private static void RegisterCamera(IContainerBuilder builder)
        {
            builder.Register<CameraShake>(Lifetime.Singleton).AsSelf().As<ICameraShake>();
            builder.RegisterEntryPoint<CameraRamShake>();
            builder.RegisterEntryPoint<CameraPileShake>();
            builder.RegisterComponentInHierarchy<CameraDirector>();
        }

        private static void RegisterHud(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<Camera>();
            builder.RegisterComponentInHierarchy<MatchTimerView>();
            builder.RegisterComponentInHierarchy<LoadBarView>();
            builder.RegisterComponentInHierarchy<ScorePopupView>();
            builder.RegisterComponentInHierarchy<ScoreListView>();
            builder.RegisterComponentInHierarchy<BlizzardAnnouncementView>();
            builder.RegisterComponentInHierarchy<DropOffArrowView>();
            builder.RegisterComponentInHierarchy<ResultsView>();
            builder.RegisterComponentInHierarchy<VirtualStickView>();
        }
    }
}
