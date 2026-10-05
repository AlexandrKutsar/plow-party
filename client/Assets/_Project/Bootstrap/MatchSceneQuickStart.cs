using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchSceneQuickStart : IAsyncStartable
    {
        private const string SessionName = "plow-party-dev";

        private readonly INetworkSession _session;

        public MatchSceneQuickStart(INetworkSession session)
        {
            _session = session;
        }

        public UniTask StartAsync(CancellationToken cancellation)
        {
            return _session.StartHostOrClientAsync(SessionName, cancellation);
        }
    }
}
