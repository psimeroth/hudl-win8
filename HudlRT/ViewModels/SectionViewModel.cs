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

        // Maps to the selected item in the cutup list
        private CutupViewModel _selectedCutup;
        public CutupViewModel SelectedCutup
        {
            get { return _selectedCutup; }
            set
            {
                _selectedCutup = value;
                NotifyOfPropertyChange(() => SelectedCutup);
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

            // Check if a specific cutup was passed to the page
            if (Parameter != null)
            {
                LoadPageFromParamter(seasonID, teamID, Parameter.gameId, Parameter.categoryId);
            } else {
                LoadPageFromDefault(seasonID, teamID);
            }
        }

        private async void LoadPageFromParamter(long seasonID, long teamID, long gameID, long categoryID)
        {
            await GetGames(teamID, seasonID);

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

        private async void LoadPageFromDefault(long seasonID, long teamID)
        {
            await GetGames(teamID, seasonID);
            SelectedGame = Schedule.FirstOrDefault();
            await GetGameCategories(SelectedGame);
            SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();
            GetCutupsByCategory(SelectedGame.SelectedCategory);
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
            SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();
            GetCutupsByCategory(SelectedGame.SelectedCategory);
        }

        public void CategorySelected(ItemClickEventArgs eventArgs)
        {
            var category = (CategoryViewModel)eventArgs.ClickedItem;

            SelectedGame.SelectedCategory = category;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = category;

            GetCutupsByCategory(category);
        }


        public void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            GetClipsByCutup(cutup);
        }

        public async void GetClipsByCutup(CutupViewModel cutup)
        {
            //ColVisibility = "Collapsed";
            //ProgressRingVisibility = "Visible";
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PagePassParameter
                {
                    selectedCutup = new Cutup {cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name }
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
    }
}
