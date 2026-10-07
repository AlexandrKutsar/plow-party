using VContainer;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class BootScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<BootSceneFlow>();
        }
    }
}
