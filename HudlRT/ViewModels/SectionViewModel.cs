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

        private DispatcherTimer timer = new DispatcherTimer();

        private const Visibility SNAPPED_VISIBILITY = Visibility.Collapsed;
        private const Visibility FULL_VISIBILITY = Visibility.Visible;

        private readonly INavigationService navigationService;

        private ConcurrentDictionary<string, Task<ClipResponse>> CachedCutupCalls;
        private List<CutupViewModel> CachedCutups;

        public enum DownloadMode {Selecting, Dowloading, Off};
        public DownloadMode downloadMode = new DownloadMode();

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

        private string downloadProgressText { get; set; }
        public string DownloadProgressText
        {
            get { return downloadProgressText; }
            set
            {
                downloadProgressText = value;
                NotifyOfPropertyChange(() => DownloadProgressText);
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

        private Visibility _noEntriesMessage_Visibility;
        public Visibility NoEntriesMessage_Visibility
        {
            get { return _noEntriesMessage_Visibility; }
            set
            {
                _noEntriesMessage_Visibility = value;
                NotifyOfPropertyChange(() => NoEntriesMessage_Visibility);
            }
        }

        private Visibility _scheduleProgressRing_Visibility;
        public Visibility ScheduleProgressRing_Visibility
        {
            get { return _scheduleProgressRing_Visibility; }
            set
            {
                _scheduleProgressRing_Visibility = value;
                NotifyOfPropertyChange(() => ScheduleProgressRing_Visibility);
            }
        }

        private Visibility _headerProgressRing_Visibility;
        public Visibility HeaderProgressRing_Visibility
        {
            get { return _headerProgressRing_Visibility; }
            set
            {
                _headerProgressRing_Visibility = value;
                NotifyOfPropertyChange(() => HeaderProgressRing_Visibility);
            }
        }

        private Visibility _cutupsProgressRing_Visibility;
        public Visibility CutupsProgressRing_Visibility
        {
            get { return _cutupsProgressRing_Visibility; }
            set
            {
                _cutupsProgressRing_Visibility = value;
                NotifyOfPropertyChange(() => CutupsProgressRing_Visibility);
            }
        }

        private Visibility _scheduleVisibility;
        public Visibility ScheduleVisibility
        {
            get { return _scheduleVisibility; }
            set
            {
                _scheduleVisibility = value;
                NotifyOfPropertyChange(() => ScheduleVisibility);
            }
        }

        private Visibility _headersVisibility;
        public Visibility HeadersVisibility
        {
            get { return _headersVisibility; }
            set
            {
                _headersVisibility = value;
                NotifyOfPropertyChange(() => HeadersVisibility);
            }
        }

        private Visibility _cutupsVisibility;
        public Visibility CutupsVisibility
        {
            get { return _cutupsVisibility; }
            set
            {
                _cutupsVisibility = value;
                NotifyOfPropertyChange(() => CutupsVisibility);
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

        private bool enabled_Boolean;
        public bool Enabled_Boolean
        {
            get { return enabled_Boolean; }
            set
            {
                enabled_Boolean = value;
                NotifyOfPropertyChange(() => Enabled_Boolean);
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
            ScheduleProgressRing_Visibility = Visibility.Collapsed;
            HeaderProgressRing_Visibility = Visibility.Collapsed;
            CutupsProgressRing_Visibility = Visibility.Collapsed;
            NoEntriesMessage_Visibility = Visibility.Collapsed;
            Enabled_Boolean = true;

            DownloadButton_Visibility = Visibility.Collapsed;
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
                    NoEntriesMessage_Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                NoEntriesMessage_Visibility = Visibility.Visible;
            }
            CachedParameter.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            if (DownloadAccessor.Instance.Downloading)
            {
                downloadMode = DownloadMode.Dowloading;
                LoadActiveDownloadsAsync();         
                DownloadProgress_Visibility = Visibility.Visible;
                CancelButton_Visibility = Visibility.Visible;
            }
            else
            {
                downloadMode = DownloadMode.Off;
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
            }
            ProgressRing_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
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
            if (Schedule != null && Schedule.Any())
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
                if (Categories != null && Categories.Any())
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
            if (Schedule != null && Schedule.Any())
            {
                if (Schedule.Contains(CachedParameter.sectionViewGameSelected))
                {
                    SelectedGame = CachedParameter.sectionViewGameSelected;
                    SelectedGame.TextColor = "#0099FF";
                    Categories = CachedParameter.sectionViewCategories;
                    SelectedCategory = CachedParameter.sectionViewCategorySelected;
                    Cutups = CachedParameter.sectionViewCutups;
                    MarkDownloads();
                    SetDownloadButtonVisibility();
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
                HeaderProgressRing_Visibility = Visibility.Collapsed;
            }
        }

        private async Task<ClipResponse> LoadCutup(CutupViewModel cutup)
        {
            CachedCutups.Add(cutup);
            return await ServiceAccessor.GetCutupClips(cutup);
        }

        public async Task GetGames(string teamID, string seasonID)
        {
            ScheduleVisibility = Visibility.Collapsed;
            ScheduleProgressRing_Visibility = Visibility.Visible;
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
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }
            else
            {
                Schedule = null;
            }
            ScheduleProgressRing_Visibility = Visibility.Collapsed;
            ScheduleVisibility = Visibility.Visible;
        }

        public async Task GetGameCategories(GameViewModel game)
        {
            HeadersVisibility = Visibility.Collapsed;
            HeaderProgressRing_Visibility = Visibility.Visible;
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
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }
            else
            {
                Categories = null;
            }
            HeaderProgressRing_Visibility = Visibility.Collapsed;
            HeadersVisibility = Visibility.Visible;
        }

        public void MarkDownloads()
        {
            bool downloadFound = false;
            if(Cutups != null)
            {
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    downloadFound = false;
                    foreach (CutupViewModel downloadedCutup in CachedParameter.downloadedCutups)
                    {
                        if (downloadedCutup.CutupId == cutupVM.CutupId)
                        {
                            cutupVM.DownloadedVisibility = Visibility.Visible;
                            cutupVM.CheckBox = false;
                            downloadFound = true;
                            break;
                        }
                    }
                    if (!downloadFound)
                    {
                        cutupVM.DownloadedVisibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void ShowCheckBoxes()
        {
            if (Cutups != null)
            {
                foreach (CutupViewModel cutupViewModel in Cutups)
                {
                    if (cutupViewModel.DownloadedVisibility == Visibility.Collapsed)//faster than checking if its in CachedParameter.downloadedCutups
                    {
                        cutupViewModel.CheckBox_Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public void HideCheckBoxes()
        {
            if (Cutups != null)
            {
                foreach (CutupViewModel cutupViewModel in Cutups)
                {
                    cutupViewModel.CheckBox = false;
                    if (cutupViewModel.CheckBox_Visibility == Visibility.Visible)//faster than checking if its in CachedParameter.downloadedCutups
                    {
                        cutupViewModel.CheckBox_Visibility = Visibility.Collapsed;  
                    }
                }
            }
        }

        public async Task GetCutupsByCategory(CategoryViewModel category)
        {
            NoEntriesMessage_Visibility = Visibility.Collapsed;
            CutupsVisibility = Visibility.Collapsed;
            CutupsProgressRing_Visibility = Visibility.Visible;
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
                MarkDownloads();
                SetDownloadButtonVisibility();
            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
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
                NoEntriesMessage_Visibility = Visibility.Visible;
            }
            else
            {
                NoEntriesMessage_Visibility = Visibility.Collapsed;
            }
            CutupsProgressRing_Visibility = Visibility.Collapsed;
            CutupsVisibility = Visibility.Visible;

        }

        public async Task<CutupViewModel> GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response;
            if (CachedCutupCalls.ContainsKey(cutup.CutupId) && ServiceAccessor.ConnectedToInternet())
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
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                return null;
            }
            else
            {
                Common.APIExceptionDialog.ShowGeneralExceptionDialog(null, null);
                return null;
            }
        }

        public void Cancel_Download()
        {
            if (DownloadAccessor.Instance.Downloading)
            {
                CachedParameter.cts.Cancel();  
            }
            downloadMode = DownloadMode.Off;
            ExitDownloadMode();
        }

        public void ExitDownloadMode()
        {
            
            ConfirmButton_Visibility = Visibility.Collapsed;
            if (downloadMode != DownloadMode.Dowloading)
            {
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
                downloadMode = DownloadMode.Off;
            }
            HideCheckBoxes();
            SetDownloadButtonVisibility();   
        }

        public void SetDownloadButtonVisibility()
        {
            if (Cutups != null)
            {
                int downloadedCount = 0;
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    foreach (CutupViewModel downloadedCutup in CachedParameter.downloadedCutups)
                    {
                        if (downloadedCutup.CutupId == cutupVM.CutupId)
                        {
                            downloadedCount++;
                            break;
                        }
                    }
                }
                if (downloadedCount == Cutups.Count)
                {
                    DownloadButton_Visibility = Visibility.Collapsed;
                }
                else
                {
                    DownloadButton_Visibility = Visibility.Visible;
                }
            }
        }

        public async void Confirm_Download()
        {
            DownloadProgress = 0;
            List<Cutup> cutupList = new List<Cutup>();
            foreach (CutupViewModel cutupVM in Cutups)
            {
                if (cutupVM.CheckBox)
                {
                    CutupViewModel vm = await GetClipsByCutup(cutupVM);
                    cutupList.Add(new Cutup { cutupId = vm.CutupId, clips = vm.Clips, displayColumns = vm.DisplayColumns, clipCount = vm.ClipCount, name = vm.Name, thumbnailLocation = vm.Thumbnail });
                }
                cutupVM.CheckBox_Visibility = Visibility.Collapsed;
            }
            DownloadProgress_Visibility = Visibility.Visible;
            ConfirmButton_Visibility = Visibility.Collapsed;
            DownloadProgressText = "";
            //StartTimer();
            CachedParameter.cts = new CancellationTokenSource();
            downloadMode = DownloadMode.Dowloading;
            DownloadProgressText = "Determining Download Size";
            CachedParameter.currentlyDownloadingCutups = cutupList;
            CachedParameter.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            DownloadAccessor.Instance.DownloadCutups(cutupList, SelectedSeason, SelectedGame);
        }


        private async Task LoadActiveDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            if (downloads.Count > 0)
            {
                //for simplicity we support only one download
                await ResumeDownloadAsync(downloads.First());
            }
        }
        private async Task ResumeDownloadAsync(DownloadOperation downloadOperation)
        {
            await downloadOperation.AttachAsync().AsTask(CachedParameter.progressCallback);
        }

        public void ProgressCallback(DownloadOperation obj)
        {
            if (CachedParameter.cts.IsCancellationRequested)
            {
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
                downloadMode = DownloadMode.Off;
                //DownloadButton_Visibility = downloadedCutupCount == Cutups.Count ? Visibility.Collapsed : Visibility.Visible;
                DownloadProgress = 0;
            }
            DownloadProgress = 100.0 * (((long)obj.Progress.BytesReceived + DownloadAccessor.Instance.CurrentDownloadedBytes) / (double)DownloadAccessor.Instance.TotalBytes);
            DownloadProgressText = DownloadAccessor.Instance.ClipsComplete + " / " + DownloadAccessor.Instance.TotalClips + " File(s)";
            int downloadedCutupCount = 0;
            if (DownloadProgress == 100)
            {
                bool downloadFound = false;
                if (Cutups != null)
                {
                    foreach (CutupViewModel cutupVM in Cutups)
                    {
                        foreach (Cutup downloadedCutup in CachedParameter.currentlyDownloadingCutups)
                        {
                            if (downloadedCutup.cutupId == cutupVM.CutupId)
                            {
                                cutupVM.DownloadedVisibility = Visibility.Visible;
                                cutupVM.CheckBox = false;
                                downloadedCutupCount++;
                                break;
                            }
                            else if (cutupVM.DownloadedVisibility == Visibility.Visible)
                            {
                                downloadedCutupCount++;
                            }
                        }
                    }
                }
                DownloadProgress_Visibility = Visibility.Collapsed;
                CancelButton_Visibility = Visibility.Collapsed;
                downloadMode = DownloadMode.Off;
                DownloadButton_Visibility = downloadedCutupCount == Cutups.Count ? Visibility.Collapsed : Visibility.Visible;
                CachedParameter.currentlyDownloadingCutups = new List<Cutup>();
                DownloadProgress = 0;
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

                if (Categories != null && Categories.Any())
                {
                    SelectedCategory = Categories.First();
                }
                else
                {
                    Categories = null;
                }

                if (Cutups != null)
                {
                    ExitDownloadMode();
                }
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
            ExitDownloadMode();
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            if (downloadMode == DownloadMode.Off || downloadMode == DownloadMode.Dowloading)
            {
                bool downloadfound = false;
                ProgressRing_Visibility = Visibility.Visible;
                foreach (CutupViewModel cVM in CachedParameter.downloadedCutups)
                {
                    if (cVM.CutupId == cutup.CutupId)
                    {
                        downloadfound = true;
                        cutup = cVM;
                        break;
                    }
                }
                if (!downloadfound)
                {
                    cutup = await GetClipsByCutup(cutup);
                }
                if (cutup != null)
                {
                    UpdateCachedParameter();
                    CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name, thumbnailLocation = cutup.Thumbnail };
                    CachedParameter.sectionViewCutupSelected = cutup;
                    navigationService.NavigateToViewModel<VideoPlayerViewModel>();
                }
            }
            else if(downloadMode == DownloadMode.Selecting)
            {
                if (cutup.DownloadedVisibility == Visibility.Collapsed)
                {
                    cutup.CheckBox = !cutup.CheckBox;
                    CheckBoxSelected();
                }
            }
        }

        public void ProgressUpdated(double percent)
        {
            DownloadProgress = percent;
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
            HideCheckBoxes();
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
                    NoEntriesMessage_Visibility = Visibility.Visible;
                }
                else
                {
                    NoEntriesMessage_Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Download_Playlists()
        {
            downloadMode = DownloadMode.Selecting;
            DownloadButton_Visibility = Visibility.Collapsed;
            CancelButton_Visibility = Visibility.Visible;
            ShowCheckBoxes();
        }

        public void Downloads_Button()
        {
            UpdateCachedParameter();
            HideCheckBoxes();
            navigationService.NavigateToViewModel<DownloadsViewModel>();
        }

        public void Downloads_Button_Snapped()
        {
            UpdateCachedParameter();
            HideCheckBoxes();
            navigationService.NavigateToViewModel<DownloadsViewModel>();
        }

    }
}
