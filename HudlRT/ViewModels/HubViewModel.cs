using Caliburn.Micro;
using HudlRT.Models;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.Storage;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private Model model;
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

        private BindableCollection<Season> seasons;
        public BindableCollection<Season> Seasons
        {
            get { return seasons; }
            set
            {
                seasons = value;
                NotifyOfPropertyChange(() => Seasons);
            }
        }

        private BindableCollection<Game> games;
        public BindableCollection<Game> Games
        {
            get { return games; }
            set
            {
                games = value;
                NotifyOfPropertyChange(() => Games);
            }
        }

        private BindableCollection<Category> categories;
        public BindableCollection<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        public HubViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            model = new Model();
            GetTeams();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public async void GetTeams()
        {
            // Get the username and password from the view
            var teams = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_TEAMS);

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!teams.Equals(""))
            {
                var obj = JsonConvert.DeserializeObject<BindableCollection<TeamDTO>>(teams);
                model.teams = new BindableCollection<Team>();
                foreach (TeamDTO teamDTO in obj)
                {
                    model.teams.Add(Team.FromDTO(teamDTO));
                }
                Teams = model.teams;
            }
            else
            {
                Feedback = "Error processing GetTeams request.";
                Teams = null;
            }
        }

        public async void GetGames(Season s)
        {
            var games = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_SCHEDULE_BY_SEASON.Replace("#", s.owningTeam.teamID.ToString()).Replace("%", s.seasonID.ToString()));

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!games.Equals(""))
            {
                s.games = new BindableCollection<Game>();
                var obj = JsonConvert.DeserializeObject<BindableCollection<GameDTO>>(games);
                foreach (GameDTO gameDTO in obj)
                {
                    s.games.Add(Game.FromDTO(gameDTO));
                }
                Games = s.games;
            }
            else
            {
                Feedback = "Error processing GetGames request.";
                Games = null;
            }
        }

        public async void GetGameCategories(Game game)
        {
            var categories = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME.Replace("#", game.gameId.ToString()));

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!categories.Equals(""))
            {
                game.categories = new BindableCollection<Category>();
                var obj = JsonConvert.DeserializeObject<BindableCollection<CategoryDTO>>(categories);
                foreach (CategoryDTO categoryDTO in obj)
                {
                    game.categories.Add(Category.FromDTO(categoryDTO));
                }
                Categories = game.categories;
            }
            else
            {
                Feedback = "Error processing GetGameCategories request.";
                Categories = null;
            }
        }

        public void TeamSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var team = (Team)eventArgs.ClickedItem;
            Seasons = team.seasons;
            Games = null;
            Categories = null;
        }

        public void SeasonSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var season = (Season)eventArgs.ClickedItem;
            GetGames(season);
            Categories = null;            
        }

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var game = (Game)eventArgs.ClickedItem;
            GetGameCategories(game);
        }
    }
}
