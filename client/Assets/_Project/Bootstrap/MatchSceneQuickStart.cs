using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Network;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MatchSceneQuickStart : IAsyncStartable
    {
        private const string SessionName = "plow-party-dev";

        private readonly NetworkSession _session;

        public MatchSceneQuickStart(NetworkSession session)
        {
            _session = session;
        }

        public UniTask StartAsync(CancellationToken cancellation)
        {
            return _session.StartHostOrClientAsync(SessionName, cancellation);
        }
    }
}
