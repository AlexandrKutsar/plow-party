using PlowParty.Infrastructure.Network;
using PlowParty.Meta.Account;
using PlowParty.Meta.Account.View;
using PlowParty.Meta.Lobby;
using PlowParty.Meta.Lobby.View;
using PlowParty.Meta.Session;
using PlowParty.Meta.Tournament;
using PlowParty.Meta.Tournament.View;
using VContainer;
using VContainer.Unity;

namespace PlowParty.Bootstrap
{
    public sealed class MenuScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<NetworkRunnerEvents>(Lifetime.Singleton);
            builder.Register<NetworkScopeBinding>(Lifetime.Singleton);
            builder.RegisterBuildCallback(resolver => resolver.Resolve<NetworkScopeBinding>().Bind());
            builder.RegisterComponentInHierarchy<NicknameView>();
            builder.RegisterEntryPoint<NicknamePresenter>();
            builder.RegisterEntryPoint<Matchmaker>().AsSelf();
            builder.RegisterComponentInHierarchy<PlayMenuView>();
            builder.RegisterComponentInHierarchy<LobbyPanelView>();
            builder.RegisterEntryPoint<LobbyPresenter>();
            builder.Register<TournamentService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TournamentView>();
            builder.RegisterEntryPoint<TournamentPresenter>();
        }
    }
}
