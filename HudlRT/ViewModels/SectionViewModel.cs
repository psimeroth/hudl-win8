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
        public PagePassParameter Parameter { get; set; }

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
        
        public BindableCollection<CutupViewModel> Cutups { get; set; }

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

        // Maps to the selected item in the category list
        private CategoryViewModel selectedCategory;
        public CategoryViewModel SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                NotifyOfPropertyChange(() => SelectedCategory);
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
                // THIS WILL NEED TO CHANGE. Possible solution is to show prompt to pick team/season although it should never be hit.
                
                // These values are here for debugging
                teamID = 57823;
                seasonID = 90841;
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
            }
            else
            {
                SelectedGame.Categories = null;
            }
        }

        /*
        public async void GetCutupsByCategory(Category category)
        {
            var cutups = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY.Replace("#", category.categoryId.ToString()));
            if (!string.IsNullOrEmpty(cutups))
            {
                category.cutups = new BindableCollection<Cutup>();
                var obj = JsonConvert.DeserializeObject<List<CutupDTO>>(cutups);
                foreach (CutupDTO cutupDTO in obj)
                {
                    category.cutups.Add(Cutup.FromDTO(cutupDTO));
                }
                Cutups = category.cutups;
            }
            else
            {
                Cutups = null;
            }
        }
        */

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

        /*
        public void CategorySelected(ItemClickEventArgs eventArgs)
        {
            var category = (Category)eventArgs.ClickedItem;

            SelectedCategory = category;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = category;

            GetCutupsByCategory(category);
        }
        */

        /*
        public void CutupSelected(ItemClickEventArgs eventArgs)
        {
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
        */

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }
}
