using System.Threading;
using Cysharp.Threading.Tasks;
using PlowParty.Infrastructure.Scenes;
using PlowParty.Meta.Account;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class BootSceneFlow : IAsyncStartable
    {
        private readonly SceneLoader _scenes;
        private readonly AccountService _account;

        public BootSceneFlow(SceneLoader scenes, AccountService account)
        {
            _scenes = scenes;
            _account = account;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _account.EnsureSignedInAsync(cancellation);
            await _scenes.LoadAsync(SceneNames.Menu, cancellation).SuppressCancellationThrow();
        }
    }
}
