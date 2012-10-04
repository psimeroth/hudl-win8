﻿using Caliburn.Micro;
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

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
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
        private Angle video;
        public Angle Video
        {
            get { return video; }
            set
            {
                video = value;
                NotifyOfPropertyChange(() => Video);
            }
        }

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            GetClipsByCutup(Parameter.Value);
        }

        public async void GetClipsByCutup(Cutup cutup)
        {
            var clips = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CLIPS.Replace("#", cutup.cutupId.ToString()));
            if (!clips.Equals(""))
            {
                cutup.clips = new BindableCollection<Clip>();
                var obj = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
                foreach (ClipDTO clipDTO in obj.ClipsList.Clips)
                {
                    Clip c = Clip.FromDTO(clipDTO);
                    if (c != null)
                    {
                        cutup.clips.Add(c);
                    }
                }
                Clips = cutup.clips;
            }
            else
            {
                
            }
            if (Clips.Count > 0)
            {
                Video = Clips.First().angles.ElementAt(0);
            }
            //(if Clips.count == 0) .. do something figure this out earlier somehow?
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            Video = clip.angles.ElementAt(0);
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
