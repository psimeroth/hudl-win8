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
        private Boolean deleting = false;

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

        private Visibility progress_Visibility;
        public Visibility Progress_Visibility
        {
            get { return progress_Visibility; }
            set
            {
                progress_Visibility = value;
                NotifyOfPropertyChange(() => Progress_Visibility);
            }
        }

        private String download_Contents;
        public String Download_Contents
        {
            get { return download_Contents; }
            set
            {
                download_Contents = value;
                NotifyOfPropertyChange(() => Download_Contents);
            }
        }

        public DownloadsViewModel(INavigationService navigationService): base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            Progress_Visibility = Visibility.Collapsed;
            await GetDownloads();
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            if (!deleting)
            {     
                CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name };
                CachedParameter.sectionViewCutupSelected = cutup;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>();
            }
            else
            {
                cutup.CheckBox = !cutup.CheckBox;
                CheckBoxSelected();
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

        public void GoBack()
        {
            navigationService.GoBack();
        }

        public void Delete_Playlists()
        {
            deleting = true;
            DeleteButton_Visibility = Visibility.Collapsed;
            CancelButton_Visibility = Visibility.Visible;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupVM.CheckBox_Visibility = Visibility.Visible;
                if (cutupVM.CheckBox)
                {
                    ConfirmButton_Visibility = Visibility.Visible;
                }
            }
        }

        public void Cancel_Delete()
        {
            deleting = false; ;
            DeleteButton_Visibility = Visibility.Visible;
            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                cutupVM.CheckBox = false;
            }
        }

        public async void Confirm_Delete()
        {
            CutupViewModel[] copy = new CutupViewModel[Cutups.Count];
            Cutups.CopyTo(copy, 0);
            foreach (CutupViewModel cutupVM in copy)
            {
                if (cutupVM.CheckBox)
                {
                    await RemoveDownload(cutupVM);
                    Cutups.Remove(cutupVM);
                }
            }

            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            var totalClips = 0;
            if (!Cutups.Any())
            {
                DeleteButton_Visibility = Visibility.Collapsed;
            }
            else
            {
                DeleteButton_Visibility = Visibility.Visible;
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                    totalClips += cutupVM.ClipCount;
                }
            }
            Download_Contents = "Cutups: " + Cutups.Count + " | Clips: " + totalClips + " | Size: " + "cutup size";
        }


        private async Task GetDownloads()
        {
            Cutups = new BindableCollection<CutupViewModel>();
            var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Downloads downloads = new Downloads();
            var totalClips = 0;
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
                    totalClips += cutupVM.ClipCount;
                }
            }
            if (!Cutups.Any())
            {
                DeleteButton_Visibility = Visibility.Collapsed;
            }
            Download_Contents = "Cutups: " + Cutups.Count + " | Clips: " + totalClips + " | Size: " + "cutup size";
        }

        private async Task RemoveDownload(CutupViewModel cutup)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + cutup.CutupId.ToString());
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
