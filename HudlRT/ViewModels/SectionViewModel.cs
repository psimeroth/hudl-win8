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
        private SectionModel model;
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

            model = new SectionModel();
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
            
            GetGames(teamID, seasonID);

            // Check if a specific cutup was passed to the page
            if (Parameter != null)
            {
                // not sure the format right now
            }
        }

        public async void GetGames(long teamID, long seasonID)
        {
            var games = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_SCHEDULE_BY_SEASON.Replace("#", teamID.ToString()).Replace("%", seasonID.ToString()));
            if (!string.IsNullOrEmpty(games))
            {
                var schedule = new BindableCollection<GameViewModel>();
                var obj = JsonConvert.DeserializeObject<List<GameDTO>>(games);
                foreach (GameDTO gameDTO in obj)
                {
                    schedule.Add(GameViewModel.FromDTO(gameDTO));
                }
                Schedule = schedule;
                SelectedGame = Schedule.FirstOrDefault();
                GetGameCategories(SelectedGame);
            }
            else
            {
                Schedule = null;
            }
        }

        public async void GetGameCategories(GameViewModel game)
        {
            var categories = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME.Replace("#", game.GameId.ToString()));
            if (!string.IsNullOrEmpty(categories))
            {
                var cats = new BindableCollection<CategoryViewModel>();
                var obj = JsonConvert.DeserializeObject<List<CategoryDTO>>(categories);
                foreach (CategoryDTO categoryDTO in obj)
                {
                    cats.Add(CategoryViewModel.FromDTO(categoryDTO));
                }
                SelectedGame.Categories = cats;
                SelectedGame.SelectedCategory = SelectedGame.Categories.FirstOrDefault();

                GetCutupsByCategory(SelectedGame.SelectedCategory);
            }
            else
            {
                SelectedGame.Categories = null;
            }
        }

        public async void GetCutupsByCategory(CategoryViewModel category)
        {
            var cutups = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY.Replace("#", category.CategoryId.ToString()));
            if (!string.IsNullOrEmpty(cutups))
            {
                var cuts = new BindableCollection<CutupViewModel>();
                var obj = JsonConvert.DeserializeObject<List<CutupDTO>>(cutups);
                foreach (CutupDTO cutupDTO in obj)
                {
                    cuts.Add(CutupViewModel.FromDTO(cutupDTO));
                }
                Cutups = cuts;
            }
            else
            {
                Cutups = null;
            }
        }

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            var game = (GameViewModel)eventArgs.ClickedItem;

            SelectedGame.Categories = null;
            SelectedGame = game;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = game;

            GetGameCategories(game);
            Cutups = null;
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
                Common.APIExceptionDialog.ShowGeneralExceptionDialog(null, null);
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
