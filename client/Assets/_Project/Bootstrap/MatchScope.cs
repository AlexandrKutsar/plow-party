using PlowParty.Bootstrap.Adapters;
using PlowParty.Gameplay.Bots.Network;
using PlowParty.Gameplay.Bucket.Network;
using PlowParty.Gameplay.CameraRig.Network;
using PlowParty.Gameplay.CameraRig.Simulation;
using PlowParty.Gameplay.CameraRig.View;
using PlowParty.Gameplay.DropOff.Network;
using PlowParty.Gameplay.Hud.View;
using PlowParty.Gameplay.Match.Network;
using PlowParty.Gameplay.Participants.Network;
using PlowParty.Gameplay.Snow.Network;
using PlowParty.Gameplay.Snow.Simulation;
using PlowParty.Gameplay.Snow.View;
using PlowParty.Gameplay.Vehicle.Network;
using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Tournament.Network;
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
            builder.Register<NetworkScopeBinding>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<NetworkScopeBinding>().Bind());
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
            builder.RegisterComponentInHierarchy<ParticipantRoster>();
            builder.RegisterEntryPoint<MatchSeating>().AsSelf();
            builder.RegisterComponentInHierarchy<BotDriver>();
            RegisterHud(builder);
            RegisterCamera(builder);
            RegisterMatchReport(builder);
            builder.RegisterEntryPoint<MatchSceneQuickStart>();
        }

        private static void RegisterMatchReport(IContainerBuilder builder)
        {
            builder.Register<MatchProgressAdapter>(Lifetime.Singleton).As<IMatchProgress>();
            builder.Register<ConnectionTokenRoster>(Lifetime.Singleton).As<IMatchRoster>();
            builder.Register<MatchReportLinks>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MatchReporter>();
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
            builder.RegisterComponentInHierarchy<WaitingForPlayersView>();
            builder.Register<MatchExitAdapter>(Lifetime.Singleton).As<IMatchExit>();
        }
    }
}
