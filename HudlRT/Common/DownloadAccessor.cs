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
        //private static readonly Lazy<DownloadAccessor> downloader = new Lazy<DownloadAccessor>(() => new DownloadAccessor());

        //public static DownloadAccessor Instance
        //{
        //    get { return downloader.Value; }
        //}

        //private DownloadAccessor()
        //{
        //    Downloading = false;
        //    TotalBytes = 0;
        //    ClipsComplete = 0;
        //    CurrentDownloadedBytes = 0;
        //    TotalClips = 0;
        //}

        //public DownloadOperation Download { get; set; }
        //public BackgroundDownloader backgroundDownloader = new BackgroundDownloader();
        //public double DownloadProgress { get; set; }
        //public bool Downloading { get; set; }
        //public long TotalBytes { get; set; }
        //public long ClipsComplete { get; set; }
        //public long CurrentDownloadedBytes { get; set; }
        //public long TotalClips { get; set; }

        //public async Task<BindableCollection<PlaylistViewModel>> GetDownloads()
        //{
        //    BindableCollection<PlaylistViewModel> playlists = new BindableCollection<PlaylistViewModel>();
        //    var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
        //    Downloads downloads = new Downloads();
        //    long totalSize = 0;
        //    foreach (StorageFolder folder in downloadFolders)
        //    {
        //        if (folder.Name.Contains(AppDataAccessor.GetUsername()))
        //        {
        //            try
        //            {
        //                StorageFile model = await folder.GetFileAsync("DownloadsModel");
        //                string text = await Windows.Storage.FileIO.ReadTextAsync(model);
        //                PlaylistViewModel playlistVM = JsonConvert.DeserializeObject<PlaylistViewModel>(text);
        //                playlistVM.Width = new GridLength(180);
        //                if (playlistVM != null)
        //                {
        //                    totalSize += playlistVM.TotalPlaylistSize;
        //                    playlists.Add(playlistVM);
        //                }
        //            }
        //            catch (Exception) { }
        //        }
        //    }
            
        //    //return SortPlaylistsByDownloadedDate(playlists);
        //    BindableCollection<PlaylistViewModel> sortedPlaylists = new BindableCollection<PlaylistViewModel>(playlists.OrderByDescending(c => c.downloadedDate));
        //    CachedParameter.downloadedPlaylists = sortedPlaylists;

        //    CachedParameter.hubViewDownloadsCount = CachedParameter.downloadedPlaylists.Count != 1 ? CachedParameter.downloadedPlaylists.Count + " Playlists" : CachedParameter.downloadedPlaylists.Count + " Playlist";
        //    long megabytes = (long)Math.Ceiling((totalSize / 1048576.0));
        //    CachedParameter.hubViewDownloadsSizeInMB = CachedParameter.downloadedPlaylists.Count > 0 ? megabytes + " MB" : "";
        //    return sortedPlaylists;
        //}

        //private async Task RemoveDownload(Playlist playlist)
        //{
        //    try
        //    {
        //        var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + playlist.playlistId.ToString());
        //        folder.DeleteAsync();
        //    }
        //    catch (Exception)
        //    {
        //        //should only fail if the folder does not exist - meaning its already deleted
        //    }
        //}

        //private async Task<StorageFile> StartDownloadAsync(DownloadOperation downloadOperation)
        //{
        //    try
        //    {
        //        Download = downloadOperation;
        //        await downloadOperation.StartAsync().AsTask(CachedParameter.cts.Token, CachedParameter.progressCallback);
        //        CurrentDownloadedBytes += (long)downloadOperation.Progress.BytesReceived;
        //        return (StorageFile)downloadOperation.ResultFile;
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        Downloading = false;
        //        CurrentDownloadedBytes = 0;
        //        foreach (Playlist downloadedPlaylist in CachedParameter.currentlyDownloadingPlaylists)
        //        {
        //            RemoveDownload(downloadedPlaylist);//may not have even started downloading this playlist when this is called
        //        }
        //        CachedParameter.currentlyDownloadingPlaylists = new List<Playlist>();
        //        return null;
        //    }
        //}

        //public async Task DownloadPlaylists(List<Playlist> playlists, Season s, GameViewModel g)
        //{

        //    backgroundDownloader = new BackgroundDownloader();
        //    Downloading = true;
        //    TotalBytes = 0;
        //    ClipsComplete = 0;
        //    CurrentDownloadedBytes = 0;
        //    TotalClips = 0;
        //    long playlistTotalSize = 0;
        //    var httpClient = new System.Net.Http.HttpClient();
        //    foreach (Playlist cut in playlists)
        //    {
        //        playlistTotalSize = 0;
        //        foreach (Clip c in cut.clips)
        //        {
        //            foreach (Angle angle in c.angles)
        //            {
        //                Uri uri = new Uri(angle.fileLocation);
        //                var httpRequestMessage = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, uri);
        //                var response = await httpClient.SendAsync(httpRequestMessage);
        //                var angleSize = response.Content.Headers.ContentLength;
        //                playlistTotalSize += (long)angleSize;
        //                TotalBytes += (long)angleSize;
        //                TotalClips++;
        //            }
        //        }
        //        cut.totalFilesSize = playlistTotalSize;
        //    }

        //    foreach (Playlist cut in playlists)
        //    {
        //        var fileFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDataAccessor.GetUsername() + cut.playlistId.ToString(), Windows.Storage.CreationCollisionOption.OpenIfExists);

        //        //save thumbnail
        //        var sourceThumb = new Uri(cut.thumbnailLocation);
        //        var destinationFileThumb = await fileFolder.CreateFileAsync("Thumbnail.jpg", CreationCollisionOption.ReplaceExisting);
        //        var downloaderThumb = new BackgroundDownloader();
        //        var downloadThumb = downloaderThumb.CreateDownload(sourceThumb, destinationFileThumb);
        //        var downloadOperationThumb = await downloadThumb.StartAsync();
        //        var fileThumb = (StorageFile)downloadOperationThumb.ResultFile;
        //        cut.thumbnailLocation = fileThumb.Path.Replace("\\","/");

        //        StorageFile downloadModel = await fileFolder.CreateFileAsync("DownloadsModel", Windows.Storage.CreationCollisionOption.OpenIfExists);
        //        foreach (Clip c in cut.clips)
        //        {
        //            foreach (Angle angle in c.angles)
        //            {
        //                try
        //                {
        //                    var source = new Uri(angle.fileLocation);
        //                    var files = await fileFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
        //                    var file = files.FirstOrDefault(x => x.Name.Equals(angle.clipAngleId.ToString()));

        //                    if (file == null)
        //                    {
        //                        //PlaylistId-ClipId-ClipAngleId
        //                        var destinationFile = await fileFolder.CreateFileAsync(cut.playlistId + "-" + c.clipId + "-" + angle.clipAngleId, CreationCollisionOption.ReplaceExisting);
        //                        Download = backgroundDownloader.CreateDownload(source, destinationFile);
        //                        file = await StartDownloadAsync(Download);
        //                        angle.preloadFile = file.Path;
        //                        angle.isPreloaded = true;
        //                        ClipsComplete++;
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    RemoveDownload(cut);
        //                    return;
        //                }
        //            }
        //        }
        //        PlaylistViewModel playlistForSave = PlaylistViewModel.FromPlaylist(cut);
        //        playlistForSave.downloadedDate = DateTime.Now;
        //        playlistForSave.GameInfo = g.Date + " - " + g.Opponent + ": ";
        //        string updatedModel = JsonConvert.SerializeObject(playlistForSave);
        //        await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
        //        CachedParameter.downloadedPlaylists.Add(playlistForSave);
                
        //    }
        //    DownloadComplete_Notification();
        //    Downloading = false;
        //    CurrentDownloadedBytes = 0;
        //}

        //private void DownloadComplete_Notification()
        //{
        //    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
        //    var elements = toastXml.GetElementsByTagName("text");
        //    elements[0].AppendChild(toastXml.CreateTextNode("Download Complete"));
        //    var toast = new ToastNotification(toastXml);
        //    ToastNotificationManager.CreateToastNotifier().Show(toast);
        //}
    }
}
