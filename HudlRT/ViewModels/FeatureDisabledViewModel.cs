using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class FeatureDisabledViewModel: ViewModelBase
    {
        private string betaNotification;
        public string BetaNotification 
        {
            get { return betaNotification; }
            set
            {
                betaNotification = value;
                NotifyOfPropertyChange(() => BetaNotification);
            }
        }

        private readonly INavigationService navigationService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            BetaNotification = "This application is currently in beta testing. In order to join our beta and use this application before release, please contact kyle.deterding@hudl.com";
        }
        
        public FeatureDisabledViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }
    }
}
