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
        public List<Playlist> currentlyDownloadingPlaylists { get; set; }

        public Progress<DownloadOperation> progressCallback { get; set; }

        public BindableCollection<Playlist> downloadedPlaylists { get; set; }

        public CancellationTokenSource cts = new CancellationTokenSource();
        
        
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
            long totalSize = 0;
            var userFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername());
            if(userFolder != null)
            {
                var playlistFolders = await userFolder.GetFoldersAsync();
                foreach (StorageFolder pFolder in playlistFolders)
                {
                    try
                    {
                        StorageFile model = await pFolder.GetFileAsync("DownloadsModel");
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
            downloadedPlaylists = playlists;
            return playlists;
        }

        public async Task<BindableCollection<Season>> GetDownloadsModel()
        {
            BindableCollection<Season> seasons = new BindableCollection<Season>();
            try
            {
                StorageFile model = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("CompleteModel");
                string text = await Windows.Storage.FileIO.ReadTextAsync(model);
                seasons = JsonConvert.DeserializeObject<BindableCollection<Season>>(text);
            }
            catch (Exception) { }
            return seasons;
        }

        public async Task RemoveDownload(Playlist playlist)
        {
            try
            {
                playlist.downloadedThumbnailLocation = null;
                foreach (Clip c in playlist.clips)
                {
                    foreach (Angle a in c.angles)
                    {
                        a.isPreloaded = false;
                    }
                }
                var userFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername());
                var playlistFolder = await userFolder.GetFolderAsync(playlist.playlistId);
                await playlistFolder.DeleteAsync();
                Playlist dlPlaylist = downloadedPlaylists.Where(u => u.playlistId == playlist.playlistId).FirstOrDefault();
                if(dlPlaylist != null)
                {
                    downloadedPlaylists.Remove(dlPlaylist);
                }
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

        public async Task DownloadPlaylists(List<Playlist> playlists, Season seasonAndGame)//list of playlists,  season(with game)
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
                var userFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(AppDataAccessor.GetUsername(), Windows.Storage.CreationCollisionOption.OpenIfExists);
                var fileFolder = await userFolder.CreateFolderAsync(pl.playlistId, Windows.Storage.CreationCollisionOption.OpenIfExists);

                //save thumbnail
                var sourceThumb = new Uri(pl.thumbnailLocation);
                var destinationFileThumb = await fileFolder.CreateFileAsync("Thumbnail.jpg", CreationCollisionOption.ReplaceExisting);
                var downloaderThumb = new BackgroundDownloader();
                var downloadThumb = downloaderThumb.CreateDownload(sourceThumb, destinationFileThumb);
                var downloadOperationThumb = await downloadThumb.StartAsync();
                var fileThumb = (StorageFile)downloadOperationThumb.ResultFile;
                pl.downloadedThumbnailLocation = fileThumb.Path.Replace("\\", "/");
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
                StorageFile downloadModel = await fileFolder.CreateFileAsync("DownloadsModel", Windows.Storage.CreationCollisionOption.OpenIfExists);
                pl.downloadedDate = DateTime.Now;
                string updatedModel = JsonConvert.SerializeObject(pl);
                await Windows.Storage.FileIO.WriteTextAsync(downloadModel, updatedModel);
                downloadedPlaylists.Add(pl);

            }
            Game selectedGame = seasonAndGame.games.FirstOrDefault();
            Game newGameWithOnlyDownloads = new Game { date = selectedGame.date, isHome = selectedGame.isHome, gameId = selectedGame.gameId, opponent = selectedGame.opponent, categories = new BindableCollection<Category>() }; 
            foreach (Category c in selectedGame.categories)
            {
                foreach (Playlist plFromSelectedGame in c.playlists)
                {
                    foreach (Playlist plFromParameter in playlists)
                    {
                        if (plFromSelectedGame.playlistId == plFromParameter.playlistId)
                        {
                            Category foundCat = newGameWithOnlyDownloads.categories.Where(u => u.categoryId == c.categoryId).FirstOrDefault();
                            if (foundCat == null)
                            {
                                newGameWithOnlyDownloads.categories.Add(new Category { categoryId = c.categoryId, name = c.name, playlists = c.playlists });
                            }
                            break;
                        }
                    }
                }
            }

            foreach (Category c in newGameWithOnlyDownloads.categories)
            {
                BindableCollection<Playlist> newPlaylists = new BindableCollection<Playlist>();
                foreach (Playlist pl in c.playlists)
                {
                    bool found = playlists.Any(u => u.playlistId == pl.playlistId);
                    if (found)
                    {
                        newPlaylists.Add(pl);
                    }
                }
                c.playlists = newPlaylists;
            }
            seasonAndGame.games[0] = newGameWithOnlyDownloads;//model to be saved
            BindableCollection<Season> modelToSave = new BindableCollection<Season>();
            modelToSave.Add(seasonAndGame);
            seasonAndGame.owningTeam.seasons = null;

            StorageFile completeModelFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("CompleteModel", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string complete = JsonConvert.SerializeObject(modelToSave);
            await Windows.Storage.FileIO.WriteTextAsync(completeModelFile, complete);

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
