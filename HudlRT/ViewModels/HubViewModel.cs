using Caliburn.Micro;
using HudlRT.Models;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.Storage;
using HudlRT.Models;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private string feedback;
        public string Feedback
        {
            get { return feedback; }
            set
            {
                feedback = value;
                NotifyOfPropertyChange(() => Feedback);
            }
        }

        private Model model;

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

        public async void GetTeams()
        {
            // Get the username and password from the view
            var teams = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_TEAMS);

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!teams.Equals(""))
            {
                var obj = JsonConvert.DeserializeObject<BindableCollection<TeamDTO>>(teams);
                model.setTeams(obj);
                Teams = model.teams;
            }
            else
            {
                Feedback = "Error processing request.";
            }
        }


        public HubViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            model = new Model();
            GetTeams();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public void TeamSelected(ItemClickEventArgs eventArgs)
        {
            var team = (Team)eventArgs.ClickedItem;

        }
    }
}
