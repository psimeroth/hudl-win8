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

namespace HudlRT.Common
{
    public class DownloadAccessor : PropertyChangedBase
    {

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

        public Boolean downloadComplete = false;
        public Boolean downloading = false;

        public async Task DownloadCutups(List<Cutup> cutups)
        {
            downloadComplete = false;
            downloading = true;
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
                            RemoveDownload(cut);
                            return;
                        }
                    }
                }
                string updatedModel = JsonConvert.SerializeObject(cut);
                await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
                downloadComplete = true;
                downloading = false;
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
    }
}
