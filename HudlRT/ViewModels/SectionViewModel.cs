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
            Categories = new BindableCollection<CategoryViewModel>();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            GetGameCategories(CachedParameter.gameId);
        }

        public async Task GetGameCategories(string gameID)
        {
            Categories = null;
            CategoryResponse response = await ServiceAccessor.GetGameCategories(gameID);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                    cats.Add(CategoryViewModel.FromCategory(category));
                }
                Categories = cats;
            }
            /*else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }*/
            else
            {
                Categories = null;
            }
        }
        /*
        public async Task GetCutupsByCategory(CategoryViewModel category)
        {
            CutupResponse response = await ServiceAccessor.GetCategoryCutups(category.CategoryId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                Cutups = new BindableCollection<CutupViewModel>();
                foreach (Cutup cutup in response.cutups)
                {
                    Cutups.Add(CutupViewModel.FromCutup(cutup));
                    Task<ClipResponse> tempResponse = LoadCutup(CutupViewModel.FromCutup(cutup));
                    CachedCutupCalls.TryAdd(cutup.cutupId, tempResponse);
                }
                MarkDownloads();
                SetDownloadButtonVisibility();
            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }
            var currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.Snapped)
            {
                foreach (var cutup in Cutups)
                {
                    cutup.Name_Visibility = SNAPPED_VISIBILITY;
                    cutup.Thumbnail_Visibility = SNAPPED_VISIBILITY;
                    cutup.Width = new GridLength(0);
                    cutup.FontSize = SNAPPED_FONT_SIZE;
                }
            }
            if (Cutups == null || Cutups.Count == 0)
            {
                NoEntriesMessage_Visibility = Visibility.Visible;
            }
            else
            {
                NoEntriesMessage_Visibility = Visibility.Collapsed;
            }
        }
         * */
    }
}
