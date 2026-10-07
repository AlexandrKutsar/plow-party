using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Scenes;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class BootSceneFlow : IAsyncStartable
    {
        private const string FirstScene = "Match";

        private readonly SceneLoader _scenes;

        public BootSceneFlow(SceneLoader scenes)
        {
            _scenes = scenes;
        }

        public UniTask StartAsync(CancellationToken cancellation)
        {
            return _scenes.LoadAsync(FirstScene, cancellation);
        }
    }
}
