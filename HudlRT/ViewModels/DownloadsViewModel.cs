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

namespace HudlRT.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
       

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

        private Visibility button_Visibility;
        public Visibility Button_Visibility
        {
            get { return button_Visibility; }
            set
            {
                button_Visibility = value;
                NotifyOfPropertyChange(() => Button_Visibility);
            }
        }

        public DownloadsViewModel(INavigationService navigationService): base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            Button_Visibility = Visibility.Collapsed;
            await GetDownloads();
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name };
            CachedParameter.sectionViewCutupSelected = cutup;
            navigationService.NavigateToViewModel<VideoPlayerViewModel>();
            //await GetClipsByCutup(cutup);
        }

        public void GoBack()
        {
            navigationService.GoBack();
        }

        public void Delete_Playlists()
        {
            Button_Visibility = Visibility.Visible;
        }

        public async Task GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                string[] clipCount = cutup.ClipCount.ToString().Split(' ');
                //UpdateCachedParameter();
                CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = Int32.Parse(clipCount[0]), name = cutup.Name };
                CachedParameter.sectionViewCutupSelected = cutup;
                //Parameter.videoPageClips = Parameter.selectedCutup.clips;

                //await GetDownloads();
                //await DownloadCutups(new List<Cutup>{Parameter.selectedCutup});
                //await RemoveDownload(Parameter.selectedCutup);
                navigationService.NavigateToViewModel<VideoPlayerViewModel>();
            }
            else
            {
                //Common.APIExceptionDialog.ShowExceptionDialog(null, null);
            }
        }

        private async Task GetDownloads()
        {
            Cutups = new BindableCollection<CutupViewModel>();
            var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Downloads downloads = new Downloads();
            foreach (StorageFolder folder in downloadFolders)
            {
                if (folder.Name.Contains(AppDataAccessor.GetUsername()))
                {
                    StorageFile model = await folder.GetFileAsync("DownloadsModel");
                    string text = await Windows.Storage.FileIO.ReadTextAsync(model);
                    Cutup savedCutup = JsonConvert.DeserializeObject<Cutup>(text);
                    CutupViewModel cutupVM = CutupViewModel.FromCutup(savedCutup);
                    cutupVM.Clips = savedCutup.clips;
                    cutupVM.DisplayColumns = savedCutup.displayColumns;
                    Cutups.Add(cutupVM);
                }
            }
        }

        private async Task RemoveDownload(Cutup cutup)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + cutup.cutupId.ToString());
                folder.DeleteAsync();
            }
            catch (Exception)
            {

            }
        }

        private async Task DownloadCutups(List<Cutup> cutups)
        {

            long totalSize = 0;
            long currentDownloadedBytes = 0;
            foreach (Cutup cut in cutups)
            {
                foreach (Clip c in cut.clips)
                {
                    foreach (Angle angle in c.angles)
                    {
                        var httpClient = new System.Net.Http.HttpClient();
                        Uri uri = new Uri(angle.fileLocation);
                        var httpRequestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, uri);
                        var response = await httpClient.SendAsync(httpRequestMessage);
                        var angleSize = response.Content.Headers.ContentLength;
                        totalSize += (long)angleSize;
                    }
                }
            }

            foreach (Cutup cut in cutups)
            {
                var fileFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDataAccessor.GetUsername() + cut.cutupId.ToString(), Windows.Storage.CreationCollisionOption.OpenIfExists);

                StorageFile downloadModel = await fileFolder.CreateFileAsync("DownloadsModel", Windows.Storage.CreationCollisionOption.OpenIfExists);
                foreach (Clip c in cut.clips)
                {
                    foreach (Angle angle in c.angles)
                    {
                        try
                        {
                            var source = new Uri(angle.fileLocation);
                            var files = await fileFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
                            var file = files.FirstOrDefault(x => x.Name.Equals(angle.clipAngleId.ToString()));

                            if (file == null)
                            {
                                //CutupId-ClipId-ClipAngleId
                                var destinationFile = await fileFolder.CreateFileAsync(cut.cutupId + "-" + c.clipId + "-" + angle.clipAngleId, CreationCollisionOption.ReplaceExisting);
                                var downloader = new BackgroundDownloader();
                                var download = downloader.CreateDownload(source, destinationFile);
                                var downloadOperation = await download.StartAsync();
                                //DownloadProgress = (download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * (100.0 / totalFiles) + (count * (100.0 / totalFiles));
                                DownloadProgress = 100 * (((long)download.Progress.BytesReceived + currentDownloadedBytes) / (double)totalSize);
                                file = (StorageFile)downloadOperation.ResultFile;
                                angle.preloadFile = file.Path;
                                angle.isPreloaded = true;
                                currentDownloadedBytes += (long)download.Progress.BytesReceived;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
                string updatedModel = JsonConvert.SerializeObject(cut);
                await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
            }

        }

        
    }

}
