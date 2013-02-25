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
        public Playlist PlaylistModel { get; set; }
        public string Name
        {
            get
            {
                return PlaylistModel.name;
            }
        }
        public string NumClips
        {
            get
            {
                return PlaylistModel.clipCount + " Clips";
            }
        }
        public bool IsDownloaded { 
            get
            {
                return false;
            }
        }
        public string ThumbnailPath
        {
            get
            {
                if (PlaylistModel.thumbnailLocation == null)
                {
                    return "ms-appx:///Assets/hudl-mark-gray.png";
                }
                else
                {
                    return PlaylistModel.thumbnailLocation;
                }
            }
        }

        public PlaylistViewModel(Playlist playlist)
        {
            PlaylistModel = playlist;
        }
    }
}
