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
        protected override void OnActivate()
        {
            base.OnActivate();

        }

        public BindableCollection<CategoryViewModel> Categories
        {
            get;
            private set;
        }

        public SectionViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Categories = new BindableCollection<CategoryViewModel>();
        }
    }
}
