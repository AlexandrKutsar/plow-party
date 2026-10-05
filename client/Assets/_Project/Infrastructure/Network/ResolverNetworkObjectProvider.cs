using Fusion;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Infrastructure.Network
{
    public sealed class ResolverNetworkObjectProvider : NetworkObjectProviderDefault
    {
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            return _resolver.Instantiate(prefab);
        }
    }
}
