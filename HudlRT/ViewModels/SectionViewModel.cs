using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private bool firstLoad = true;
        public HubSectionParameter Parameter { get; set; }

        private BindableCollection<GameViewModel> _schedule { get; set; }
        public BindableCollection<GameViewModel> Schedule
        {
            get { return _schedule; }
            set
            {
                _schedule = value;
                NotifyOfPropertyChange(() => Schedule);
            }
        }

        
        private BindableCollection<CutupViewModel> _cutups { get; set; }
        public BindableCollection<CutupViewModel> Cutups
        {
            get { return _cutups; }
            set
            {
                _cutups = value;
                NotifyOfPropertyChange(() => Cutups);
            }
        }

        // Maps to the selected game in the game list
        private GameViewModel selectedGame;
        public GameViewModel SelectedGame
        {
            get { return selectedGame; }
            set
            {
                selectedGame = value;
                NotifyOfPropertyChange(() => SelectedGame);
            }
        }

        public SectionViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            // Get the team and season ID
            long teamID;
            long seasonID;
            try
            {
                teamID = (long)ApplicationData.Current.RoamingSettings.Values["hudl-teamID"];
                seasonID = (long)ApplicationData.Current.RoamingSettings.Values["hudl-seasonID"];
            }
            catch (Exception ex)
            {
                teamID = 0;
                seasonID = 0;
            }

            // Load data for the drop down
            PopulateDropDown();
        }

        private async void LoadPageFromParamter(long seasonID, long teamID, long gameID, long categoryID)
        {
            Cutups = null;
            await GetGames(teamID, seasonID);

            // Make sure there are game entries for the season.
            if (Schedule.Count == 0)
            {
                Schedule = null;
            }
            else
            {
                // Find the passed in game
                SelectedGame = null;
                foreach (GameViewModel game in Schedule)
                {
                    if (game.GameId == gameID)
                    {
                        SelectedGame = game;
                        break;
                    }
                }

                // If the game isn't found set the first one as the default
                if (SelectedGame == null)
                {
                    SelectedGame = Schedule.FirstOrDefault();
                }

                await GetGameCategories(SelectedGame);

                // Make sure there are categories for the selected game
                if (SelectedGame.Categories.Count == 0)
                {
                    SelectedGame.Categories = null;
                }
                else
                {
                    // Find the selected category
                    SelectedGame.SelectedCategory = null;
                    foreach (CategoryViewModel cat in SelectedGame.Categories)
                    {
                        if (cat.CategoryId == categoryID)
                        {
                            SelectedGame.SelectedCategory = cat;
                        }
                    }

                    // If the category isn't found set the first as the default
                    if (SelectedGame.SelectedCategory == null)
                    {
                        SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();
                    }
                    GetCutupsByCategory(SelectedGame.SelectedCategory);
                }
            }
        }

        private async void LoadPageFromDefault(long seasonID, long teamID)
        {
            Cutups = null;
            await GetGames(teamID, seasonID);
            if (Schedule.Count == 0)
            {
                Schedule = null;
            }
            else
            {
                SelectedGame = Schedule.FirstOrDefault();
                await GetGameCategories(SelectedGame);
                if (SelectedGame.Categories.Count == 0)
                {
                    SelectedGame.Categories = null;
                }
                else
                {
                    SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();
                    GetCutupsByCategory(SelectedGame.SelectedCategory);
                }
            }
        }

        public async Task GetGames(long teamID, long seasonID)
        {
            GameResponse response = await ServiceAccessor.GetGames(teamID.ToString(), seasonID.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var schedule = new BindableCollection<GameViewModel>();
                foreach (Game game in response.games)
                {
                    schedule.Add(GameViewModel.FromGame(game));
                }
                Schedule = schedule;
            }
            /*else if (games.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
            }*/
            else
            {
                Schedule = null;
            }
        }

        public async Task GetGameCategories(GameViewModel game)
        {
            CategoryResponse response = await ServiceAccessor.GetGameCategories(game.GameId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                    cats.Add(CategoryViewModel.FromCategory(category));
                }
                SelectedGame.Categories = cats;
            }
            /*else if (response.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
            }*/
            else
            {
                SelectedGame.Categories = null;
            }
        }

        public async Task GetCutupsByCategory(CategoryViewModel category)
        {
            CutupResponse response = await ServiceAccessor.GetCategoryCutups(category.CategoryId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cuts = new BindableCollection<CutupViewModel>();
                foreach (Cutup cutup in response.cutups)
                {
                    cuts.Add(CutupViewModel.FromCutup(cutup));
                }
                Cutups = cuts;
            }
            /*else if (response.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
            }*/
            else
            {
                Cutups = null;
            }
        }

        public async void GameSelected(ItemClickEventArgs eventArgs)
        {
            var game = (GameViewModel)eventArgs.ClickedItem;

            SelectedGame.Categories = null;
            SelectedGame = game;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = game;
            Cutups = null;

            await GetGameCategories(game);

            if (SelectedGame.Categories.Count == 0)
            {
                SelectedGame.Categories = null;
            }
            else
            {
                SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();
                GetCutupsByCategory(SelectedGame.SelectedCategory);
            }
        }

        public void CategorySelected(ItemClickEventArgs eventArgs)
        {
            var category = (CategoryViewModel)eventArgs.ClickedItem;

            SelectedGame.SelectedCategory = category;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = category;

            GetCutupsByCategory(category);
        }


        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            cutup.ClipLoading = true;
            cutup.Opacity = 0.5;
            await GetClipsByCutup(cutup);
            cutup.ClipLoading = true;
        }

        public async Task GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                string[] clipCount = cutup.ClipCount.ToString().Split(' ');
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PagePassParameter
                {
                    selectedCutup = new Cutup {cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = Int32.Parse(clipCount[0]), name = cutup.Name }
                });
            }
            else
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
            }
        }
        

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }


        public void GoBack()
        {
            navigationService.NavigateToViewModel<HubViewModel>(Parameter);
        }

        // Used for the season/team dropdown
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

        public void SeasonSelected(SelectionChangedEventArgs eventArgs)
        {
            if (eventArgs != null && !firstLoad)
            {
                firstLoad = false;
                var selectedSeason = (Season)eventArgs.AddedItems[0];
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["hudl-teamID"] = selectedSeason.owningTeam.teamID;
                roamingSettings.Values["hudl-seasonID"] = selectedSeason.seasonID;

                if (Parameter != null)
                {
                    LoadPageFromParamter(selectedSeason.seasonID, selectedSeason.owningTeam.teamID, Parameter.gameId, Parameter.categoryId);
                }
                else
                {
                    LoadPageFromDefault(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
                }
            }
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
                if (Parameter != null)
                {
                    LoadPageFromParamter(selectedSeason.seasonID, selectedSeason.owningTeam.teamID, Parameter.gameId, Parameter.categoryId);
                }
                else
                {
                    LoadPageFromDefault(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
                }
                NotifyOfPropertyChange(() => SelectedSeason);
                if (!foundSavedSeason && SeasonsDropDown.Count > 0)
                {
                    SelectedSeason = SeasonsDropDown[0];
                    if (Parameter != null)
                    {
                        LoadPageFromParamter(selectedSeason.seasonID, selectedSeason.owningTeam.teamID, Parameter.gameId, Parameter.categoryId);
                    }
                    else
                    {
                        LoadPageFromDefault(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
                    }
                    NotifyOfPropertyChange(() => SelectedSeason);
                }
            }
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
                Teams = null;
            }
        }
    }
}
