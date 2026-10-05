using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlowParty.Infrastructure.Network
{
    public interface INetworkSession
    {
        UniTask StartHostOrClientAsync(string sessionName, CancellationToken cancellationToken);
    }
}
