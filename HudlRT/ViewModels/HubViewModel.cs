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
            var teams = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_TEAMS);
            if (!string.IsNullOrEmpty(teams))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<TeamDTO>>(teams);
                    model.teams = new BindableCollection<Team>();
                    foreach (TeamDTO teamDTO in obj)
                    {
                        model.teams.Add(Team.FromDTO(teamDTO));
                    }
                }
                catch (Exception)
                {
                    //show error message
                    Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
            if (!string.IsNullOrEmpty(games))
            {
                try
                {
                    s.games = new BindableCollection<Game>();
                    var obj = JsonConvert.DeserializeObject<List<GameDTO>>(games);
                    foreach (GameDTO gameDTO in obj)
                    {
                        s.games.Add(Game.FromDTO(gameDTO));
                    }
                }
                catch (Exception)
                {
                    //show error message
                    Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
            if (!string.IsNullOrEmpty(categories))
            {
                try
                {
                    game.categories = new BindableCollection<Category>();
                    var obj = JsonConvert.DeserializeObject<List<CategoryDTO>>(categories);
                    foreach (CategoryDTO categoryDTO in obj)
                    {
                        game.categories.Add(Category.FromDTO(categoryDTO));
                    }
                }
                catch (Exception)
                {
                    //show error message
                    Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                }
                Categories = game.categories;
            }
            else
            {
                Feedback = "Error processing GetGameCategories request.";
                Categories = null;
            }
        }

        public async void GetCutupsByCategory(Category category)
        {
            var cutups = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY.Replace("#", category.categoryId.ToString()));
            if (!string.IsNullOrEmpty(cutups))
            {
                try
                {
                    category.cutups = new BindableCollection<Cutup>();
                    var obj = JsonConvert.DeserializeObject<List<CutupDTO>>(cutups);
                    foreach (CutupDTO cutupDTO in obj)
                    {
                        category.cutups.Add(Cutup.FromDTO(cutupDTO));
                    }
                }
                catch (Exception)
                {
                    //show error message
                    Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                }
                Cutups = category.cutups;
            }
            else
            {
                Feedback = "Error processing GetCutupsByCategory request.";
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
            var clips = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CLIPS.Replace("#", cutup.cutupId.ToString()));
            if (!string.IsNullOrEmpty(clips))
            {
                try
                {
                    cutup.clips = new BindableCollection<Clip>();
                    var obj = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
                    cutup.displayColumns = obj.DisplayColumns;
                    foreach (ClipDTO clipDTO in obj.ClipsList.Clips)
                    {
                        Clip c = Clip.FromDTO(clipDTO, cutup.displayColumns);
                        if (c != null)
                        {
                            cutup.clips.Add(c);
                        }
                    }
                    ProgressRingVisibility = "Collapsed";
                    ColVisibility = "Visible";
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
                catch (Exception)
                {
                    ProgressRingVisibility = "Collapsed";
                    ColVisibility = "Visible";
                    Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                }
            }
        }

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}
