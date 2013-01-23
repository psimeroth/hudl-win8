using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Controls;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using System.Threading;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        private const int SNAPPED_FONT_SIZE = 24;
        private const int FONT_SIZE = 28;

        private DispatcherTimer timer;

        private const Visibility SNAPPED_VISIBILITY = Visibility.Collapsed;
        private const Visibility FULL_VISIBILITY = Visibility.Visible;

        private readonly INavigationService navigationService;

        private ConcurrentDictionary<string, Task<ClipResponse>> CachedCutupCalls;
        private List<CutupViewModel> CachedCutups;

        private Boolean downloading = false;

        //CancellationTokenSource cts = new CancellationTokenSource();

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

        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                NotifyOfPropertyChange(() => Visibility);
            }
        }

        private Visibility downloadButton_Visibility;
        public Visibility DownloadButton_Visibility
        {
            get { return downloadButton_Visibility; }
            set
            {
                downloadButton_Visibility = value;
                NotifyOfPropertyChange(() => DownloadButton_Visibility);
            }
        }

        private Visibility confirmButton_Visibility;
        public Visibility ConfirmButton_Visibility
        {
            get { return confirmButton_Visibility; }
            set
            {
                confirmButton_Visibility = value;
                NotifyOfPropertyChange(() => ConfirmButton_Visibility);
            }
        }

        private Visibility cancelButton_Visibility;
        public Visibility CancelButton_Visibility
        {
            get { return cancelButton_Visibility; }
            set
            {
                cancelButton_Visibility = value;
                NotifyOfPropertyChange(() => CancelButton_Visibility);
            }
        }

        private Boolean enabled_Boolean;
        public Boolean Enabled_Boolean
        {
            get { return enabled_Boolean; }
            set
            {
                enabled_Boolean = value;
                NotifyOfPropertyChange(() => Enabled_Boolean);
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

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                NotifyOfPropertyChange(() => DownloadProgress);
            }
        }

        private Visibility downloadProgress_Visibility;
        public Visibility DownloadProgress_Visibility
        {
            get { return downloadProgress_Visibility; }
            set
            {
                downloadProgress_Visibility = value;
                NotifyOfPropertyChange(() => DownloadProgress_Visibility);
            }
        }

        private Visibility progressRing_Visibility;
        public Visibility ProgressRing_Visibility
        {
            get { return progressRing_Visibility; }
            set
            {
                progressRing_Visibility = value;
                NotifyOfPropertyChange(() => ProgressRing_Visibility);
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
            string teamID;
            string seasonID;
            try
            {
                TeamContextResponse response = AppDataAccessor.GetTeamContext();
                teamID = response.teamID;
                seasonID = response.seasonID;
            }
            catch (Exception ex)
            {
                teamID = null;
                seasonID = null;
            }

			CachedCutups = new List<CutupViewModel>();
            CachedCutupCalls = new ConcurrentDictionary<string, Task<ClipResponse>>();

            if (CachedParameter.isInitialized)
            {
                SeasonsDropDown = CachedParameter.seasonsDropDown;
                SelectedSeason = CachedParameter.seasonSelected;
                Cutups = CachedParameter.sectionViewCutups;
                if (CachedParameter.categoryId != null && CachedParameter.gameId != null)
                {
                    LoadPageFromParameter(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID, CachedParameter.gameId, CachedParameter.categoryId, CachedParameter.sectionViewGames);
                }
                else
                {
                    LoadPageFromDefault(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID, CachedParameter.sectionViewGames);
                }
            }
            if (Cutups != null)
            {
                var currentViewState = ApplicationView.Value;
                if (currentViewState == ApplicationViewState.Snapped)
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = SNAPPED_VISIBILITY;
                        cutup.Thumbnail_Visibility = SNAPPED_VISIBILITY;
                        cutup.Width = new GridLength(0);
                        cutup.FontSize = SNAPPED_FONT_SIZE;
                    }
                }
                else
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = FULL_VISIBILITY;
                        cutup.Thumbnail_Visibility = FULL_VISIBILITY;
                        cutup.Width = new GridLength(180);
                        cutup.FontSize = FONT_SIZE;
                    }
                }
                if (Cutups.Count != 0)
                {
                    Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Visibility = Visibility.Visible;
            }


            DownloadProgress = CachedParameter.downloadAccessor.DownloadProgress;
            UpdateProgressBar();
            ConfirmButton_Visibility = Visibility.Collapsed;
            if (CachedParameter.downloadAccessor.downloading)
            {
                DownloadProgress_Visibility = Visibility.Visible;
                CancelButton_Visibility = Visibility.Visible;
                DownloadButton_Visibility = Visibility.Collapsed;
            }
            else
            {
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
            }
            ProgressRing_Visibility = Visibility.Collapsed;
            Enabled_Boolean = true;
        }

        private async void LoadPageFromParameter(string seasonID, string teamID, string gameID, string categoryID, BindableCollection<GameViewModel> games)
        {
            Cutups = null;
            if (games != null)
            {
                Schedule = games;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
            }
            else
            {
                await GetGames(teamID, seasonID);
            }

            // Make sure there are game entries for the season.
            if (Schedule.Any())
            {
                // Find the passed in game
                SelectedGame = Schedule.FirstOrDefault(game => game.GameId == gameID);

                // If the game isn't found set the first one as the default
                if (SelectedGame == null)
                {
                    SelectedGame = Schedule.FirstOrDefault();
                }
                await GetGameCategories(SelectedGame);

                // Make sure there are categories for the selected game
                if (Categories.Any())
                {
                    // Find the selected category
                    SelectedCategory = Categories.FirstOrDefault(cat => cat.CategoryId == categoryID);

                    // If the category isn't found set the first as the default
                    if (SelectedCategory == null)
                    {
                        SelectedCategory = Categories.FirstOrDefault();
                    }
                }
                else
                {
                    Categories = null;
                }
            }
            else
            {
                Schedule = null;
            }
        }

        private async void LoadPageFromDefault(string seasonID, string teamID, BindableCollection<GameViewModel> games)
        {
            Cutups = null;
            if (games != null)
            {
                Schedule = games;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
            }
            else
            {
                await GetGames(teamID, seasonID);
            }
            if (Schedule.Any())
            {
                if (Schedule.Contains(CachedParameter.sectionViewGameSelected))
                {
                    SelectedGame = CachedParameter.sectionViewGameSelected;
                    SelectedGame.TextColor = "#0099FF";
                    Categories = CachedParameter.sectionViewCategories;
                    SelectedCategory = CachedParameter.sectionViewCategorySelected;
                    Cutups = CachedParameter.sectionViewCutups;
                }
                else
                {
                    SelectedGame = Schedule.First();
                    await GetGameCategories(SelectedGame);
                    if (Categories.Any())
                    {
                        SelectedCategory = Categories.First();
                    }
                    else
                    {
                        Categories = null;
                    }
                }
            }
            else
            {
                Schedule = null;
            }
        }

        private async Task<ClipResponse> LoadCutup(CutupViewModel cutup)
        {
            CachedCutups.Add(cutup);
            return await ServiceAccessor.GetCutupClips(cutup);
        }

        public async Task GetGames(string teamID, string seasonID)
        {
            GameResponse response = await ServiceAccessor.GetGames(teamID.ToString(), seasonID.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var schedule = new BindableCollection<GameViewModel>();
                foreach (Game game in response.games)
                {
                    schedule.Add(GameViewModel.FromGame(game));
                }
                Schedule = new BindableCollection<GameViewModel>();
                for (int i = schedule.Count() - 1; i >= 0; i--)
                {
                    Schedule.Add(schedule[i]);
                }
            }
            else
            {
                Schedule = null;
            }
        }

        public async Task GetGameCategories(GameViewModel game)
        {
            Categories = null;
            game.TextColor = "#0099FF";
            CategoryResponse response = await ServiceAccessor.GetGameCategories(game.GameId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                var cats = new BindableCollection<CategoryViewModel>();
                foreach (Category category in response.categories)
                {
                    cats.Add(CategoryViewModel.FromCategory(category));
                }
                Categories = cats;
            }
            else
            {
                Categories = null;
            }
        }

        public void UpdateDownloadsCheckBox()
        {
            bool cutupDownloaded = false;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupDownloaded = false;
                foreach (CutupViewModel downloadedCutup in CachedParameter.downloadedCutups)
                {
                    if (downloadedCutup.CutupId == cutupVM.CutupId)
                    {
                        cutupDownloaded = true;
                        break;
                    }
                }
                if (cutupDownloaded)
                {
                    cutupVM.DownloadedVisibility = Visibility.Visible;
                }
                else
                {
                    if (downloading)
                    {
                        cutupVM.CheckBox_Visibility = Visibility.Visible;
                    }
                }
                cutupVM.CheckBox = false;
            }
        }

        public async Task GetCutupsByCategory(CategoryViewModel category)
        {
            Cutups = null;
            SelectedCategory.TextColor = "#0099FF";
            CutupResponse response = await ServiceAccessor.GetCategoryCutups(category.CategoryId.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                Cutups = new BindableCollection<CutupViewModel>();
                foreach (Cutup cutup in response.cutups)
                {
                    Cutups.Add(CutupViewModel.FromCutup(cutup));
                    Task<ClipResponse> tempResponse = LoadCutup(CutupViewModel.FromCutup(cutup));
                    CachedCutupCalls.TryAdd(cutup.cutupId, tempResponse);
                }
                UpdateDownloadsCheckBox();
            }
            var currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.Snapped)
            {
                foreach (var cutup in Cutups)
                {
                    cutup.Name_Visibility = SNAPPED_VISIBILITY;
                    cutup.Thumbnail_Visibility = SNAPPED_VISIBILITY;
                    cutup.Width = new GridLength(0);
                    cutup.FontSize = SNAPPED_FONT_SIZE;
                }
            }
            if (Cutups == null || Cutups.Count == 0)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }

        }

        public async Task<CutupViewModel> GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response;
            if (CachedCutupCalls.ContainsKey(cutup.CutupId))
            {
                // Don't need to check if it exists b/c the addition to cached cutups is in the same place as cached cutup calls
                int cutCacheIndex = CachedCutups.FindIndex(cut => cut.CutupId == cutup.CutupId);
                cutup = CachedCutups[cutCacheIndex];
                response = await CachedCutupCalls[cutup.CutupId];
            }
            else
            {
                response = await ServiceAccessor.GetCutupClips(cutup);
            }
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                return cutup;
            }
            else
            {
                Common.APIExceptionDialog.ShowGeneralExceptionDialog(null, null);
                return null;
            }
        }

        public void Cancel_Download()
        {
            if (!CachedParameter.downloadAccessor.downloading)
            {
                downloading = false; ;
                DownloadButton_Visibility = Visibility.Visible;
                ConfirmButton_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                    cutupVM.CheckBox = false;
                }
            }
            else
            {
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
                DownloadButton_Visibility = Visibility.Visible;
                CachedParameter.cts.Cancel();  
            }
        }

        public async void Confirm_Download()
        {
            List<Cutup> cutupList = new List<Cutup>();
            foreach (CutupViewModel cutupVM in Cutups)
            {
                if (cutupVM.CheckBox)
                {
                    CutupViewModel vm = await GetClipsByCutup(cutupVM);
                    cutupList.Add(new Cutup { cutupId = vm.CutupId, clips = vm.Clips, displayColumns = vm.DisplayColumns, clipCount = vm.ClipCount, name = vm.Name });
                }
                cutupVM.CheckBox_Visibility = Visibility.Collapsed;
            }
            DownloadProgress_Visibility = Visibility.Visible;
            ConfirmButton_Visibility = Visibility.Collapsed;
            DownloadProgress = CachedParameter.downloadAccessor.DownloadProgress;
            timer.Start();
            CachedParameter.cts = new CancellationTokenSource();
            CachedParameter.downloadAccessor.DownloadCutups(cutupList, CachedParameter.cts.Token);
        }

        private async Task UpdateProgressBar()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += timerTick;
            timer.Start();
        }

        void timerTick(object sender, object e)
        {
            if (!CachedParameter.downloadAccessor.downloadComplete)
            {
                //await Task.Run(() => DownloadProgress = CachedParameter.downloadAccessor.DownloadProgress);
                DownloadProgress = CachedParameter.downloadAccessor.DownloadProgress;
                if (CachedParameter.downloadAccessor.downloadCanceled)
                {
                    timer.Stop();
                }
            }
            else
            {
                DownloadProgress = 100;
                timer.Stop();
                UpdateDownloadsCheckBox();
                downloading = false;
            }
        }

        public async void GameSelected(ItemClickEventArgs eventArgs)
        {
            if (Schedule != null)
            {
                var game = (GameViewModel)eventArgs.ClickedItem;
                SelectedGame = game;
                ListView x = (ListView)eventArgs.OriginalSource;
                x.SelectedItem = game;
                Cutups = null;
                foreach (var g in Schedule.ToList())
                {
                    g.TextColor = "#E0E0E0";
                }
                await GetGameCategories(game);

                if (Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
                else
                {
                    Categories = null;
                }

                //DownloadButton_Visibility = Visibility.Visible;
                //CancelButton_Visibility = Visibility.Collapsed;
                //ConfirmButton_Visibility = Visibility.Collapsed;
                //Cancel_Download();
            }
        }

        public void CategorySelected(SelectionChangedEventArgs eventArgs)
        {
            if (Categories != null)
            {
                var category = (CategoryViewModel)eventArgs.AddedItems.FirstOrDefault();
                List<CategoryViewModel> categories = Categories.ToList();
                foreach (var cat in categories)
                {
                    cat.TextColor = "#E0E0E0";
                }

                SelectedCategory = category;
                GetCutupsByCategory(category);
            }
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            if (!downloading)
            {
                ProgressRing_Visibility = Visibility.Visible;
                Enabled_Boolean = false;
                cutup = await GetClipsByCutup(cutup);
                UpdateCachedParameter();
                CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name };
                CachedParameter.sectionViewCutupSelected = cutup;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>();
            }
            else
            {
                if (cutup.DownloadedVisibility == Visibility.Collapsed)
                {
                    cutup.CheckBox = !cutup.CheckBox;
                    CheckBoxSelected();
                }
            }
        }

        public void CheckBoxSelected()
        {
            bool checkFound = false;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                if (cutupVM.CheckBox)
                {
                    checkFound = true;
                    ConfirmButton_Visibility = Visibility.Visible;
                }
            }
            if (!checkFound)
            {
                ConfirmButton_Visibility = Visibility.Collapsed;
            }
        }

        internal void SeasonSelected(object p)
        {
            Schedule = null;
            var selectedSeason = (Season)p;
            AppDataAccessor.SetTeamContext(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
            UpdateParameterOnSeasonChange();
            Categories = null;
            LoadPageFromDefault(selectedSeason.seasonID, selectedSeason.owningTeam.teamID, null);
        }

        public void GoBack()
        {
            UpdateCachedParameter();
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                cutupVM.DownloadedVisibility = Visibility.Collapsed;
            }
            navigationService.GoBack();
        }

        public void LogOut()
        {
            navigationService.NavigateToViewModel<LoginViewModel>();
        }

        public void UpdateCachedParameter()
        {
            CachedParameter.seasonsDropDown = SeasonsDropDown;
            CachedParameter.seasonSelected = SelectedSeason;
            CachedParameter.sectionViewCutups = Cutups;
            CachedParameter.sectionViewCategorySelected = SelectedCategory;
            CachedParameter.sectionViewCategories = Categories;
            CachedParameter.sectionViewGames = Schedule;
            CachedParameter.sectionViewGameSelected = SelectedGame;
            CachedParameter.gameId = null;
            CachedParameter.categoryId = null;
        }

        public void UpdateParameterOnSeasonChange()
        {
            CachedParameter.hubViewNextGame = null;
            CachedParameter.hubViewPreviousGame = null;
        }

        public void OnWindowSizeChanged()
        {
            if (Cutups != null)
            {
                var currentViewState = ApplicationView.Value;
                if (currentViewState == ApplicationViewState.Snapped)
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = SNAPPED_VISIBILITY;
                        cutup.Thumbnail_Visibility = SNAPPED_VISIBILITY;
                        cutup.Width = new GridLength(0);
                        cutup.FontSize = SNAPPED_FONT_SIZE;
                    }
                }
                else
                {
                    foreach (var cutup in Cutups)
                    {
                        cutup.Name_Visibility = FULL_VISIBILITY;
                        cutup.Thumbnail_Visibility = FULL_VISIBILITY;
                        cutup.Width = new GridLength(180);
                        cutup.FontSize = FONT_SIZE;
                    }
                }

                if (Cutups == null || Cutups.Count == 0)
                {
                    Visibility = Visibility.Visible;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Download_Playlists()
        {
            downloading = true;
            DownloadButton_Visibility = Visibility.Collapsed;
            CancelButton_Visibility = Visibility.Visible;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                UpdateDownloadsCheckBox();
                if (cutupVM.CheckBox)
                {
                    ConfirmButton_Visibility = Visibility.Visible;
                }
            }
        }

        public void Downloads_Button()
        {
            UpdateCachedParameter();
            if (Cutups != null)
            {
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                    cutupVM.DownloadedVisibility = Visibility.Collapsed;
                }
            }
            navigationService.NavigateToViewModel<DownloadsViewModel>();
        }

    }
}
