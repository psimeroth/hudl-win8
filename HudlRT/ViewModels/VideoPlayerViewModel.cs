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

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private const int INITIAL_LOAD_COUNT = 1;

        private readonly INavigationService navigationService;
        private DisplayRequest dispRequest = null;
        private PlaybackType playbackType;
        public CachedParameter Parameter { get; set; }
        private BindableCollection<Clip> clips;
        public BindableCollection<Clip> Clips
        {
            get { return clips; }
            set
            {
                clips = value;
                NotifyOfPropertyChange(() => Clips);
            }
        }
        private Angle selectedAngle;
        public Angle SelectedAngle
        {
            get { return selectedAngle; }
            set
            {
                selectedAngle = value;
                NotifyOfPropertyChange(() => SelectedAngle);
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
        private string cutupName;
        public string CutupName
        {
            get { return cutupName; }
            set
            {
                cutupName = value;
                NotifyOfPropertyChange(() => CutupName);
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

        private int SelectedClipIndex = 0;
        Point initialPoint = new Point();
        Point currentPoint;
        bool isFullScreenGesture = false;
        public ListView listView { get; set; }

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            AppDataAccessor.SetLastViewed(Parameter.selectedCutup.name, DateTime.Now.ToString("g"), Parameter.selectedCutup.cutupId);
            Clips = new BindableCollection<Clip>(Parameter.selectedCutup.clips.Where(u => u.order < INITIAL_LOAD_COUNT).ToList());
            GridHeaders = Parameter.selectedCutup.displayColumns;
            if (Clips.Count > 0)
            {
                GetAngleNames();
                SelectedClip = Clips.First();
                SelectedAngle = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).FirstOrDefault();
            }
            CutupName = Parameter.selectedCutup.name;
            
            if (!AppDataAccessor.PlaybackTypeSet())	
            {
                AppDataAccessor.SetPlaybackType((int)PlaybackType.once);
            }
            playbackType = (PlaybackType)AppDataAccessor.GetPlaybackType();
            setToggleButtonContent();

            dispRequest = new DisplayRequest();
            dispRequest.RequestActive();
        }

        private async void initialClipPreload()
        {
            await DeleteTempData(); //Make sure there are no left over temp files (from app crash, etc)
            PreloadClips(SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList());
            if (Clips.Count > 1)
            {
                PreloadClips(Clips[1].angles.Where(angle => angle.angleType.IsChecked).ToList());
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            AddClipsToGrid(Parameter.selectedCutup.clips.Count);
            initialClipPreload();
        }

        private async Task AddClipsToGrid(int count)
        {
            foreach (Clip clip in new BindableCollection<Clip>(Parameter.selectedCutup.clips.Where(u => u.order >= INITIAL_LOAD_COUNT).ToList()))
            {
                await Task.Run(() => Clips.Add(clip));
            }
        }

        private async Task loopThroughClipsAndAdd(Clip clip)
        {

            //return new Task();
        }


        private void GetAngleNames()
        {
            HashSet<string> types = new HashSet<string>();
            foreach (Clip clip in Parameter.selectedCutup.clips)
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
            foreach (Clip clip in Parameter.selectedCutup.clips)
            {
                foreach (Angle angle in clip.angles)
                {
                    angle.angleType = AngleTypes.Where(angleType => angleType.Name.Equals(angle.angleName)).FirstOrDefault();
                }
            }
 
            getAnglePreferences();
        }

        private void getAnglePreferences()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            long teamID = (long)roamingSettings.Values["hudl-teamID"];
            foreach (AngleType angleName in AngleTypes)
            {
                string angleNameKey = String.Concat(teamID.ToString(), "-", angleName.Name);
                if (roamingSettings.Values[angleNameKey] == null)
                {
                    angleName.IsChecked = true;
                }
                else
                {
                    angleName.IsChecked = (bool)roamingSettings.Values[angleNameKey];
                }
            }
        }

        private void saveAnglePreferences()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            long teamID = (long)roamingSettings.Values["hudl-teamID"];
            foreach (AngleType angleName in AngleTypes)
            {
                roamingSettings.Values[String.Concat(teamID.ToString(), "-", angleName.Name)] = angleName.IsChecked;
            }
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            setClip(clip);
        }

        public void setClip(Clip clip)
        {
            if (clip != null && SelectedClip.clipId != clip.clipId)
            {
                SelectedClip = clip;
                SelectedClipIndex = (int)clip.order;

                listView.SelectedItem = SelectedClip;

                Angle nextAngle = clip.angles.Where(angle => angle.angleType.IsChecked).FirstOrDefault();
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;

                int nextClipIndex = (SelectedClipIndex + 1) % Clips.Count;
                PreloadClips(SelectedClip.angles.Where(angle => angle.angleType.IsChecked && angle.isPreloaded == false).ToList());
                PreloadClips(Clips[nextClipIndex].angles.Where(angle => angle.angleType.IsChecked && angle.isPreloaded == false).ToList());
            }
            else 
            {
                listView.SelectedItem = SelectedClip;
            }
        }

        public void NextClip(NextAngleEvent eventType)
        {
            
            if (SelectedAngle == null)
            {
                goToNextClip();
            }
            else
            {
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                Angle currentAngle = SelectedClip.angles.Where(a => a.clipAngleId == SelectedAngle.clipAngleId).FirstOrDefault();

                int angleIndex = filteredAngles.IndexOf(currentAngle);
                if (angleIndex < filteredAngles.Count - 1)
                {
                    Angle nextAngle = filteredAngles[angleIndex + 1];
                    SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
                }
                else
                {
                    if (eventType == NextAngleEvent.mediaEnded && playbackType == PlaybackType.loop)
                    {
                        if (filteredAngles.Any())
                        {
                            Angle nextAngle = filteredAngles[0];
                            SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
                        }
                        else
                        {
                            SelectedAngle = null;
                        }
                    }
                    else if(eventType == NextAngleEvent.buttonClick || playbackType == PlaybackType.next)
                    {
                        goToNextClip();
                    }
                }
            }
        }

        private void goToNextClip()
        {
            if (Clips.Count > 1)
            {
                SelectedClipIndex = (SelectedClipIndex + 1) % Clips.Count;

                SelectedClip = Clips[SelectedClipIndex];
                listView.SelectedItem = SelectedClip;
                Angle nextAngle = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).FirstOrDefault();
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
                
                int nextClipIndex = (SelectedClipIndex + 1) % Clips.Count;
                PreloadClips(Clips[nextClipIndex].angles.Where(angle => angle.angleType.IsChecked && angle.isPreloaded == false).ToList());
            }
        }

        public void PreviousClip(ItemClickEventArgs eventArgs)
        {
            if (SelectedAngle == null)
            {
                goToPreviousClip();
            }
            else
            {
                List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();
                Angle currentAngle = SelectedClip.angles.Where(a => a.clipAngleId == SelectedAngle.clipAngleId).FirstOrDefault();

                int angleIndex = filteredAngles.IndexOf(currentAngle);
                if (angleIndex > 0)
                {
                    Angle nextAngle = filteredAngles[angleIndex - 1];
                    SelectedAngle = nextAngle.isPreloaded ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
                }
                else
                {
                    goToPreviousClip();
                }
            }
        }

        private void goToPreviousClip()
        {
            if (Clips.Count > 1)
            {
                SelectedClipIndex = (SelectedClipIndex == 0) ? Clips.Count - 1 : SelectedClipIndex - 1;

                SelectedClip = Clips[SelectedClipIndex];
                listView.SelectedItem = SelectedClip;
                Angle nextAngle = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).FirstOrDefault();
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
            }
        }

        public void angleFilter()
        {
            List<Angle> filteredAngles = SelectedClip.angles.Where(angle => angle.angleType.IsChecked).ToList<Angle>();

            int nextClipIndex = (SelectedClipIndex + 1) % Clips.Count;
            PreloadClips(filteredAngles.Where(angle => angle.isPreloaded == false).ToList());
            PreloadClips(Clips[nextClipIndex].angles.Where(angle => angle.angleType.IsChecked && angle.isPreloaded == false).ToList());

            //If the current angle has been filtered out, reset the clip to the first unfiltered angle, or null
            if (SelectedAngle != null)
            {
                if (filteredAngles.Where(angle => angle.clipAngleId == SelectedAngle.clipAngleId).FirstOrDefault() == null)
                {
                    Angle nextAngle = filteredAngles.FirstOrDefault();
                    SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
                }
            }
            else
            {
                Angle nextAngle = filteredAngles.FirstOrDefault();
                SelectedAngle = (nextAngle != null && nextAngle.isPreloaded) ? new Angle(nextAngle.clipAngleId, nextAngle.preloadFile.Path) : nextAngle;
            }
        }

        public void playbackToggle()
        {
            playbackType = (PlaybackType)(((int)playbackType + 1) % Enum.GetNames(typeof(PlaybackType)).Length);
            
            setToggleButtonContent();
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["hudl-playbackType"] = (int)playbackType;
        }

        public void setToggleButtonContent()
        {
            if (playbackType == PlaybackType.once)
            {
                ToggleButtonContent = "Playback: Once";
            }
            else if (playbackType == PlaybackType.loop)
            {
                ToggleButtonContent = "Playback: Loop";
            }
            else
            {
                ToggleButtonContent = "Playback: Next";
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

        private async Task PreloadClips(List<Angle> angles)
        {
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            foreach (Angle angle in angles)
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
                        angle.preloadFile = file;
                        angle.isPreloaded = true;
                    }
                }
                catch (Exception e)
                {

                }
            } 
        }

        private async Task<bool> DeleteTempData()
        {
            var folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);

            foreach (StorageFile file in files)
            {
                try
                {
                    await file.DeleteAsync();
                }
                catch (Exception e)
                {

                }
            }
            return true;
        }

        public void GoBack()
        {
            DeleteTempData();
            dispRequest.RequestRelease();
			dispRequest = null;
            saveAnglePreferences();
            //CachedParameter param;
            //if (Parameter.sectionViewGameSelected == null)
            //{
            //    param = new CachedParameter { categoryId = 0, gameId = 0, seasonsDropDown = Parameter.seasonsDropDown, seasonSelected = Parameter.seasonSelected, sectionViewGames = null };
            //}
            //else
            //{
            //    param = new CachedParameter { categoryId = Parameter.sectionViewCategorySelected.CategoryId, gameId = Parameter.sectionViewGameSelected.GameId, seasonsDropDown = Parameter.seasonsDropDown, seasonSelected = Parameter.seasonSelected, sectionViewGames = Parameter.sectionViewGames };
            //}
            navigationService.NavigateToViewModel<SectionViewModel>(Parameter);
        }
    }
}
