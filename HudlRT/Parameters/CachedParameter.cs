using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace HudlRT.Parameters
{
    public static class CachedParameter
    {
        public static bool isInitialized = false;
        public static string categoryId { get; set; }
        public static string gameId { get; set; }
        
        public static Game hubViewNextGame { get; set; }
        public static Game hubViewPreviousGame { get; set; }
                
        public static BindableCollection<GameViewModel> sectionViewGames { get; set; }
        public static GameViewModel sectionViewGameSelected { get; set; }
                
        public static BindableCollection<Category> sectionViewCategories { get; set; }
        public static CategoryViewModel sectionViewCategorySelected { get; set; }
                
        public static BindableCollection<PlaylistViewModel> sectionViewPlaylists { get; set; }
                
        public static PlaylistViewModel sectionViewPlaylistSelected { get; set; }
                
        public static BindableCollection<Clip> videoPageClips { get; set; }
                
        public static Playlist selectedPlaylist { get; set; }


        //new shit starts here
        public static BindableCollection<Season> seasonsDropDown { get; set; }
        public static BindableCollection<Team> teams { get; set; }
        public static Season seasonSelected { get; set; }
        //public static BindableCollection<Game> games {get;set;}

        

        public static bool noConnection = false;

        public static string hubViewDownloadsCount { get; set; }
        
        public static string hubViewDownloadsSizeInMB { get; set; }

        public static List<PlaylistViewModel> currentlyDownloadingPlaylists { get; set; }

        public static Progress<DownloadOperation> progressCallback { get; set; }

        public static BindableCollection<PlaylistViewModel> downloadedPlaylists { get; set; }

        public static CancellationTokenSource cts = new CancellationTokenSource();


        public static void resetCache(){
            isInitialized = false;
            categoryId = null;
            gameId = null;
            seasonsDropDown = null;
            seasonSelected = null;
            hubViewNextGame = null;
            hubViewPreviousGame = null;
            sectionViewGames = null;
            sectionViewGameSelected = null;
            sectionViewCategories = null;
            sectionViewCategorySelected = null;
            sectionViewPlaylists = null;
            sectionViewPlaylistSelected = null;
            videoPageClips = null;
            selectedPlaylist = null;
            downloadedPlaylists = null;
        }
    }
}
