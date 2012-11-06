using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;

namespace HudlRT.ViewModels
{
    public class FeatureDisabledViewModel: ViewModelBase
    {
        private readonly INavigationService navigationService;
        
        public FeatureDisabledViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        public void GoBack()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}
