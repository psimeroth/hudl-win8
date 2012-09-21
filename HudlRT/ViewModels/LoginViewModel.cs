using Caliburn.Micro;
using System;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public LoginViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

        }

        public void Login()
        {
            navigationService.NavigateToViewModel(typeof(MainViewModel));
        }
    }
}
