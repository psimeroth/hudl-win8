using Caliburn.Micro;
using System;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private BindableCollection<string> teams;
        public BindableCollection<string> Teams
        {
            get { return teams; }
            set
            {
                teams = value;
                NotifyOfPropertyChange(() => Teams);
            }
        } 
        public HubViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            //Teams = new BindableCollection<string>();
            //Teams.Add("Team one");
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        } 
    }
}
