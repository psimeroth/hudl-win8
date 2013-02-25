using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        INavigationService navigationService;
        public Game Parameter { get; set; }       //Passed in from hub page - contains the game selected.
        private string _gameId;     //Used to tell if the page needs to be reloaded
        private BindableCollection<CategoryViewModel> _categories;
        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        private string _noPlaylistText;
        public string NoPlaylistText
        {
            get { return _noPlaylistText; }
            set
            {
                _noPlaylistText = value;
                NotifyOfPropertyChange(() => NoPlaylistText);
            }
        }

        public SectionViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            Categories = new BindableCollection<CategoryViewModel>();
        }

        protected override void OnActivate()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
            //To insure the data shown is fetched if coming from the hub page to a new game
            //But that it doesn't fetch the data again if coming back from the video page.
            if (Parameter.gameId != _gameId)
            {
                _gameId = Parameter.gameId;
                GetGameCategories(_gameId);

            }
            base.OnActivate();
        }

        public async Task GetGameCategories(string gameID)
        {
            Categories = null;
            BindableCollection<CategoryViewModel> cats = new BindableCollection<CategoryViewModel>();
            foreach (Category c in Parameter.categories)
            {
                CategoryViewModel cat = new CategoryViewModel(c);
                foreach (Playlist p in c.playlists)
                {
                    PlaylistViewModel pvm = new PlaylistViewModel(p);
                    cat.Playlists.Add(pvm);
                    AddClipsAndHeadersForPlaylist(p);
                }
                if (c.playlists != null && c.playlists.Count() != 0)
                {
                    cats.Add(cat);
                }
            }
            Categories = cats;

            if (Categories.Count == 0)
            {
                NoPlaylistText = "There are no playlists for this schedule entry";
            }
            else
            {
                NoPlaylistText = "";
            }
        }

        public async Task AddClipsAndHeadersForPlaylist(Playlist playlist)
        {
            playlist.clips = new BindableCollection<Clip>();
            ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(playlist.playlistId);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                playlist.clips = response.clips;
                playlist.displayColumns = response.DisplayColumns;
            }
            else
            {
            }
        }

        public void PlaylistSelected(ItemClickEventArgs eventArgs)
        {
            navigationService.NavigateToViewModel<VideoPlayerViewModel>(((PlaylistViewModel)eventArgs.ClickedItem).PlaylistModel);

        }
    }
}
