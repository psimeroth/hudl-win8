using Caliburn.Micro;
using HudlRT.Models;
using System;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private BindableCollection<Team> teams;
        public BindableCollection<Team> Teams
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
            Teams = new BindableCollection<Team>();
            Teams.Add(new Team()
            {
                Name="Team One"
            });
            Teams.Add(new Team()
            {
                Name = "Team Two"
            });
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public void TeamSelected(ItemClickEventArgs eventArgs)
        {
            var team = (Team)eventArgs.ClickedItem;

            //if (sample.ViewModelType == null)
                //return;

            //_navigationService.NavigateToViewModel(sample.ViewModelType);
        }
    }
}
