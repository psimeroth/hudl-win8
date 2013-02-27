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
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.Networking.BackgroundTransfer;
using System.Threading;

namespace HudlRT.ViewModels
{
    public class SectionViewModel : ViewModelBase
    {
        INavigationService navigationService;
        public Season Parameter { get; set; }       //Passed in from hub page - contains the game selected.
        public Game gameSelected { get; set; }
        private string _gameId;     //Used to tell if the page needs to be reloaded
        GridView categoriesGrid;
        List<Object> playlistsSelected;

        private Visibility _progressRingVisibility;
        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set
            {
                _progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        private bool _progressRingIsActive;
        public bool ProgressRingIsActive
        {
            get { return _progressRingIsActive; }
            set
            {
                _progressRingIsActive = value;
                NotifyOfPropertyChange(() => ProgressRingIsActive);
            }
        }

        private string _noPlaylistText;
        public string NoPlaylistText
        {
            get { return _noPlaylistText; }
            set
            {
                _noPlaylistText = value;
                NotifyOfPropertyChange(() => NoPlaylistText);
            }
        }

        private bool _pageIsEnabled;
        public bool PageIsEnabled
        {
            get { return _pageIsEnabled; }
            set
            {
                _pageIsEnabled = value;
                NotifyOfPropertyChange(() => PageIsEnabled);
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

        private bool appBarOpen;
        public bool AppBarOpen
        {
            get { return appBarOpen; }
            set
            {
                appBarOpen = value;
                NotifyOfPropertyChange(() => AppBarOpen);
            }
        }

        private Visibility downloading_Visibility;
        public Visibility Downloading_Visibility
        {
            get { return downloading_Visibility; }
            set
            {
                downloading_Visibility = value;
                NotifyOfPropertyChange(() => Downloading_Visibility);
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

        private Visibility deleteButton_Visibility;
        public Visibility DeleteButton_Visibility
        {
            get { return deleteButton_Visibility; }
            set
            {
                deleteButton_Visibility = value;
                NotifyOfPropertyChange(() => DeleteButton_Visibility);
            }
        }

        private BindableCollection<CategoryViewModel> _categories;
        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        public SectionViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            Categories = new BindableCollection<CategoryViewModel>();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
            //To insure the data shown is fetched if coming from the hub page to a new game
            //But that it doesn't fetch the data again if coming back from the video page.
            gameSelected = Parameter.games.FirstOrDefault();

            PageIsEnabled = true;

            ProgressRingVisibility = Visibility.Collapsed;
            ProgressRingIsActive = false;

            if (Categories.Count == 0 && (NoPlaylistText == "" || NoPlaylistText == null))
            {
                ProgressRingVisibility = Visibility.Visible;
                ProgressRingIsActive = true;
            }

            if (gameSelected.gameId != _gameId)
            {
                _gameId = gameSelected.gameId;
                GetGameCategories(_gameId);
            }
            DeleteButton_Visibility = Visibility.Collapsed;
            DownloadButton_Visibility = Visibility.Collapsed;
            Downloading_Visibility = Visibility.Collapsed;
            if (DownloadAccessor.Instance.Downloading)
            {
                Downloading_Visibility = Visibility.Visible;
                AppBarOpen = true;
            }
        }

        public async Task GetGameCategories(string gameID)
        {
            Categories = null;
            BindableCollection<CategoryViewModel> cats = new BindableCollection<CategoryViewModel>();
            foreach (Category c in gameSelected.categories)
            {
                CategoryViewModel cat = new CategoryViewModel(c);
                foreach (Playlist p in c.playlists)
                {
                    PlaylistViewModel pvm = new PlaylistViewModel(p);
                    cat.Playlists.Add(pvm);
                    pvm.FetchClips = pvm.FetchClipsAndHeaders();
                    //AddClipsAndHeadersForPlaylist(p);
                }
                if (c.playlists != null && c.playlists.Count() != 0)
                {
                    cats.Add(cat);
                }
            }

            ProgressRingVisibility = Visibility.Collapsed;
            ProgressRingIsActive = false;

            Categories = cats;
            MarkDownloadedPlaylists();

            Categories = cats;

            if (Categories.Count == 0)
            {
                NoPlaylistText = "There are no playlists for this schedule entry";
            }
            else
            {
                NoPlaylistText = "";
            }
        }

        public async Task AddClipsAndHeadersForPlaylist(Playlist playlist)
        {
            if (ServiceAccessor.ConnectedToInternet())
            {
                playlist.clips = new BindableCollection<Clip>();
                ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(playlist.playlistId);
                if (response.status == SERVICE_RESPONSE.SUCCESS)
                {
                    playlist.clips = response.clips;
                    playlist.displayColumns = response.DisplayColumns;
                }
                else
                {
                }
            }
        }

        public async void PlaylistSelected(ItemClickEventArgs eventArgs)
        {
            ProgressRingIsActive = true;
            ProgressRingVisibility = Visibility.Visible;
            PageIsEnabled = false;

            PlaylistViewModel vmClicked = (PlaylistViewModel)eventArgs.ClickedItem;
            Playlist playlistClicked = vmClicked.PlaylistModel;
            Playlist matchingDownload = DownloadAccessor.Instance.downloadedPlaylists.Where(u => u.playlistId == playlistClicked.playlistId).FirstOrDefault();
            if (matchingDownload != null)
            {
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(matchingDownload);
            }
            else
            {
                await vmClicked.FetchClips;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(playlistClicked);
            }
            

        }

        public async void DeleteButtonClick()
        {
            foreach (PlaylistViewModel playVM in playlistsSelected)
            {
                await DownloadAccessor.Instance.RemoveDownload(playVM.PlaylistModel);
            }
            MarkDownloadedPlaylists();
            categoriesGrid.SelectedItem = null;
            AppBarOpen = false;
        }

        public void CancelButtonClick()
        {
            if (DownloadAccessor.Instance.Downloading)
            {
                DownloadAccessor.Instance.cts.Cancel();
            }
            categoriesGrid.SelectedItem = null;
            AppBarOpen = false;
        }

        public async void DownloadButtonClick()
        {
            List<Playlist> playlistsToBeDownloaded = new List<Playlist>();
            foreach (PlaylistViewModel playVM in playlistsSelected)
            {
                if(playVM.PlaylistModel.clips.Count == 0)
                {
                    ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(playVM.PlaylistModel.playlistId);
                    playVM.PlaylistModel.clips = response.clips;
                    playVM.PlaylistModel.displayColumns = response.DisplayColumns;
                }
                List<Clip> additionalClips = await ServiceAccessor.GetAdditionalPlaylistClips(playVM.PlaylistModel.playlistId, playVM.PlaylistModel.clips.Count);
                foreach (Clip c in additionalClips)
                {
                    playVM.PlaylistModel.clips.Add(c);
                }
                playlistsToBeDownloaded.Add(playVM.PlaylistModel);
            }
            DownloadButton_Visibility = Visibility.Collapsed;
            Downloading_Visibility = Visibility.Visible;
            DownloadProgressText = "Determining Download Size";
            DownloadAccessor.Instance.cts = new CancellationTokenSource();
            DownloadAccessor.Instance.currentlyDownloadingPlaylists = playlistsToBeDownloaded;
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            DownloadAccessor.Instance.DownloadPlaylists(playlistsToBeDownloaded, Parameter);//TODO need to deep copy here

        }

        private void CategoriesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoriesGrid = (GridView)sender;
            PlaylistViewModel playlistAdded = (PlaylistViewModel)e.AddedItems.FirstOrDefault();

            if (categoriesGrid.SelectedItems.Count == 0)
            {
                DownloadButton_Visibility = Visibility.Collapsed;
                Downloading_Visibility = Visibility.Collapsed;
                DeleteButton_Visibility = Visibility.Collapsed;
            }

            if (categoriesGrid.SelectedItems.Count == 1 && playlistAdded != null)
            {
                if (playlistAdded.DownloadedIcon_Visibility == Visibility.Visible)
                {
                    DownloadButton_Visibility = Visibility.Collapsed;
                    Downloading_Visibility = Visibility.Collapsed;
                    DeleteButton_Visibility = Visibility.Visible;

                }
                else
                {
                    DownloadButton_Visibility = Visibility.Visible;
                    Downloading_Visibility = Visibility.Collapsed;
                    DeleteButton_Visibility = Visibility.Collapsed;
                }
            }

            if (categoriesGrid.SelectedItems.Count > 1)
            {
                PlaylistViewModel firstPlaylist = (PlaylistViewModel)playlistsSelected.ElementAt(0);
                if (playlistAdded != null)
                {
                    if (playlistAdded.DownloadedIcon_Visibility != firstPlaylist.DownloadedIcon_Visibility)
                    {
                        categoriesGrid.SelectedItems.Remove(e.AddedItems[0]);
                    }
                }
            }
            playlistsSelected = categoriesGrid.SelectedItems.ToList();
            AppBarOpen = playlistsSelected.Any() ? true : false;
        }

        private async Task LoadActiveDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            if (downloads.Count > 0)
            {
                await ResumeDownloadAsync(downloads.First());
            }
        }
        private async Task ResumeDownloadAsync(DownloadOperation downloadOperation)
        {
            await downloadOperation.AttachAsync().AsTask(DownloadAccessor.Instance.progressCallback);
        }

        private void MarkDownloadedPlaylists()
        {
            //await DownloadAccessor.Instance.GetDownloads();
            if (Categories != null)
            {
                foreach (CategoryViewModel cat in Categories)
                {
                    foreach (PlaylistViewModel pl in cat.Playlists)
                    {
                        bool downloadFound = DownloadAccessor.Instance.downloadedPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                        if (downloadFound)
                        {
                            pl.DownloadedIcon_Visibility = Visibility.Visible;
                        }
                        else
                        {
                            pl.DownloadedIcon_Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        public void ProgressCallback(DownloadOperation obj)
        {
            DownloadProgress = 100.0 * (((long)obj.Progress.BytesReceived + DownloadAccessor.Instance.CurrentDownloadedBytes) / (double)DownloadAccessor.Instance.TotalBytes);
            DownloadProgressText = DownloadAccessor.Instance.ClipsComplete + " / " + DownloadAccessor.Instance.TotalClips + " File(s)";
            if (DownloadProgress == 100)
            {
                if (Categories != null)
                {
                    foreach (CategoryViewModel cat in Categories)
                    {
                        foreach (PlaylistViewModel pl in cat.Playlists)
                        {
                            bool downloadFound = DownloadAccessor.Instance.downloadedPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                            bool currentlyDownloadingFound = DownloadAccessor.Instance.currentlyDownloadingPlaylists.Any(play => play.playlistId == pl.PlaylistModel.playlistId);
                            if (downloadFound || currentlyDownloadingFound)
                            {
                                pl.DownloadedIcon_Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                categoriesGrid.SelectedItem = null;
                DownloadAccessor.Instance.currentlyDownloadingPlaylists = new List<Playlist>();
                DownloadProgressText = "";
                DownloadProgress = 0;
                Downloading_Visibility = Visibility.Collapsed;
                AppBarOpen = false;
            }
        }
    }
}
