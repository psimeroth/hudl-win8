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

        private Game nextGame;
        public Game NextGame
        {
            get { return nextGame; }
            set
            {
                nextGame = value;
                NotifyOfPropertyChange(() => NextGame);
            }
        }

        private Game previousGame;
        public Game PreviousGame
        {
            get { return previousGame; }
            set
            {
                previousGame = value;
                NotifyOfPropertyChange(() => PreviousGame);
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

        private BindableCollection<Category> nextGameCategories;
        public BindableCollection<Category> NextGameCategories
        {
            get { return nextGameCategories; }
            set
            {
                nextGameCategories = value;
                NotifyOfPropertyChange(() => NextGameCategories);
            }
        }

        private BindableCollection<Category> previousGameCategories;
        public BindableCollection<Category> PreviousGameCategories
        {
            get { return previousGameCategories; }
            set
            {
                previousGameCategories = value;
                NotifyOfPropertyChange(() => PreviousGameCategories);
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
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                Teams = response.teams;
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
                foreach (Team team in Teams)
                {
                    foreach (Season season in team.seasons)
                    {
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
                FindNextGame(SelectedSeason);
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Teams = null;
            }
        }

        public async void FindNextGame(Season s)//sets gameThisWeek and gameNextWeek
        {
            GameResponse response = await ServiceAccessor.GetGames(s.owningTeam.teamID.ToString(), s.seasonID.ToString());
            
            NextGame = null;
            PreviousGame = null;
            NextGameCategories = null;
            PreviousGameCategories = null;
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                if (response.games.Count > 0)
                {
                    List<Game> sortedGames = new List<Game>();
                    foreach (Game game in response.games)
                    {
                        sortedGames.Add(game);
                    }
                    sortedGames.Sort((x, y) => DateTime.Compare(y.date, x.date));//most recent to least recent
                    DateTime lastGameDate = sortedGames[0].date;

                    if (DateTime.Compare(DateTime.Now, lastGameDate) >= 0)
                    {
                        NextGame = sortedGames[0];
                        if (sortedGames.Count >= 2)
                        {
                            PreviousGame = sortedGames[1];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < sortedGames.Count; i++)
                        {
                            if (DateTime.Compare(sortedGames[i].date, DateTime.Now) < 0)
                            {
                                if (i == 0)
                                {
                                    NextGame = sortedGames[i];
                                }
                                else
                                {
                                    NextGame = sortedGames[i - 1];
                                    PreviousGame = sortedGames[i];
                                }
                                break;
                            }
                        }
                    }
                }
                if (NextGame != null)
                {
                    CategoryResponse catResponse = await ServiceAccessor.GetGameCategories(NextGame.gameId.ToString());
                    if (catResponse.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        NextGameCategories = catResponse.categories;
                        NextGame.categories = NextGameCategories;
                    }
                    else//could better handle exceptions
                    {
                        Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                        NextGameCategories = null;
                    }
                }
                if (PreviousGame != null)
                {
                    CategoryResponse catResponse = await ServiceAccessor.GetGameCategories(NextGame.gameId.ToString());
                    if (catResponse.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        PreviousGameCategories = catResponse.categories;
                        PreviousGame.categories = PreviousGameCategories;
                    }
                    else//could better handle exceptions
                    {
                        Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                        PreviousGameCategories = null;
                    }
                }
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                NextGame = null;
            }
        }

        public void SeasonSelected(SelectionChangedEventArgs eventArgs)
        {
            if (eventArgs != null)
            {

                var selectedSeason = (Season)eventArgs.AddedItems[0];
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["hudl-teamID"] = selectedSeason.owningTeam.teamID;
                roamingSettings.Values["hudl-seasonID"] = selectedSeason.seasonID;

                FindNextGame(selectedSeason);
            }
        }

        public void NextCategorySelected(ItemClickEventArgs eventArgs)
        {
            
            var category = (Category)eventArgs.ClickedItem;
            HubSectionParameter param = new HubSectionParameter { categoryId = category.categoryId, gameId = NextGame.gameId};
            navigationService.NavigateToViewModel<SectionViewModel>(param);

        }

        public void PreviousCategorySelected(ItemClickEventArgs eventArgs)
        {

            var category = (Category)eventArgs.ClickedItem;
            HubSectionParameter param = new HubSectionParameter { categoryId = category.categoryId, gameId = PreviousGame.gameId };
            navigationService.NavigateToViewModel<SectionViewModel>(param);

        }

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}