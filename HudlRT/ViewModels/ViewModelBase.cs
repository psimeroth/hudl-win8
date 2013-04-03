using Caliburn.Micro;

namespace HudlRT.ViewModels
{
    public class ViewModelBase : Screen
    {
        private readonly INavigationService navigationService;

        public ViewModelBase(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public bool CanGoBack
        {
            get
            {
                return navigationService.CanGoBack;
            }
        }
    }
}
