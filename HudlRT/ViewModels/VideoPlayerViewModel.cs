using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HudlRT.Parameters;
using HudlRT.Models;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.System.Display;
using System.Threading;
using Windows.UI.Xaml.Data;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public PageParameter Parameter { get; set; }       //Passed in from hub page - contains the game Id.
        private DisplayRequest dispRequest = null;
        private PlaybackType playbackType;
        private List<Clip> Clips { get; set; }

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

        private string columnHeaderName;
        public string ColumnHeaderName
        {
            get { return columnHeaderName; }
            set
            {
                columnHeaderName = value;
                NotifyOfPropertyChange(() => ColumnHeaderName);
            }
        }
        private bool isLoopChecked;
        public bool IsLoopChecked
        {
            get { return isLoopChecked; }
            set
            {
                isLoopChecked = value;
                NotifyOfPropertyChange(() => IsLoopChecked);
            }
        }
        private bool isOnceChecked;
        public bool IsOnceChecked
        {
            get { return isOnceChecked; }
            set
            {
                isOnceChecked = value;
                NotifyOfPropertyChange(() => IsOnceChecked);
            }
        }
        private bool isNextChecked;
        public bool IsNextChecked
        {
            get { return isNextChecked; }
            set
            {
                isNextChecked = value;
                NotifyOfPropertyChange(() => IsNextChecked);
            }
        }

        private Visibility downloadButtonVisibility;
        public Visibility DownloadButtonVisibility
        {
            get { return downloadButtonVisibility; }
            set
            {
                downloadButtonVisibility = value;
                NotifyOfPropertyChange(() => DownloadButtonVisibility);
            }
        }

        private Visibility downloadedVisibility;
        public Visibility DownloadedVisibility
        {
            get { return downloadedVisibility; }
            set
            {
                downloadedVisibility = value;
                NotifyOfPropertyChange(() => DownloadedVisibility);
            }
        }

        private Visibility progressGridVisibility;
        public Visibility ProgressGridVisibility
        {
            get { return progressGridVisibility; }
            set
            {
                progressGridVisibility = value;
                NotifyOfPropertyChange(() => ProgressGridVisibility);
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

        private ObservableCollection<Clip> filteredClips;
        public ObservableCollection<Clip> FilteredClips
        {
            get { return filteredClips; }
            set
            {
                filteredClips = value;
                NotifyOfPropertyChange(() => FilteredClips);
            }
        }
        private Angle selectedAngle;
        public Angle SelectedAngle
        {
            get { return selectedAngle; }
            set
            {
                selectedAngle = value;
                if (videoMediaElement != null)
                {
                    videoMediaElement.Source = (selectedAngle != null) ? new Uri(SelectedAngle.fileLocation) : null;
                }
            }   
        }

        private string[] gridHeaders;
        public string[] GridHeaders
        {
            get { return gridHeaders; }
            set
            {
                gridHeaders = value;
                NotifyOfPropertyChange(() => GridHeaders);
            }
        }
        private string playlistName;
        public string PlaylistName
        {
            get { return playlistName; }
            set
            {
                playlistName = value;
                NotifyOfPropertyChange(() => PlaylistName);
            }
        }
        private Clip selectedClip;
        public Clip SelectedClip
        {
            get { return selectedClip; }
            set
            {
                selectedClip = value;
                NotifyOfPropertyChange(() => SelectedClip);
            }
        }
        private string toggleButtonContent;
        public string ToggleButtonContent
        {
            get { return toggleButtonContent; }
            set
            {
                toggleButtonContent = value;
                NotifyOfPropertyChange(() => ToggleButtonContent);
            }
        }
        private BindableCollection<AngleType> angleNames;
        public BindableCollection<AngleType> AngleTypes
        {
            get { return angleNames; }
            set
            {
                angleNames = value;
                NotifyOfPropertyChange(() => AngleTypes);
            }
        }
        private FilterViewModel selectedFilter;
        public FilterViewModel SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                NotifyOfPropertyChange(() => SelectedFilter);
            }
        }

        private int SelectedClipIndex = 0;
        Point initialPoint = new Point();
        Point currentPoint;
        bool isFullScreenGesture = false;
        public ListView listView { get; set; }
        private List<FilterViewModel> FiltersList { get; set; }
        public Windows.UI.Xaml.Controls.Primitives.Popup SortFilterPopupControl { get; set; }
        private CancellationTokenSource preloadCTS { get; set; }
        private CancellationToken preloadCT { get; set; }
        public List<TextBlock> ColumnHeaderTextBlocks { get; set; }
        private Microsoft.PlayerFramework.MediaPlayer videoMediaElement { get; set; }
        public List<string> GridHeadersTextSorted { get; set; }
        public List<string> GridHeadersTextUnsorted { get; set; }
        public Windows.UI.Xaml.Controls.AppBar TopAppBar { get; set; }
        public Windows.UI.Xaml.Controls.AppBar BottomAppBar { get; set; }

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }
        
        protected override async void OnActivate()
        {
            base.OnActivate();

            AppDataAccessor.SetLastViewed(Parameter.playlist.name, DateTime.Now.ToString("g"), Parameter.playlist.playlistId, Parameter.playlist.thumbnailLocation);
            Clips = Parameter.playlist.clips.ToList();
            
            FilteredClips = new ObservableCollection<Clip>(Clips);
            if (FilteredClips.Any())
            {
                getAngleNames();
                SelectedClip = FilteredClips.First();
                SelectedClipIndex = 0;
                Angle nextAngle = SelectedClip.angles.FirstOrDefault(angle => angle.angleType.IsChecked);
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                if (listView != null)
                {
                    listView.SelectedItem = SelectedClip;
                }
            }

            getMoreClips();

            GridHeaders = Parameter.playlist.displayColumns;
            PlaylistName = Parameter.playlist.name;

            int? playbackTypeResult = AppDataAccessor.GetPlaybackType();
            if (playbackTypeResult == null)
            {
                AppDataAccessor.SetPlaybackType((int)PlaybackType.once);
                playbackType = PlaybackType.once;
            }
            else
            {
                playbackType = (PlaybackType)playbackTypeResult;
            }
            setPlaybackRadioButton();

            dispRequest = new DisplayRequest();
            dispRequest.RequestActive();

            FiltersList = new List<FilterViewModel>();

            preloadCTS = new CancellationTokenSource();
            preloadCT = preloadCTS.Token;
            initialClipPreload();

            bool downloadFound = false;
            BindableCollection<Playlist> downloadedPlaylists = DownloadAccessor.Instance.downloadedPlaylists;
            foreach (Playlist p in downloadedPlaylists)
            {
                if (p.playlistId == Parameter.playlist.playlistId)
                {
                    downloadFound = true;
                    break;
                }
            }
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            DownloadedVisibility = Visibility.Collapsed;
            if (DownloadAccessor.Instance.Downloading)
            {
                LoadActiveDownloadsAsync();
                ProgressGridVisibility = Visibility.Visible;
                DownloadButtonVisibility = Visibility.Collapsed;
            }
            else
            {
                ProgressGridVisibility = Visibility.Collapsed;
                if (!downloadFound)
                {
                    DownloadButtonVisibility = Visibility.Visible;
                }
                else
                {
                    DownloadButtonVisibility = Visibility.Collapsed;
                    DownloadedVisibility = Visibility.Visible;
                }
            }
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            TopAppBar.IsOpen = true;
            BottomAppBar.IsOpen = true;
        }

        private async Task LoadActiveDownloadsAsync()
        {
            if (DownloadAccessor.Instance.Downloading)
            {
                await ResumeDownloadAsync();
            }
        }
        private async Task ResumeDownloadAsync()
        {
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            await DownloadAccessor.Instance.Download.AttachAsync().AsTask(DownloadAccessor.Instance.progressCallback);
        }

        private async void initialClipPreload()
        {
            if (FilteredClips.Any())
            {
                PreloadClips(preloadCT, FilteredClips[0].angles.Where(angle => angle.angleType.IsChecked).Take(2));
            }
        }

        private async void getMoreClips()
        {
            List<Clip> remainingClipsList = await ServiceAccessor.GetAdditionalPlaylistClips(Parameter.playlist.playlistId, 100);
            foreach (Clip clip in remainingClipsList)
            {
                foreach (Angle angle in clip.angles)
                {
                    angle.angleType = AngleTypes.FirstOrDefault(angleType => angleType.Name.Equals(angle.angleName));
                }
            }
            foreach (Clip c in remainingClipsList)
            {
                Clips.Add(c);
                if (!FiltersList.Any())
                {
                    FilteredClips.Add(c);
                }
            }
        }

        private void getAngleNames()
        {
            HashSet<string> types = new HashSet<string>();
            foreach (Clip clip in Parameter.playlist.clips)
            {
                foreach (Angle angle in clip.angles)
                {
                    types.Add(angle.angleName);
                }
            }

            BindableCollection<AngleType> typeObjects = new BindableCollection<AngleType>();
            foreach (string s in types)
            {
                typeObjects.Add(new AngleType(s, this));
            }

            AngleTypes = typeObjects;
            foreach (Clip clip in Parameter.playlist.clips)
            {
                foreach (Angle angle in clip.angles)
                {
                    angle.angleType = AngleTypes.FirstOrDefault(angleType => angleType.Name.Equals(angle.angleName));
                }
            }

            getAnglePreferences();
        }

        private void getAnglePreferences()
        {
            foreach (AngleType angleName in AngleTypes)
            {
                bool? angleChecked = AppDataAccessor.GetAnglePreference(angleName.Name);

                if (angleChecked == null)
                {
                    angleName.IsChecked = true;
                }
                else
                {
                    angleName.IsChecked = (bool)angleChecked;
                }
            }
        }

        private void saveAnglePreferences()
        {
            foreach (AngleType angleName in AngleTypes)
            {
                AppDataAccessor.SetAnglePreference(angleName.Name, angleName.IsChecked);
            }
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            SetClip(clip);
        }

        public void SetClip(Clip clip)
        {
            if (clip != null && (SelectedClip == null || SelectedClip.clipId != clip.clipId))
            {
                SelectedClip = clip;
                SelectedClipIndex = FilteredClips.IndexOf(clip);

                listView.SelectedItem = SelectedClip;

                Angle nextAngle = clip.angles.FirstOrDefault(angle => angle.angleType.IsChecked);
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                
                int nextClipIndex = (SelectedClipIndex + 1) % FilteredClips.Count;
                PreloadClips(preloadCT, SelectedClip.angles.Where(angle => angle.angleType.IsChecked).Take(2));
            }
            else
            {
                listView.SelectedItem = SelectedClip;
            }
        }

        public void NextClip(NextAngleEvent eventType)
        {
            if (SelectedClip != null)
            {
                if (SelectedAngle == null)
                {
                    GoToNextClip();
                }
                else
                {
                    List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList();
                    Angle currentAngle = SelectedClip.angles.FirstOrDefault(a => a.clipAngleId == SelectedAngle.clipAngleId);

                    int angleIndex = filteredAngles.IndexOf(currentAngle);
                    if (angleIndex < filteredAngles.Count - 1)
                    {
                        Angle nextAngle = filteredAngles[angleIndex + 1];
                        SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                        if (angleIndex + 2 < filteredAngles.Count)
                        {
                            List<Angle> angles = new List<Angle>();
                            angles.Add(filteredAngles[angleIndex + 2]);
                            PreloadClips(preloadCT, angles);
                        }
                        else if (FilteredClips.Count > 1 && FilteredClips[(SelectedClipIndex + 1) % FilteredClips.Count].angles.Where(a => a.angleType.IsChecked).Any())
                        {
                            List<Angle> angles = new List<Angle>();
                            angles.Add(FilteredClips[(SelectedClipIndex + 1) % FilteredClips.Count].angles.Where(a => a.angleType.IsChecked).First());
                            PreloadClips(preloadCT, angles);
                        }
                    }
                    else
                    {
                        if (eventType == NextAngleEvent.mediaEnded && playbackType == PlaybackType.loop)
                        {
                            if (filteredAngles.Any())
                            {
                                Angle nextAngle = filteredAngles[0];
                                SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : new Angle(nextAngle.clipAngleId, nextAngle.fileLocation);
                            }
                            else
                            {
                                SelectedAngle = null;
                            }
                        }
                        else if (eventType == NextAngleEvent.buttonClick || playbackType == PlaybackType.next)
                        {
                            GoToNextClip();
                        }
                    }
                }
            }
        }

        public void GoToNextClip()
        {
            if (FilteredClips.Count > 1 && FilteredClips.Any(c => c.angles.Any(a => a.angleType.IsChecked)))
            {
                SelectedAngle = null;
                while (SelectedAngle == null)
                {
                    SelectedClipIndex = (SelectedClipIndex + 1) % FilteredClips.Count;

                    SelectedClip = FilteredClips[SelectedClipIndex];
                    listView.SelectedItem = SelectedClip;
                    Angle nextAngle = SelectedClip.angles.FirstOrDefault(angle => angle.angleType.IsChecked);
                    SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                }
                if (FilteredClips[SelectedClipIndex].angles.Where(angle => angle.angleType.IsChecked).Any())
                {
                    PreloadClips(preloadCT, FilteredClips[SelectedClipIndex].angles.Where(angle => angle.angleType.IsChecked).Take(2));
                }
            }
        }

        public void PreviousClip(ItemClickEventArgs eventArgs)
        {
            if (SelectedClip != null)
            {
                if (SelectedAngle == null)
                {
                    GoToPreviousClip();
                }
                else
                {
                    List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                    Angle currentAngle = SelectedClip.angles.FirstOrDefault(a => a.clipAngleId == SelectedAngle.clipAngleId);

                    int angleIndex = filteredAngles.IndexOf(currentAngle);
                    if (angleIndex > 0)
                    {
                        Angle nextAngle = filteredAngles[angleIndex - 1];
                        SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                    }
                    else
                    {
                        GoToPreviousClip();
                    }
                }
            }
        }

        public void GoToPreviousClip()
        {
            if (FilteredClips.Count > 1 && FilteredClips.Any(c => c.angles.Any(a => a.angleType.IsChecked)))
            {
                SelectedAngle = null;
                while (SelectedAngle == null)
                {
                    SelectedClipIndex = (SelectedClipIndex == 0) ? FilteredClips.Count - 1 : SelectedClipIndex - 1;

                    SelectedClip = FilteredClips[SelectedClipIndex];
                    listView.SelectedItem = SelectedClip;
                    Angle nextAngle = SelectedClip.angles.FirstOrDefault(angle => angle.angleType.IsChecked);
                    SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                }
                if (FilteredClips[SelectedClipIndex].angles.Where(angle => angle.angleType.IsChecked).Any())
                {
                    PreloadClips(preloadCT, FilteredClips[SelectedClipIndex].angles.Where(angle => angle.angleType.IsChecked).Take(2));
                }
            }
        }

        public void ResetClip()
        {
            Angle firstAngle = SelectedClip.angles.FirstOrDefault(angle => angle.angleType.IsChecked);
            if (firstAngle != null)
            {
                SelectedAngle = (firstAngle.isPreloaded) ? new Angle(firstAngle.clipAngleId, firstAngle.preloadFile) : new Angle(firstAngle.clipAngleId, firstAngle.fileLocation);
            }
            else
            {
                SelectedAngle = firstAngle;
            }
        }

        public void AngleFilter()
        {
            List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();

            int nextClipIndex = (SelectedClipIndex + 1) % FilteredClips.Count;
            PreloadClips(preloadCT, filteredAngles.Take(2));

            //If the current angle has been filtered out, reset the clip to the first unfiltered angle, or null
            if (SelectedAngle != null)
            {
                if (filteredAngles.FirstOrDefault(angle => angle.clipAngleId == SelectedAngle.clipAngleId) == null)
                {
                    Angle nextAngle = filteredAngles.FirstOrDefault();
                    SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
                }
            }
            else
            {
                Angle nextAngle = filteredAngles.FirstOrDefault();
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile) : nextAngle;
            }
        }
        
        public void ApplySelectedFilter()
        {
            if (SelectedFilter.sortType != SortType.None || SelectedFilter.FilterCriteria.Where(f => f.IsChecked).Any())
            {
                ColumnHeaderTextBlocks[SelectedFilter.columnId].Foreground = (Windows.UI.Xaml.Media.Brush)Windows.UI.Xaml.Application.Current.Resources["HudlBlue"];

                if (ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.Count > 1)
                {
                    ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.RemoveAt(1);
                }
                if (SelectedFilter.sortType == SortType.Ascending)
                {
                    Run text = new Run();
                    text.Text = " \u25B2";
                    ColumnHeaderTextBlocks[SelectedFilter.columnId].Text = GridHeadersTextSorted[SelectedFilter.columnId];
                    ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.Add(text);
                }
                else if (SelectedFilter.sortType == SortType.Descending)
                {
                    Run text = new Run();
                    text.Text = " \u25BC";
                    ColumnHeaderTextBlocks[SelectedFilter.columnId].Text = GridHeadersTextSorted[SelectedFilter.columnId];
                    ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.Add(text);
                }

                List<Clip> newFilteredClips = new List<Clip>();
                List<Clip> currentFilteredClips;

                if (FiltersList.Contains(SelectedFilter))
                {
                    currentFilteredClips = removeFilter();
                }
                else
                {
                    currentFilteredClips = FilteredClips.ToList();
                }

                if (SelectedFilter.FilterCriteria != null && SelectedFilter.FilterCriteria.Any(c => c.IsChecked))
                {
                    List<String> filtersApplied = new List<String>();
                    foreach (FilterCriteriaViewModel criteria in SelectedFilter.FilterCriteria.Where(c => c.IsChecked))
                    {
                        newFilteredClips.AddRange(currentFilteredClips.Where(clip => clip.breakDownData[SelectedFilter.columnId].Equals(criteria.Name)));
                        filtersApplied.Add(criteria.Name);
                    }
                    Logger.Instance.LogFilterApplied(filtersApplied);
                }
                else
                {
                    newFilteredClips.AddRange(currentFilteredClips);
                }

                FilterViewModel currentSortFilter = FiltersList.FirstOrDefault(f => f.sortType != SortType.None);
                if (SelectedFilter.sortType == SortType.Ascending || SelectedFilter.sortType == SortType.Descending)
                {
                    if (currentSortFilter != null)
                    {
                        if (!currentSortFilter.FilterCriteria.Any(c => c.IsChecked))
                        {
                            ColumnHeaderTextBlocks[currentSortFilter.columnId].Foreground = (Windows.UI.Xaml.Media.Brush)Windows.UI.Xaml.Application.Current.Resources["HudlMediumDarkGray"];
                            if (ColumnHeaderTextBlocks[currentSortFilter.columnId].Inlines.Count > 1)
                            {
                                ColumnHeaderTextBlocks[currentSortFilter.columnId].Inlines.RemoveAt(1);
                                ColumnHeaderTextBlocks[currentSortFilter.columnId].Text = GridHeadersTextUnsorted[currentSortFilter.columnId];
                            }
                            FiltersList.Remove(currentSortFilter);
                        }
                        else
                        {
                            if (ColumnHeaderTextBlocks[currentSortFilter.columnId].Inlines.Count > 1)
                            {
                                ColumnHeaderTextBlocks[currentSortFilter.columnId].Inlines.RemoveAt(1);
                                ColumnHeaderTextBlocks[currentSortFilter.columnId].Text = GridHeadersTextUnsorted[currentSortFilter.columnId];
                            }
                            currentSortFilter.setSortType(SortType.None);
                        }
                    }
                    currentSortFilter = SelectedFilter;
                }

                sortClips(ref newFilteredClips, currentSortFilter);
                FiltersList.Add(SelectedFilter);
                applyFilter(newFilteredClips);
            }
            else
            {
                if (FiltersList.Contains(SelectedFilter))
                {
                    RemoveSelectedFilter();
                }
            }
        }

        public void RemoveSelectedFilter()
        {
            ColumnHeaderTextBlocks[SelectedFilter.columnId].Foreground = (Windows.UI.Xaml.Media.Brush)Windows.UI.Xaml.Application.Current.Resources["HudlMediumDarkGray"];
            if (ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.Count > 1)
            {
                ColumnHeaderTextBlocks[SelectedFilter.columnId].Inlines.RemoveAt(1);
                ColumnHeaderTextBlocks[SelectedFilter.columnId].Text = GridHeadersTextUnsorted[SelectedFilter.columnId];
            }
            List<Clip> clips = removeFilter();
            sortClips(ref clips, FiltersList.FirstOrDefault(f => f.sortType != SortType.None));
            applyFilter(clips);
        }

        private List<Clip> removeFilter()
        {
            FiltersList.Remove(SelectedFilter);

            List<Clip> clips = new List<Clip>();
            List<Clip> allClips = new List<Clip>();
            allClips.AddRange(Clips);

            foreach (FilterViewModel filter in FiltersList)
            {
                if (filter.FilterCriteria != null && filter.FilterCriteria.Where(c => c.IsChecked).Any())
                {
                    foreach (FilterCriteriaViewModel criteria in filter.FilterCriteria.Where(c => c.IsChecked))
                    {
                        clips.AddRange(allClips.Where(clip => clip.breakDownData[filter.columnId].Equals(criteria.Name)));
                    }
                }
                else
                {
                    clips.AddRange(allClips);
                }

                allClips.Clear();
                allClips.AddRange(clips);
                clips.Clear();
            }

            return allClips;
        }

        private void applyFilter(List<Clip> clips)
        {
            SelectedClipIndex = 0;
            SelectedClip = null;
            SelectedAngle = null;
            FilteredClips = new ObservableCollection<Clip>(clips);

            if (FilteredClips.Any())
            {
                SetClip(FilteredClips.First());
            }

            SortFilterPopupControl.IsOpen = false;
        }

        private void sortClips(ref List<Clip> clips, FilterViewModel filter)
        {
            if (filter != null)
            {
                if (filter.sortType != SortType.None)
                {
                    List<Clip> unfilteredClips = new List<Clip>();
                    unfilteredClips.AddRange(clips.Where(clip => clip.breakDownData[filter.columnId].Equals("-")));
                    foreach (Clip clip in unfilteredClips)
                    {
                        clips.Remove(clip);
                    }

                    if (filter.sortType == SortType.Ascending)
                    {
                        clips = clips.OrderBy(c => c.order).ToList();
                        try
                        {
                            clips = clips.OrderBy(clip => Convert.ToInt32(clip.breakDownData[filter.columnId])).ToList();
                        }
                        catch
                        {
                            clips = clips.OrderBy(clip => clip.breakDownData[filter.columnId]).ToList();
                        }
                    }
                    else if (filter.sortType == SortType.Descending)
                    {
                        clips = clips.OrderBy(c => c.order).ToList();
                        try
                        {
                            clips = clips.OrderByDescending(clip => Convert.ToInt32(clip.breakDownData[filter.columnId])).ToList();
                        }
                        catch
                        {
                            clips = clips.OrderByDescending(clip => clip.breakDownData[filter.columnId]).ToList();
                        }
                    }

                    clips.AddRange(unfilteredClips);
                }

                Logger.Instance.LogSortApplied(filter);
            }
            else
            {
                clips = clips.OrderBy(c => c.order).ToList();
            }
        }

        public void PrepareSortFilterPopup(int id)
        {
            FilterViewModel filter = FiltersList.FirstOrDefault(f => f.columnId == id);

            if (filter == null)
            {
                List<string> breakdownData = GetBreakdownDataValues(id);
                BindableCollection<FilterCriteriaViewModel> filterCriteria = new BindableCollection<FilterCriteriaViewModel>();
                foreach (string criteria in breakdownData)
                {
                    filterCriteria.Add(new FilterCriteriaViewModel(id, criteria));
                }

                filter = new FilterViewModel(id, Parameter.playlist.displayColumns[id], SortType.None, filterCriteria, this);
            }
            else
            {
                filter.setSortType(filter.sortType);
                filter.RemoveButtonVisibility = "Visible";
            }

            SelectedFilter = filter;
        }

        public List<string> GetBreakdownDataValues(int column)
        {
            HashSet<string> values = new HashSet<string>();
            foreach (Clip clip in Clips)
            {
                values.Add(clip.breakDownData[column]);
            }
            values.Remove("-");
            return values.ToList();
        }

        private void setPlaybackRadioButton()
        {
            switch (playbackType)
            {
                case PlaybackType.once:
                    IsOnceChecked = true;
                    IsNextChecked = false;
                    IsLoopChecked = false;
                    break;
                case PlaybackType.next:
                    IsOnceChecked = false;
                    IsNextChecked = true;
                    IsLoopChecked = false;
                    break;
                case PlaybackType.loop:
                    IsOnceChecked = false;
                    IsNextChecked = false;
                    IsLoopChecked = true;
                    break;
            }
        }

        public void PlaybackButton_Click(int playbackTypeSelected)
        {
            playbackType = (PlaybackType)playbackTypeSelected;
            setPlaybackRadioButton();
            AppDataAccessor.SetPlaybackType((int)playbackType);
        }

        private void DownloadButtonClick()
        {
            ProgressGridVisibility = Visibility.Visible;
            DownloadButtonVisibility = Visibility.Collapsed;
            DownloadProgress = 0;
            DownloadAccessor.Instance.cts = new CancellationTokenSource();
            DownloadProgressText = "Preparing Download";
            Playlist playlistCopy = Playlist.Copy(Parameter.playlist);
            List<Playlist> currentPlaylistList = new List<Playlist> { playlistCopy };
            DownloadAccessor.Instance.currentlyDownloadingPlaylists = currentPlaylistList;
            DownloadAccessor.Instance.progressCallback = new Progress<DownloadOperation>(ProgressCallback);
            DownloadAccessor.Instance.DownloadPlaylists(currentPlaylistList, Parameter.season);
        }

        public void CancelButtonClick()
        {
            if (DownloadAccessor.Instance.Downloading)
            {
                DownloadAccessor.Instance.cts.Cancel();
            }
            DownloadedVisibility = Visibility.Collapsed;
            ProgressGridVisibility = Visibility.Collapsed;
            DownloadButtonVisibility = Visibility.Visible;
        }

        public void ProgressCallback(DownloadOperation obj)
        {
            DownloadProgress = 100.0 * (((long)obj.Progress.BytesReceived + DownloadAccessor.Instance.CurrentDownloadedBytes) / (double)DownloadAccessor.Instance.TotalBytes);
            DownloadProgressText = DownloadAccessor.Instance.ClipsComplete + " / " + DownloadAccessor.Instance.TotalClips + " File(s)";
            if (DownloadProgress == 100)
            {
                ProgressGridVisibility = Visibility.Collapsed;
                DownloadAccessor.Instance.currentlyDownloadingPlaylists = new List<Playlist>();
                DownloadProgress = 0;
                DownloadedVisibility = Visibility.Visible;
            }
        }

        void videoMediaElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((currentPoint.X == 0 && currentPoint.Y == 0) || (currentPoint.X - e.Position.X <= 50 && currentPoint.X - e.Position.X >= -50))
                currentPoint = e.Position;

            if (e.Delta.Scale >= 1.1 || e.Delta.Scale <= .92)
                isFullScreenGesture = true;
        }

        void videoMediaElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            isFullScreenGesture = false;
            initialPoint = e.Position;
            currentPoint = new Point();
        }

        void videoMediaElement_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventHandler e)
        {
            if (initialPoint.X - currentPoint.X >= 50 && !isFullScreenGesture)
            {
                NextClip(NextAngleEvent.buttonClick);
            }

            else if (initialPoint.X - currentPoint.X <= -50 && !isFullScreenGesture)
            {
                PreviousClip(null);
            }
        }

        public void setVideoMediaElement(Microsoft.PlayerFramework.MediaPlayer videoMediaElement)
        {
            this.videoMediaElement = videoMediaElement;
            videoMediaElement.Source = (SelectedAngle != null) ? new Uri(SelectedAngle.fileLocation) : null;
        }

        private async Task PreloadClips(CancellationToken ct, IEnumerable<Angle> angles)
        {
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            foreach (Angle angle in angles)
            {
                if (!ct.IsCancellationRequested && !angle.isPreloaded)
                {
                    try
                    {
                        var source = new Uri(angle.fileLocation);
                        var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
                        var file = files.FirstOrDefault(x => x.Name.Equals(angle.clipAngleId.ToString()));

                        if (file == null)
                        {
                            var destinationFile = await folder.CreateFileAsync(angle.clipAngleId.ToString(), CreationCollisionOption.GenerateUniqueName);
                            var downloader = new BackgroundDownloader();
                            var download = downloader.CreateDownload(source, destinationFile);

                            var downloadOperation = await download.StartAsync();

                            file = (StorageFile)downloadOperation.ResultFile;
                            angle.preloadFile = file.Path;
                            angle.isPreloaded = true;
                        }
                    }
                    catch (Exception e) { }
                }
            }
        }

        public void GoBack()
        {
            preloadCTS.Cancel();
            //DeleteTempData();
            dispRequest.RequestRelease();
            dispRequest = null;
            saveAnglePreferences();
            navigationService.GoBack();
        }
    }

    public enum SortType
    {
        Ascending = 0,
        Descending = 1,
        None = 2
    }
}
