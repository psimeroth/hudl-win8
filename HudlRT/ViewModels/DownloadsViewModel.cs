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
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;

namespace HudlRT.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        //private const int SNAPPED_FONT_SIZE = 24;
        //private const int FONT_SIZE = 28;
        //private const Visibility SNAPPED_VISIBILITY = Visibility.Collapsed;
        //private const Visibility FULL_VISIBILITY = Visibility.Visible;

        private readonly INavigationService navigationService;
        //private Boolean deleting = false;
        //public Parameter Parameter;
        //private BindableCollection<PlaylistViewModel> _playlists { get; set; }
        //public BindableCollection<PlaylistViewModel> Playlists
        //{
        //    get { return _playlists; }
        //    set
        //    {
        //        _playlists = value;
        //        NotifyOfPropertyChange(() => Playlists);
        //    }
        //}

        //private double downloadProgress;
        //public double DownloadProgress
        //{
        //    get { return downloadProgress; }
        //    set
        //    {
        //        downloadProgress = value;
        //        NotifyOfPropertyChange(() => DownloadProgress);
        //    }
        //}

        //private Visibility cancelButton_Visibility;
        //public Visibility CancelButton_Visibility
        //{
        //    get { return cancelButton_Visibility; }
        //    set
        //    {
        //        cancelButton_Visibility = value;
        //        NotifyOfPropertyChange(() => CancelButton_Visibility);
        //    }
        //}

        //private Visibility deleteButton_Visibility;
        //public Visibility DeleteButton_Visibility
        //{
        //    get { return deleteButton_Visibility; }
        //    set
        //    {
        //        deleteButton_Visibility = value;
        //        NotifyOfPropertyChange(() => DeleteButton_Visibility);
        //    }
        //}

        //private Visibility confirmButton_Visibility;
        //public Visibility ConfirmButton_Visibility
        //{
        //    get { return confirmButton_Visibility; }
        //    set
        //    {
        //        confirmButton_Visibility = value;
        //        NotifyOfPropertyChange(() => ConfirmButton_Visibility);
        //    }
        //}

        //private Visibility progress_Visibility;
        //public Visibility Progress_Visibility
        //{
        //    get { return progress_Visibility; }
        //    set
        //    {
        //        progress_Visibility = value;
        //        NotifyOfPropertyChange(() => Progress_Visibility);
        //    }
        //}

        //private Visibility no_downloads_Visibility;
        //public Visibility NoDownloadsVisibility
        //{
        //    get { return no_downloads_Visibility; }
        //    set
        //    {
        //        no_downloads_Visibility = value;
        //        NotifyOfPropertyChange(() => NoDownloadsVisibility);
        //    }
        //}        
        
        //private Visibility backButton_Visibility;
        //public Visibility BackButton_Visibility
        //{
        //    get { return backButton_Visibility; }
        //    set
        //    {
        //        backButton_Visibility = value;
        //        NotifyOfPropertyChange(() => BackButton_Visibility);
        //    }
        //}

        //private String download_Contents;
        //public String Download_Contents
        //{
        //    get { return download_Contents; }
        //    set
        //    {
        //        download_Contents = value;
        //        NotifyOfPropertyChange(() => Download_Contents);
        //    }
        //}

        public DownloadsViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
        }

        //protected override async void OnActivate()
        //{
        //    base.OnActivate();

        //    SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;

        //    Playlists = new BindableCollection<PlaylistViewModel>();
        //    CancelButton_Visibility = Visibility.Collapsed;
        //    ConfirmButton_Visibility = Visibility.Collapsed;
        //    NoDownloadsVisibility = Visibility.Collapsed;
        //    Progress_Visibility = Visibility.Collapsed;
        //    await GetDownloads();

        //    if (Playlists != null)
        //    {
        //        var currentViewState = ApplicationView.Value;
        //        if (currentViewState == ApplicationViewState.Snapped)
        //        {
        //            foreach (var playlist in Playlists)
        //            {
        //                playlist.Name_Visibility = SNAPPED_VISIBILITY;
        //                playlist.Thumbnail_Visibility = SNAPPED_VISIBILITY;
        //                playlist.Width = new GridLength(0);
        //                playlist.FontSize = SNAPPED_FONT_SIZE;
        //            }
        //        }
        //        else
        //        {
        //            foreach (var playlist in Playlists)
        //            {
        //                playlist.Name_Visibility = FULL_VISIBILITY;
        //                playlist.Thumbnail_Visibility = FULL_VISIBILITY;
        //                playlist.Width = new GridLength(180);
        //                playlist.FontSize = FONT_SIZE;
        //            }
        //        }
        //    }
        //}

        //private void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    NoDownloadsVisibility = Playlists.Any() ? Visibility.Collapsed : Visibility.Visible;
        //    Playlists = new BindableCollection<PlaylistViewModel>(Playlists.OrderByDescending(c => c.downloadedDate));
        //    long totalsize = 0;
        //    long totalClips = 0;
        //    foreach (PlaylistViewModel c in Playlists)
        //    {
        //        totalsize += c.TotalPlaylistSize;
        //        totalClips += c.ClipCount;
        //    }
        //    long megabytes = (long)Math.Ceiling((totalsize / 1048576.0));
        //    Download_Contents = "Playlists: " + Playlists.Count + " | Clips: " + totalClips + " | Size: " + megabytes + " MB";
        //}

        //public async void PlaylistSelected(ItemClickEventArgs eventArgs)
        //{
        //    var playlist = (PlaylistViewModel)eventArgs.ClickedItem;
        //    if (!deleting)
        //    {     
        //        CachedParameter.selectedPlaylist = new Playlist { playlistId = playlist.PlaylistId, clips = playlist.Clips, displayColumns = playlist.DisplayColumns, clipCount = playlist.ClipCount, name = playlist.Name, thumbnailLocation = playlist.Thumbnail };
        //        CachedParameter.sectionViewPlaylistSelected = playlist;
        //        navigationService.NavigateToViewModel<VideoPlayerViewModel>();
        //    }
        //    else
        //    {
        //        playlist.CheckBox = !playlist.CheckBox;
        //        CheckBoxSelected();
        //    }
        //}

        //public void CheckBoxSelected()
        //{
        //    bool checkFound = false;
        //    foreach (PlaylistViewModel playlistVM in Playlists)
        //    {
        //        if (playlistVM.CheckBox)
        //        {
        //            checkFound = true;
        //            ConfirmButton_Visibility = Visibility.Visible;
        //        }
        //    }
        //    if (!checkFound)
        //    {
        //        ConfirmButton_Visibility = Visibility.Collapsed;
        //    }
        //}

        //public void GoBack()
        //{
        //    if (!ServiceAccessor.ConnectedToInternet())
        //    {
        //        APIExceptionDialog.ShowNoInternetConnectionDialog(null, null);
        //    }
        //    else
        //    { 
        //        if (CachedParameter.noConnection)
        //        {
        //            CachedParameter.noConnection = false;
        //            navigationService.NavigateToViewModel<HubViewModel>();
        //        }
        //        else
        //        {
        //            navigationService.GoBack();
        //        }
        //    }
        //}

        //public void Delete_Playlists()
        //{
        //    deleting = true;
        //    DeleteButton_Visibility = Visibility.Collapsed;
        //    CancelButton_Visibility = Visibility.Visible;
        //    foreach (PlaylistViewModel playlistVM in Playlists)
        //    {
        //        playlistVM.CheckBox_Visibility = Visibility.Visible;
        //        if (playlistVM.CheckBox)
        //        {
        //            ConfirmButton_Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        //public void Cancel_Delete()
        //{
        //    deleting = false; 
        //    DeleteButton_Visibility = Visibility.Visible;
        //    CancelButton_Visibility = Visibility.Collapsed;
        //    ConfirmButton_Visibility = Visibility.Collapsed;
        //    foreach (PlaylistViewModel playlistVM in Playlists)
        //    {
        //        playlistVM.CheckBox_Visibility = Visibility.Collapsed;
        //        playlistVM.CheckBox = false;
        //    }
        //}

        //public async void Confirm_Delete()
        //{
        //    PlaylistViewModel[] copy = new PlaylistViewModel[Playlists.Count];
        //    Playlists.CopyTo(copy, 0);
        //    foreach (PlaylistViewModel playlistVM in copy)
        //    {
        //        if (playlistVM.CheckBox)
        //        {
        //            await RemoveDownload(playlistVM);
        //            Playlists.Remove(playlistVM);
        //        }
        //    }

        //    CancelButton_Visibility = Visibility.Collapsed;
        //    ConfirmButton_Visibility = Visibility.Collapsed;
        //    var totalClips = 0;
        //    if (!Playlists.Any())
        //    {
        //        DeleteButton_Visibility = Visibility.Collapsed;
        //        NoDownloadsVisibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        DeleteButton_Visibility = Visibility.Visible;
        //        NoDownloadsVisibility = Visibility.Collapsed;
        //        foreach (PlaylistViewModel playlistVM in Playlists)
        //        {
        //            playlistVM.CheckBox_Visibility = Visibility.Collapsed;
        //            totalClips += playlistVM.ClipCount;
        //        }
        //    }
        //    long totalsize = 0;
        //    foreach (PlaylistViewModel c in Playlists)
        //    {
        //        totalsize += c.TotalPlaylistSize;
        //    }
        //    long megabytes = (long)Math.Ceiling((totalsize / 1048576.0));
        //    Download_Contents = "Playlists: " + Playlists.Count + " | Clips: " + totalClips + " | Size: " + megabytes + " MB";

        //    CachedParameter.hubViewDownloadsCount = CachedParameter.downloadedPlaylists.Count != 1 ? CachedParameter.downloadedPlaylists.Count + " Playlists" : CachedParameter.downloadedPlaylists.Count + " Playlist";
        //    CachedParameter.hubViewDownloadsSizeInMB = CachedParameter.downloadedPlaylists.Count > 0 ? megabytes + " MB" : " 0 MB";

        //}


        //private async Task GetDownloads()
        //{
        //    long totalsize = 0;
        //    var totalClips = 0;
        //    Playlists = await DownloadAccessor.Instance.GetDownloads();
        //    if (!Playlists.Any())
        //    {
        //        DeleteButton_Visibility = Visibility.Collapsed;
        //        NoDownloadsVisibility = Visibility.Visible;
        //    }
        //    foreach (PlaylistViewModel cVM in Playlists)
        //    {
        //        totalsize += cVM.TotalPlaylistSize;
        //        totalClips += cVM.ClipCount;
        //    }
        //    long megabytes = (totalsize / (1048576));
        //    if (Playlists.Count > 0 && megabytes < 1)
        //    {
        //        megabytes = 1;
        //    }
        //    Download_Contents = "Playlists: " + Playlists.Count + " | Clips: " + totalClips + " | Size: " + megabytes + " MB";
        //    Playlists.CollectionChanged += Playlists_CollectionChanged;
        //}

        //private async Task RemoveDownload(PlaylistViewModel playlist)
        //{
        //    try
        //    {
        //        var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + playlist.PlaylistId.ToString());
        //        try
        //        {
        //            folder.DeleteAsync();
        //            CachedParameter.downloadedPlaylists.Remove(playlist);
        //        }
        //        catch (Exception) { }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        //public void OnWindowSizeChanged()
        //{
        //    if (Playlists != null)
        //    {
        //        var currentViewState = ApplicationView.Value;
        //        if (currentViewState == ApplicationViewState.Snapped)
        //        {
        //            foreach (var playlist in Playlists)
        //            {
        //                playlist.Name_Visibility = SNAPPED_VISIBILITY;
        //                playlist.Thumbnail_Visibility = SNAPPED_VISIBILITY;
        //                playlist.Width = new GridLength(0);
        //                playlist.FontSize = SNAPPED_FONT_SIZE;
        //            }
        //        }
        //        else
        //        {
        //            foreach (var playlist in Playlists)
        //            {
        //                playlist.Name_Visibility = FULL_VISIBILITY;
        //                playlist.Thumbnail_Visibility = FULL_VISIBILITY;
        //                playlist.Width = new GridLength(180);
        //                playlist.FontSize = FONT_SIZE;
        //            }
        //        }
        //    }
        //}

        
    }

}
