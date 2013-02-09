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

        private BindableCollection<Season> seasonsForDropDown;
        public BindableCollection<Season> SeasonsDropDown
        {
            get { return seasonsForDropDown; }
            set
            {
                seasonsForDropDown = value;
                NotifyOfPropertyChange(() => SeasonsDropDown);
            }
        }

        private Season selectedSeason;
        public Season SelectedSeason
        {
            get { return selectedSeason; }
            set
            {
                selectedSeason = value;
                NotifyOfPropertyChange(() => SelectedSeason);
            }
        }

        public SectionViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            Categories = new BindableCollection<CategoryViewModel>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            SeasonsDropDown = CachedParameter.seasonsDropDown;
            SelectedSeason = CachedParameter.seasonSelected;

            GetGameCategories(CachedParameter.gameId);
        }

        public async Task GetGameCategories(string gameID)
        {
            Categories = null;
            CategoryResponse response = await ServiceAccessor.GetGameCategories(gameID);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                BindableCollection<CategoryViewModel> cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                    CategoryViewModel cat = CategoryViewModel.FromCategory(category);
                    cats.Add(cat);
                    await AddPlaylistsForCategory(cat);
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
            PlaylistResponse response = await ServiceAccessor.GetCategoryCutups(category.CategoryId);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                category.Playlists = new BindableCollection<PlaylistViewModel>();
                foreach (Playlist playlist in response.playlists)
                {
                    category.Playlists.Add(PlaylistViewModel.FromPlaylist(playlist));
                }
            }
            else
            {
                //What should go here?
            }
        }

        public void PlaylistSelected(ItemClickEventArgs eventArgs)
        {
            //CachedParameter.selectedCutup = ((PlaylistViewModel)eventArgs.ClickedItem).PlaylistId;
            //navigationService.NavigateToViewModel<VideoPlayerViewModel>();

        }
    }
}
