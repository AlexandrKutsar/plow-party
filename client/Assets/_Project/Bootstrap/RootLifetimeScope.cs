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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_vehicleConfig);
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}
