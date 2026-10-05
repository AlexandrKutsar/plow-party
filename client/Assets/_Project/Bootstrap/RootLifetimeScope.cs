using PlowParty.Infrastructure.Scenes;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
        }
    }
}
