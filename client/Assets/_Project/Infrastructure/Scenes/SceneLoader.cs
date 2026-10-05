using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace PlowParty.Infrastructure.Scenes
{
    public sealed class SceneLoader
    {
        public UniTask LoadAsync(string sceneName, CancellationToken cancellationToken)
        {
            return SceneManager.LoadSceneAsync(sceneName).ToUniTask(cancellationToken: cancellationToken);
        }
    }
}
