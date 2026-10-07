using System;
using PlowParty.Gameplay.Bots.Config;
using PlowParty.Gameplay.Bucket.Config;
using PlowParty.Gameplay.CameraRig.Config;
using PlowParty.Gameplay.DropOff.Config;
using PlowParty.Gameplay.Hud.Config;
using PlowParty.Gameplay.Match.Config;
using PlowParty.Gameplay.Participants.Config;
using PlowParty.Gameplay.Snow.Config;
using PlowParty.Gameplay.Vehicle.Config;
using PlowParty.Infrastructure.Backend;
using PlowParty.Infrastructure.Network;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Infrastructure.Session;
using PlowParty.Infrastructure.Storage;
using PlowParty.Meta.Account;
using PlowParty.Meta.Session;
using PlowParty.Meta.Session.Config;
using PlowParty.Meta.Tournament;
using PlowParty.Meta.Tournament.Config;
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
        [SerializeField] private ParticipantsConfig _participantsConfig;
        [SerializeField] private BotConfig _botConfig;
        [SerializeField] private BackendConfig _backendConfig;
        [SerializeField] private MatchmakingConfig _matchmakingConfig;
        [SerializeField] private TournamentConfig _tournamentConfig;
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
            builder.RegisterInstance(_participantsConfig);
            builder.RegisterInstance(_botConfig);
            builder.RegisterInstance(_backendConfig);
            builder.RegisterInstance(_matchmakingConfig);
            builder.RegisterInstance(_tournamentConfig);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<MatchmakingResultStore>(Lifetime.Singleton);
            builder.Register<NetworkSession>(Lifetime.Singleton);
            RegisterMeta(builder);
            builder.RegisterBuildCallback(_ => Application.targetFrameRate = _targetFrameRate);
        }

        private static void RegisterMeta(IContainerBuilder builder)
        {
            builder.Register<BackendClient>(Lifetime.Singleton);
            builder.RegisterInstance(new LocalFileStore(StorageFolder.For(Application.dataPath, Application.persistentDataPath, Application.isEditor)));
            builder.Register<AccountService>(Lifetime.Singleton);
            builder.RegisterInstance(new MatchmakingPool(PoolName()));
            builder.RegisterEntryPoint<SessionExit>().AsSelf();
            builder.Register<MatchReportApi>(Lifetime.Singleton);
        }

        private static string PoolName()
        {
            var session = DevSessionName.For(
                Application.dataPath,
                Application.isEditor,
                Environment.GetEnvironmentVariable(DevSessionName.OverrideVariable));
            return $"{session}-{Application.version}";
        }
    }
}
