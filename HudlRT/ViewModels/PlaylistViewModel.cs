using Caliburn.Micro;
using HudlRT.Common;
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
        public Task FetchClips { get; set; }

        public async Task FetchClipsAndHeaders()
        {
            if (ServiceAccessor.ConnectedToInternet())
            {
                PlaylistModel.clips = new BindableCollection<Clip>();
                ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(PlaylistModel.playlistId);
                if (response.status == SERVICE_RESPONSE.SUCCESS)
                {
                    PlaylistModel.clips = response.clips;
                    PlaylistModel.displayColumns = response.DisplayColumns;
                }
                else
                {
                }
            }
        }

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
        private Visibility downloadedIcon_Visibility;
        public Visibility DownloadedIcon_Visibility
        {
            get { return downloadedIcon_Visibility; }
            set
            {
                downloadedIcon_Visibility = value;
                NotifyOfPropertyChange(() => DownloadedIcon_Visibility);
            }
        }

        public PlaylistViewModel(Playlist playlist)
        {
            PlaylistModel = playlist;
            DownloadedIcon_Visibility = Visibility.Collapsed;
        }
    }
}
