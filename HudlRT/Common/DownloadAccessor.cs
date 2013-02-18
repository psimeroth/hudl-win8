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
        public static List<Playlist> currentlyDownloadingPlaylists { get; set; }

        public static Progress<DownloadOperation> progressCallback { get; set; }

        public static BindableCollection<Playlist> downloadedPlaylists { get; set; }

        public static CancellationTokenSource cts = new CancellationTokenSource();
        
        
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

        public async Task<BindableCollection<Playlist>> GetDownloads()
        {
            BindableCollection<Playlist> playlists = new BindableCollection<Playlist>();
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
                        Playlist playlist = JsonConvert.DeserializeObject<Playlist>(text);
                        if (playlist != null)
                        {
                            totalSize += playlist.totalFilesSize;
                            playlists.Add(playlist);
                        }
                    }
                    catch (Exception) { }
                }
            }
            BindableCollection<Playlist> sortedPlaylists = new BindableCollection<Playlist>(playlists.OrderByDescending(c => c.downloadedDate));
            downloadedPlaylists = sortedPlaylists;
            return sortedPlaylists;
        }

        private async Task RemoveDownload(Playlist playlist)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + playlist.playlistId.ToString());
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
                await downloadOperation.StartAsync().AsTask(cts.Token, progressCallback);
                CurrentDownloadedBytes += (long)downloadOperation.Progress.BytesReceived;
                return (StorageFile)downloadOperation.ResultFile;
            }
            catch (TaskCanceledException)
            {
                Downloading = false;
                CurrentDownloadedBytes = 0;
                foreach (Playlist downloadedPlaylist in currentlyDownloadingPlaylists)
                {
                    RemoveDownload(downloadedPlaylist);//may not have even started downloading this playlist when this is called
                }
                currentlyDownloadingPlaylists = new List<Playlist>();
                return null;
            }
        }

        public async Task DownloadPlaylists(List<Playlist> playlists)
        {
            currentlyDownloadingPlaylists = playlists;
            backgroundDownloader = new BackgroundDownloader();
            Downloading = true;
            TotalBytes = 0;
            ClipsComplete = 0;
            CurrentDownloadedBytes = 0;
            TotalClips = 0;
            long playlistTotalSize = 0;
            var httpClient = new System.Net.Http.HttpClient();
            foreach (Playlist cut in playlists)
            {
                playlistTotalSize = 0;
                foreach (Clip c in cut.clips)
                {
                    foreach (Angle angle in c.angles)
                    {
                        Uri uri = new Uri(angle.fileLocation);
                        var httpRequestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, uri);
                        var response = await httpClient.SendAsync(httpRequestMessage);
                        var angleSize = response.Content.Headers.ContentLength;
                        playlistTotalSize += (long)angleSize;
                        TotalBytes += (long)angleSize;
                        TotalClips++;
                    }
                }
                cut.totalFilesSize = playlistTotalSize;
            }

            foreach (Playlist pl in playlists)
            {
                var fileFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDataAccessor.GetUsername() + pl.playlistId.ToString(), Windows.Storage.CreationCollisionOption.OpenIfExists);

                //save thumbnail
                var sourceThumb = new Uri(pl.thumbnailLocation);
                var destinationFileThumb = await fileFolder.CreateFileAsync("Thumbnail.jpg", CreationCollisionOption.ReplaceExisting);
                var downloaderThumb = new BackgroundDownloader();
                var downloadThumb = downloaderThumb.CreateDownload(sourceThumb, destinationFileThumb);
                var downloadOperationThumb = await downloadThumb.StartAsync();
                var fileThumb = (StorageFile)downloadOperationThumb.ResultFile;
                pl.thumbnailLocation = fileThumb.Path.Replace("\\", "/");

                StorageFile downloadModel = await fileFolder.CreateFileAsync("DownloadsModel", Windows.Storage.CreationCollisionOption.OpenIfExists);
                foreach (Clip c in pl.clips)
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
                                //PlaylistId-ClipId-ClipAngleId
                                var destinationFile = await fileFolder.CreateFileAsync(pl.playlistId + "-" + c.clipId + "-" + angle.clipAngleId, CreationCollisionOption.ReplaceExisting);
                                Download = backgroundDownloader.CreateDownload(source, destinationFile);
                                file = await StartDownloadAsync(Download);
                                angle.preloadFile = file.Path;
                                angle.isPreloaded = true;
                                ClipsComplete++;
                            }
                        }
                        catch (Exception e)
                        {
                            RemoveDownload(pl);
                            return;
                        }
                    }
                }
                pl.downloadedDate = DateTime.Now;
                string updatedModel = JsonConvert.SerializeObject(pl);
                await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
                downloadedPlaylists.Add(pl);

            }
            DownloadComplete_Notification();
            Downloading = false;
            CurrentDownloadedBytes = 0;
            currentlyDownloadingPlaylists = new List<Playlist>();
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
