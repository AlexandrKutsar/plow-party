using System;
using VContainer;

namespace PlowParty.Infrastructure.Network
{
    public sealed class NetworkScopeBinding : IDisposable
    {
        private readonly NetworkSession _session;
        private readonly NetworkRunnerEvents _events;
        private readonly IObjectResolver _resolver;

        public NetworkScopeBinding(NetworkSession session, NetworkRunnerEvents events, IObjectResolver resolver)
        {
            _session = session;
            _events = events;
            _resolver = resolver;
        }

        public void Bind()
        {
            _session.Bind(_resolver, _events);
        }

        public void Dispose()
        {
            _session.Unbind(_events);
        }
    }
}
