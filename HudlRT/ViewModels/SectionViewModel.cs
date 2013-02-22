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
        public string Parameter { get; set; }       //Passed in from hub page - contains the game Id.
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
            if (Parameter != _gameId)
            {
                _gameId = Parameter;
                GetGameCategories(_gameId);

            }
            base.OnActivate();
        }

        public async Task GetGameCategories(string gameID)
        {
            //if(CachedParameter.gameId == )
            Categories = null;
            CategoryResponse response = await ServiceAccessor.GetGameCategories(gameID);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                BindableCollection<CategoryViewModel> cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                        CategoryViewModel cat = new CategoryViewModel(category);
                        await AddPlaylistsForCategory(cat);
                        if (cat.Playlists != null && cat.Playlists.Count() != 0)
                        {
                            cats.Add(cat);
                        }
                }
                Categories = cats;
            }
            else
            {
                Categories = null;
            }
        }

        public async Task AddPlaylistsForCategory(CategoryViewModel category)
        {
            PlaylistResponse response = await ServiceAccessor.GetCategoryPlaylists(category.CategoryModel.categoryId);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                category.Playlists = new BindableCollection<PlaylistViewModel>();
                foreach (Playlist playlist in response.playlists)
                {
                    category.Playlists.Add(new PlaylistViewModel(playlist));
                    AddClipsAndHeadersForPlaylist(playlist);
                }
            }
            else
            {
                //What should go here?
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
