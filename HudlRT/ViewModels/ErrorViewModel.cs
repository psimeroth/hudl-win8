using Caliburn.Micro;
using HudlRT.Models;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using HudlRT.Parameters;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        INavigationService navigationService;

        protected override async void OnInitialize()
        {
        }

        protected override void OnActivate()
        {
        }

        public void GoBack()
        {
            navigationService.GoBack();
        }

        public ErrorViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
        }
    }
}
