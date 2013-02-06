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
            else
            {
                Categories = null;
            }
        }
    }
}
