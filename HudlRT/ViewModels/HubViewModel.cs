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

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private Model model;
        private readonly INavigationService navigationService;
        public PagePassParameter Parameter { get; set; }
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

        private BindableCollection<Cutup> cutups;
        public BindableCollection<Cutup> Cutups
        {
            get { return cutups; }
            set
            {
                cutups = value;
                NotifyOfPropertyChange(() => Cutups);
            }
        }
        private Team selectedTeam;
        public Team SelectedTeam
        {
            get { return selectedTeam; }
            set
            {
                selectedTeam = value;
                NotifyOfPropertyChange(() => SelectedTeam);
            }
        }
        private Season selectedSeason;
        public Season SelectedSeason
        {
            get { return selectedSeason; }
            set
            {
                selectedSeason = value;
                NotifyOfPropertyChange(() => SelectedSeason);
            }
        }
        private Game selectedGame;
        public Game SelectedGame
        {
            get { return selectedGame; }
            set
            {
                selectedGame = value;
                NotifyOfPropertyChange(() => SelectedGame);
            }
        }
        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                NotifyOfPropertyChange(() => SelectedCategory);
            }
        }

        private BindableCollection<Clip> Clips = new BindableCollection<Clip>();


        // Bound to the visibility of the progress ring. Swaps with 
        private string progressRingVisibility;
        public string ProgressRingVisibility
        {
            get { return progressRingVisibility; }
            set
            {
                progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        // Bound to the visibility of the login form stack panel
        private string colVisibility;
        public string ColVisibility
        {
            get { return colVisibility; }
            set
            {
                colVisibility = value;
                NotifyOfPropertyChange(() => ColVisibility);
            }
        }

        public HubViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Parameter != null)
            {
                Teams = Parameter.teams;
                Games = Parameter.games;
                Seasons = Parameter.seasons;
                Categories = Parameter.categories;
                Cutups = Parameter.cutups;
                SelectedTeam = Parameter.selectedTeam;
                SelectedSeason = Parameter.selectedSeason;
                SelectedGame = Parameter.selectedGame;
                SelectedCategory = Parameter.selectedCategory;
            }
            else
            {
                model = new Model();
                GetTeams();
            }

            ColVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        public async void GetTeams()
        {
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.success)
            {
                Teams = response.teams;
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Teams = null;
            }
        }

        public async void GetGames(Season s)
        {
            GameResponse response = await ServiceAccessor.GetGames(s.owningTeam.teamID.ToString(), s.seasonID.ToString());
            if (response.success)
            {
                Games = response.games;
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Games = null;
            }
        }

        public async void GetGameCategories(Game game)
        {
            CategoryResponse response = await ServiceAccessor.GetGameCategories(game.gameId.ToString());
            if (response.success)
            {
                Categories = response.categories;
                game.categories = Categories;
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Categories = null;
            }
        }

        public async void GetCutupsByCategory(Category category)
        {
            CutupResponse response = await ServiceAccessor.GetCategoryCutups(category.categoryId.ToString());
            if (response.success)
            {
                Cutups = response.cutups;
                category.cutups = Cutups;
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Cutups = null;
            }
        }

        public void TeamSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var team = (Team)eventArgs.ClickedItem;
            
            SelectedTeam = team;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = team;

            Seasons = team.seasons;
            SelectedSeason = null;
            Games = null;
            Categories = null;
            Cutups = null;
        }

        public void SeasonSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var season = (Season)eventArgs.ClickedItem;

            SelectedSeason = season;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = season;

            GetGames(season);
            Categories = null;
            Cutups = null;
        }

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var game = (Game)eventArgs.ClickedItem;

            SelectedGame = game;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = game;

            GetGameCategories(game);
            Cutups = null;
        }

        public void CategorySelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var category = (Category)eventArgs.ClickedItem;

            SelectedCategory = category;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = category;

            GetCutupsByCategory(category);
        }

        public void CutupSelected(ItemClickEventArgs eventArgs)
        {
            Feedback = null;
            var cutup = (Cutup)eventArgs.ClickedItem;
            GetClipsByCutup(cutup);
        }

        public async void GetClipsByCutup(Cutup cutup)
        {
            ColVisibility = "Collapsed";
            ProgressRingVisibility = "Visible";
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.success)
            {
                ProgressRingVisibility = "Collapsed";
                ColVisibility = "Visible";
                cutup.clips = response.clips;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PagePassParameter
                {
                    teams = teams,
                    games = games,
                    categories = categories,
                    seasons = seasons,
                    cutups = cutups,
                    selectedTeam = SelectedTeam,
                    selectedSeason = SelectedSeason,
                    selectedGame = SelectedGame,
                    selectedCategory = SelectedCategory,
                    selectedCutup = cutup
                });
            }
            else//could better handle exceptions
            {
                ProgressRingVisibility = "Collapsed";
                ColVisibility = "Visible";
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
            }
        }

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}
