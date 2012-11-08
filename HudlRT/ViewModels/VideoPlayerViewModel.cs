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

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private int playbackType;
        public PagePassParameter Parameter { get; set; }
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

        private int index = 0;
        Point initialPoint = new Point();
        Point currentPoint;
        bool isFullScreenGesture = false;

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Clips = Parameter.selectedCutup.clips;
            GridHeaders = Parameter.selectedCutup.displayColumns;
            if (Clips.Count > 0)
            {
                SelectedClip = Clips.First();
                SelectedAngle = SelectedClip.angles.ElementAt(0);
            }
            CutupName = Parameter.selectedCutup.name;
            
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["hudl-playbackType"] == null)	
            {
                roamingSettings.Values["hudl-playbackType"] = 0;	
            }
            playbackType = (int)roamingSettings.Values["hudl-playbackType"];
            setToggleButtonContent();
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            SelectedClip = clip;
            SelectedAngle = clip.angles.ElementAt(0);
            index = (int)clip.order;
        }

        public void NextClip(int eventArgs)
        {
            int angleIndex = SelectedClip.angles.IndexOf(selectedAngle);
            if (angleIndex < SelectedClip.angles.Count() - 1)
            {
                SelectedAngle = SelectedClip.angles.ElementAt(angleIndex + 1);
            }
            else
            {
                if (eventArgs == 1 && playbackType == 0)
                {
                    //Do nothing
                }
                else if (eventArgs == 1 && playbackType == 1)
                {
                    SelectedAngle = SelectedClip.angles.ElementAt(0);
                }
                else if (Clips.Count > 1)
                {
                    if (index == (Clips.Count - 1))
                    {
                        SelectedClip = Clips.First();
                        index = 0;
                    }
                    else
                        SelectedClip = Clips.ElementAt(++index);
                    SelectedAngle = SelectedClip.angles.ElementAt(0);
                }
            }
        }

        public void PreviousClip(ItemClickEventArgs eventArgs)
        {
            int angleIndex = SelectedClip.angles.IndexOf(selectedAngle);
            if (angleIndex > 0)
            {
                SelectedAngle = SelectedClip.angles.ElementAt(angleIndex - 1);
            }
            else
            {
                if (Clips.Count > 1)
                {
                    if (index == 0)
                    {
                        SelectedClip = Clips.Last();
                        index = Clips.Count - 1;
                    }
                    else
                        SelectedClip = Clips.ElementAt(--index);
                    SelectedAngle = SelectedClip.angles.ElementAt(0);
                }
            }
        }

        public void playbackToggle()
        {
            playbackType++;
            if (playbackType == 3)
            {
                playbackType = 0;
            }
            setToggleButtonContent();
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["hudl-playbackType"] = playbackType;
        }

        public void setToggleButtonContent()
        {
            if (playbackType == 0)
                ToggleButtonContent = "Playback: Once";
            else if (playbackType == 1)
                ToggleButtonContent = "Playback: Loop";
            else
                ToggleButtonContent = "Playback: Next";
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
                NextClip(0);
            }

            else if (initialPoint.X - currentPoint.X <= -50 && !isFullScreenGesture)
            {
                PreviousClip(null);
            }
        }

        async void save_myFile(string uri)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                Uri source = new Uri(uri);
                string destination = uri.Substring(uri.LastIndexOf('/')+1, uri.IndexOf('?') - uri.LastIndexOf('/')-1);

                StorageFile destinationFile = await localFolder.CreateFileAsync(destination, CreationCollisionOption.GenerateUniqueName);

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);

                // Attach progress and completion handlers.
                HandleDownloadAsync(download, true);
            }
            catch (Exception)
            {
            }

        }

        private async void HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                // Store the download so we can pause/resume.

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>();
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(progressCallback);
                }

                ResponseInformation response = download.GetResponseInformation();
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception)
            {
            }
        }

        public void GoBack()
        {
            navigationService.NavigateToViewModel<HubViewModel>(Parameter);
        }
    }
}
