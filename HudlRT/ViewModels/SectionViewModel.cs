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

        private BindableCollection<CategoryViewModel> _categories { get; set; }
        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }
        
        public BindableCollection<CutupViewModel> Cutups { get; set; }

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
            }
            else
            {
                Schedule = null;
            }
        }

        public async void GetGameCategories(Game game)
        {
            var categories = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME.Replace("#", game.gameId.ToString()));
            if (!string.IsNullOrEmpty(categories))
            {
                var cats = new BindableCollection<CategoryViewModel>();
                var obj = JsonConvert.DeserializeObject<List<CategoryDTO>>(categories);
                foreach (CategoryDTO categoryDTO in obj)
                {
                    cats.Add(CategoryViewModel.FromDTO(categoryDTO));
                }
                Categories = cats;
            }
            else
            {
                Categories = null;
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

        /*
        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            var game = (Game)eventArgs.ClickedItem;

            SelectedGame = game;
            ListView x = (ListView)eventArgs.OriginalSource;
            x.SelectedItem = game;

            GetGameCategories(game);
            Cutups = null;
        }
        */

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

            navigationService.NavigateToViewModel<VideoPlayerViewModel>(new PagePassParameter
            {
                games = games,
                categories = categories,
                cutups = cutups,
                selectedGame = SelectedGame,
                selectedCategory = SelectedCategory,
                selectedCutup = cutup
            });
        }
        */

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }
    }

    /// <summary>
    /// Used for binding to a list of games
    /// </summary>
    public class GameViewModel : PropertyChangedBase
    {
        private string _opponent { get; set; }
        private string _date { get; set; }
        private bool _isHome { get; set; }
        //private BindableCollection<Category> _categories { get; set; }
        private long _gameId { get; set; }

        public static GameViewModel FromDTO(GameDTO gameDTO)
        {
            GameViewModel game = new GameViewModel();
            game._gameId = gameDTO.GameId;
            game._isHome = gameDTO.Ishome;
            game._opponent = gameDTO.Opponent;
            game._date = gameDTO.Date.ToString("d");
            return game;
        }

        public string Opponent
        {
            get { return _opponent; }
            set
            {
                if (value == _opponent) return;
                _opponent = value;
                NotifyOfPropertyChange(() => Opponent);
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                if (value == _date) return;
                _date = value;
                NotifyOfPropertyChange(() => Date);
            }
        }

        public bool IsHome
        {
            get { return _isHome; }
            set
            {
                if (value == _isHome) return;
                _isHome = value;
                NotifyOfPropertyChange(() => IsHome);
            }
        }

        public long GameId
        {
            get { return _gameId; }
            set
            {
                if (value == _gameId) return;
                _gameId = value;
                NotifyOfPropertyChange(() => GameId);
            }
        }
    }

    /// <summary>
    /// Used for binding to a list of categories
    /// </summary>
    public class CategoryViewModel : PropertyChangedBase
    {
        private string _name { get; set; }
        //private BindableCollection<Category> _cutups { get; set; }
        private long _categoryId { get; set; }

        public static CategoryViewModel FromDTO(CategoryDTO catDTO)
        {
            CategoryViewModel cat = new CategoryViewModel();
            cat._name = catDTO.Name;
            cat._categoryId = catDTO.CategoryId;
            return cat;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public long CategoryId
        {
            get { return _categoryId; }
            set
            {
                if (value == _categoryId) return;
                _categoryId = value;
                NotifyOfPropertyChange(() => CategoryId);
            }
        }
    }

    /// <summary>
    /// Used for binding to a list of cutups
    /// </summary>
    public class CutupViewModel : PropertyChangedBase
    {
        private string _name { get; set; }
        private int _clipCount { get; set; }
        private long _cutupId { get; set; }
        //private BindableCollection<Clip> _clips { get; set; }
        private string[] _displayColumns { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public int ClipCount
        {
            get { return _clipCount; }
            set
            {
                if (value == _clipCount) return;
                _clipCount = value;
                NotifyOfPropertyChange(() => ClipCount);
            }
        }

        public long CutupId
        {
            get { return _cutupId; }
            set
            {
                if (value == _cutupId) return;
                _cutupId = value;
                NotifyOfPropertyChange(() => CutupId);
            }
        }

        public string[] DisplayColumns
        {
            get { return _displayColumns; }
            set
            {
                if (value == _displayColumns) return;
                _displayColumns = value;
                NotifyOfPropertyChange(() => DisplayColumns);
            }
        }
    }
}
