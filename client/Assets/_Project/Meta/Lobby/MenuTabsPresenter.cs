using System;
using PlowParty.Meta.Lobby.Simulation;
using PlowParty.Meta.Lobby.View;
using VContainer.Unity;

namespace PlowParty.Meta.Lobby
{
    public sealed class MenuTabsPresenter : IStartable, IDisposable
    {
        private readonly MenuTabsView _view;

        public MenuTabsPresenter(MenuTabsView view)
        {
            _view = view;
        }

        public void Start()
        {
            _view.TabRequested += _view.Show;
            _view.Show(MenuTab.Play);
        }

        public void Dispose()
        {
            _view.TabRequested -= _view.Show;
        }
    }
}
