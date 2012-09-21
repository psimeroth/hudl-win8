using Caliburn.Micro;
using System;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public MainViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        } 
    }
}
