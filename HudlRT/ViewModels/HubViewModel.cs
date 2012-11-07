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

        private BindableCollection<Season> seasonsForDropDown;
        public BindableCollection<Season> SeasonsDropDown
        {
            get { return seasonsForDropDown; }
            set
            {
                seasonsForDropDown = value;
                NotifyOfPropertyChange(() => SeasonsDropDown);
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

        private Game mostRecentGameFootage;
        public Game MostRecentGameFootage
        {
            get { return mostRecentGameFootage; }
            set
            {
                mostRecentGameFootage = value;
                NotifyOfPropertyChange(() => MostRecentGameFootage);
            }
        }

        private Game otherRecentGameFootage;
        public Game OtherRecentGameFootage
        {
            get { return otherRecentGameFootage; }
            set
            {
                otherRecentGameFootage = value;
                NotifyOfPropertyChange(() => OtherRecentGameFootage);
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
                //GetTeams();
                PopulateDropDown();
            }

            ColVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        public async void PopulateDropDown()
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

                    Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                    long teamID = -1;
                    long seasonID = -1;
                    bool foundSavedSeason = false;
                    if (roamingSettings.Values["hudl-teamID"] != null && roamingSettings.Values["hudl-seasonID"] != null)
                    {
                        teamID = (long)roamingSettings.Values["hudl-teamID"];
                        seasonID = (long)roamingSettings.Values["hudl-seasonID"];
                    }
                    
                    SeasonsDropDown = new BindableCollection<Season>();
                    foreach (Team team in model.teams)
                    {
                        foreach (Season season in team.seasons)
                        {
                            //School - Team - Season
                            season.name = season.owningTeam.school + " - " + season.owningTeam.name + " - " + season.name;
                            if (teamID == season.owningTeam.teamID && seasonID == season.seasonID)
                            {
                                SelectedSeason = season;
                                foundSavedSeason = true;
                            }
                            SeasonsDropDown.Add(season);
                        }
                    }
                    if (!foundSavedSeason && SeasonsDropDown.Count > 0)
                    {
                        SelectedSeason = SeasonsDropDown[0];
                    }
                    //populate this/next game
                    NotifyOfPropertyChange(() => SelectedSeason);
                    GetGames(SelectedSeason);
                    
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

        public async void GetGames(Season s)//sets gameThisWeek and gameNextWeek
        {
            var games = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_SCHEDULE_BY_SEASON.Replace("#", s.owningTeam.teamID.ToString()).Replace("%", s.seasonID.ToString()));
            if (!string.IsNullOrEmpty(games))
            {
                try
                {
                    s.games = new BindableCollection<Game>();
                    List<Game> gamesToBeSorted = new List<Game>();
                    var obj = JsonConvert.DeserializeObject<List<GameDTO>>(games);
                    foreach (GameDTO gameDTO in obj)
                    {
                        s.games.Add(Game.FromDTO(gameDTO));
                        gamesToBeSorted.Add(Game.FromDTO(gameDTO));
                    }
                    gamesToBeSorted.Sort((x, y) => DateTime.Compare(y.date, x.date));//most recent to least recent
                    foreach (Game g in gamesToBeSorted)
                    {
                        var categories = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME.Replace("#", g.gameId.ToString()));
                        if (!string.IsNullOrEmpty(categories))
                        {
                                g.categories = new BindableCollection<Category>();
                                var cats = JsonConvert.DeserializeObject<List<CategoryDTO>>(categories);
                                foreach (CategoryDTO categoryDTO in cats)
                                {
                                    //check to see if category has a cutup
                                    Category newCategory = Category.FromDTO(categoryDTO);
                                    var cutups = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY.Replace("#", newCategory.categoryId.ToString()));
                                    newCategory.cutups = new BindableCollection<Cutup>();
                                    if (!string.IsNullOrEmpty(cutups))
                                    {    
                                            var cuts = JsonConvert.DeserializeObject<List<CutupDTO>>(cutups);
                                            foreach (CutupDTO cutupDTO in cuts)
                                            {
                                                newCategory.cutups.Add(Cutup.FromDTO(cutupDTO));
                                            }
                                    }
                                    if (newCategory.cutups.Count > 0)
                                    {
                                        g.categories.Add(newCategory);
                                    }
                                }
                        }
                    }
                    int count = 0;
                    foreach (Game g in gamesToBeSorted)
                    {
                        foreach (Category c in g.categories)
                        {
                            if (c.name == "Game Footage" && count == 1)
                            {
                                count++;
                                OtherRecentGameFootage = g;
                            }
                            if (c.name == "Game Footage" && count == 0)
                            {
                                count++;
                                MostRecentGameFootage = g;
                            }
                            

                        }
                    }
                    NotifyOfPropertyChange(() => MostRecentGameFootage);
                    NotifyOfPropertyChange(() => OtherRecentGameFootage);
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

        public void SeasonSelected(SelectionChangedEventArgs eventArgs)
        {
            if (eventArgs != null)
            {

                var selectedSeason = (Season)eventArgs.AddedItems[0];
                ComboBox x = (ComboBox)eventArgs.OriginalSource;
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["hudl-teamID"] = selectedSeason.owningTeam.teamID;
                roamingSettings.Values["hudl-seasonID"] = selectedSeason.seasonID;

                GetGames(selectedSeason);
            }
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

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}