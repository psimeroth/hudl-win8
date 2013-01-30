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

namespace HudlRT.Common
{
    public class DownloadAccessor
    {

        public double DownloadProgress;

        public bool downloadComplete = false;
        public bool downloading = false;
        public bool downloadCanceled = false;
        public bool findingFileSize = false;
        public DownloadOperation download;
        public long totalBytes = 0;
        public long clipsComplete = 0;
        public long currentDownloadedBytes = 0;
        public long totalClips = 0;

        public async Task<BindableCollection<CutupViewModel>> GetDownloads()
        {
            BindableCollection<CutupViewModel> cutups = new BindableCollection<CutupViewModel>();
            var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Downloads downloads = new Downloads();
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
                            cutups.Add(cutupVM);
                        }
                    }
                    catch (Exception) { }
                }
            }
            
            //return SortCutupsByDownloadedDate(cutups);
            BindableCollection<CutupViewModel> sortedCutups = new BindableCollection<CutupViewModel>(cutups.OrderByDescending(c => c.downloadedDate));
            CachedParameter.downloadedCutups = sortedCutups;
            return sortedCutups;
        }

        private async Task RemoveDownload(Cutup cutup)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + cutup.cutupId.ToString());
                try
                {
                    folder.DeleteAsync();
                }
                catch (Exception) { }
            }
            catch (Exception)
            {

            }
        }

        public async Task DownloadCutups(List<Cutup> cutups, Season s, GameViewModel g, CancellationToken ct)
        {
            downloadComplete = false;
            downloading = true;
            downloadCanceled = false;
            totalBytes = 0;
            clipsComplete = 0;
            currentDownloadedBytes = 0;
            totalClips = 0;

            findingFileSize = true;
            long cutupTotalSize = 0;
            var httpClient = new System.Net.Http.HttpClient();
            foreach (Cutup cut in cutups)
            {
                cutupTotalSize = 0;
                foreach (Clip c in cut.clips)
                {
                    foreach (Angle angle in c.angles)
                    {
                        Uri uri = new Uri(angle.fileLocation);
                        var httpRequestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, uri);
                        var response = await httpClient.SendAsync(httpRequestMessage);
                        var angleSize = response.Content.Headers.ContentLength;
                        cutupTotalSize += (long)angleSize;
                        totalBytes += (long)angleSize;
                        totalClips++;
                    }
                }
                cut.totalFilesSize = cutupTotalSize;
            }
            findingFileSize = false;

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
                            if (ct.IsCancellationRequested)
                            {
                                await CachedParameter.downloadAccessor.RemoveDownload(cut);
                                downloadComplete = false;
                                downloading = false;
                                downloadCanceled = true;
                                currentDownloadedBytes = 0;
                                return;
                            }
                            var source = new Uri(angle.fileLocation);
                            var files = await fileFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
                            var file = files.FirstOrDefault(x => x.Name.Equals(angle.clipAngleId.ToString()));

                            if (file == null)
                            {
                                //CutupId-ClipId-ClipAngleId
                                var destinationFile = await fileFolder.CreateFileAsync(cut.cutupId + "-" + c.clipId + "-" + angle.clipAngleId, CreationCollisionOption.ReplaceExisting);
                                var downloader = new BackgroundDownloader();
                                download = downloader.CreateDownload(source, destinationFile);
                                var downloadOperation = await download.StartAsync();
                                file = (StorageFile)downloadOperation.ResultFile;
                                angle.preloadFile = file.Path;
                                angle.isPreloaded = true;
                                currentDownloadedBytes += (long)download.Progress.BytesReceived;
                                clipsComplete++;
                            }
                        }
                        catch (Exception e)
                        {
                            RemoveDownload(cut);
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
            downloadComplete = true;
            DownloadComplete_Notification();
            downloading = false;
            currentDownloadedBytes = 0;
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
