using System.Threading;
using Cysharp.Threading.Tasks;

namespace PlowParty.Infrastructure.Scenes
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(string sceneName, CancellationToken cancellationToken);
    }
}
