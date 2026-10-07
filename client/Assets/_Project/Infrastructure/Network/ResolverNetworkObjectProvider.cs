using Fusion;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Infrastructure.Network
{
    public sealed class ResolverNetworkObjectProvider : NetworkObjectProviderDefault
    {
        private IObjectResolver _resolver;

        public void Use(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            return _resolver != null ? _resolver.Instantiate(prefab) : base.InstantiatePrefab(runner, prefab);
        }
    }
}
