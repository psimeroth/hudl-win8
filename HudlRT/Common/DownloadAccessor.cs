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
using HudlRT.ViewModels;
using System.Threading;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.ComponentModel;

namespace HudlRT.Common
{
    public sealed class DownloadAccessor
    {
        private static readonly Lazy<DownloadAccessor> downloader = new Lazy<DownloadAccessor>(() => new DownloadAccessor());

        public static DownloadAccessor Instance
        {
            get { return downloader.Value; }
        }

        private DownloadAccessor()
        {
            Downloading = false;
            TotalBytes = 0;
            ClipsComplete = 0;
            CurrentDownloadedBytes = 0;
            TotalClips = 0;
        }

        public DownloadOperation Download { get; set; }
        public BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        public double DownloadProgress { get; set; }
        public bool Downloading { get; set; }
        public long TotalBytes { get; set; }
        public long ClipsComplete { get; set; }
        public long CurrentDownloadedBytes { get; set; }
        public long TotalClips { get; set; }

        public async Task<BindableCollection<CutupViewModel>> GetDownloads()
        {
            BindableCollection<CutupViewModel> cutups = new BindableCollection<CutupViewModel>();
            var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Downloads downloads = new Downloads();
            long totalSize = 0;
            foreach (StorageFolder folder in downloadFolders)
            {
                if (folder.Name.Contains(AppDataAccessor.GetUsername()))
                {
                    try
                    {
                        StorageFile model = await folder.GetFileAsync("DownloadsModel");
                        string text = await Windows.Storage.FileIO.ReadTextAsync(model);
                        CutupViewModel cutupVM = JsonConvert.DeserializeObject<CutupViewModel>(text);
                        cutupVM.Width = new GridLength(180);
                        if (cutupVM != null)
                        {
                            totalSize += cutupVM.TotalCutupSize;
                            cutups.Add(cutupVM);
                        }
                    }
                    catch (Exception) { }
                }
            }
            
            //return SortCutupsByDownloadedDate(cutups);
            BindableCollection<CutupViewModel> sortedCutups = new BindableCollection<CutupViewModel>(cutups.OrderByDescending(c => c.downloadedDate));
            CachedParameter.downloadedCutups = sortedCutups;

            CachedParameter.hubViewDownloadsCount = CachedParameter.downloadedCutups.Count != 1 ? CachedParameter.downloadedCutups.Count + " Cutups" : CachedParameter.downloadedCutups.Count + " Cutup";
            long megabytes = (long)Math.Ceiling((totalSize / 1048576.0));
            CachedParameter.hubViewDownloadsSizeInMB = CachedParameter.downloadedCutups.Count > 0 ? megabytes + " MB" : "";
            return sortedCutups;
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
                //should only fail if the folder does not exist - meaning its already deleted
            }
        }

        private async Task<StorageFile> StartDownloadAsync(DownloadOperation downloadOperation)
        {
            try
            {
                Download = downloadOperation;
                await downloadOperation.StartAsync().AsTask(CachedParameter.cts.Token, CachedParameter.progressCallback);
                CurrentDownloadedBytes += (long)downloadOperation.Progress.BytesReceived;
                return (StorageFile)downloadOperation.ResultFile;
            }
            catch (TaskCanceledException)
            {
                Downloading = false;
                CurrentDownloadedBytes = 0;
                foreach (Cutup downloadedCutup in CachedParameter.currentlyDownloadingCutups)
                {
                    RemoveDownload(downloadedCutup);//may not have even started downloading this cutup when this is called
                }
                CachedParameter.currentlyDownloadingCutups = new List<Cutup>();
                return null;
            }
        }

        public async Task DownloadCutups(List<Cutup> cutups, Season s, GameViewModel g)
        {

            backgroundDownloader = new BackgroundDownloader();
            Downloading = true;
            TotalBytes = 0;
            ClipsComplete = 0;
            CurrentDownloadedBytes = 0;
            TotalClips = 0;
            long cutupTotalSize = 0;
            var httpClient = new System.Net.Http.HttpClient();
            foreach (Cutup cut in cutups)
            {
                cutupTotalSize = 0;
                if (cut.clipCount != cut.clips.Count)
                {
                    List<Clip> remainingClipsList = await ServiceAccessor.GetAdditionalCutupClips(cut.cutupId, cut.clips.Count);
                    foreach (Clip c in remainingClipsList)
                    {
                        cut.clips.Add(c);
                    }
                }
                foreach (Clip c in cut.clips)
                {
                    foreach (Angle angle in c.angles)
                    {
                        Uri uri = new Uri(angle.fileLocation);
                        var httpRequestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, uri);
                        var response = await httpClient.SendAsync(httpRequestMessage);
                        var angleSize = response.Content.Headers.ContentLength;
                        cutupTotalSize += (long)angleSize;
                        TotalBytes += (long)angleSize;
                        TotalClips++;
                    }
                }
                cut.totalFilesSize = cutupTotalSize;
            }

            foreach (Cutup cut in cutups)
            {
                var fileFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDataAccessor.GetUsername() + cut.cutupId.ToString(), Windows.Storage.CreationCollisionOption.OpenIfExists);

                //save thumbnail
                var sourceThumb = new Uri(cut.thumbnailLocation);
                var destinationFileThumb = await fileFolder.CreateFileAsync("Thumbnail.jpg", CreationCollisionOption.ReplaceExisting);
                var downloaderThumb = new BackgroundDownloader();
                var downloadThumb = downloaderThumb.CreateDownload(sourceThumb, destinationFileThumb);
                var downloadOperationThumb = await downloadThumb.StartAsync();
                var fileThumb = (StorageFile)downloadOperationThumb.ResultFile;
                cut.thumbnailLocation = fileThumb.Path.Replace("\\","/");

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
                                Download = backgroundDownloader.CreateDownload(source, destinationFile);
                                file = await StartDownloadAsync(Download);
                                angle.preloadFile = file.Path;
                                angle.isPreloaded = true;
                                ClipsComplete++;
                            }
                        }
                        catch (Exception e)
                        {
                            RemoveDownload(cut);
                            Downloading = false;
                            return;
                        }
                    }
                }
                CutupViewModel cutupForSave = CutupViewModel.FromCutup(cut);
                cutupForSave.downloadedDate = DateTime.Now;
                cutupForSave.GameInfo = g.Date + " - " + g.Opponent + ": ";
                string updatedModel = JsonConvert.SerializeObject(cutupForSave);
                await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
                CachedParameter.downloadedCutups.Add(cutupForSave);
                
            }
            DownloadComplete_Notification();
            Downloading = false;
            CurrentDownloadedBytes = 0;
        }

        private void DownloadComplete_Notification()
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var elements = toastXml.GetElementsByTagName("text");
            elements[0].AppendChild(toastXml.CreateTextNode("Download Complete"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
