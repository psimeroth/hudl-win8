using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    /// <summary>
    /// Used for binding to a list of playlists
    /// </summary>
    public class PlaylistViewModel : PropertyChangedBase
    {
        public string Name { get; set; }
        public string NumClips { get; set; }
        public bool IsDownloaded { get; set; }
        public string ThumbnailPath { get; set; }
        public string PlaylistId { get; set; }

        public static PlaylistViewModel FromPlaylist(Playlist playlist)
        {
            PlaylistViewModel cvm = new PlaylistViewModel();
            cvm.Name = playlist.name;
            cvm.NumClips = playlist.clipCount.ToString();
            cvm.PlaylistId = playlist.playlistId;
            cvm.IsDownloaded = false;
            if (playlist.thumbnailLocation == null)
            {
                cvm.ThumbnailPath = "ms-appx:///Assets/agile-hudl-logo-dark.png";
            }
            else
            {
                cvm.ThumbnailPath = playlist.thumbnailLocation;

            }
            return cvm;
        }
    }
}
