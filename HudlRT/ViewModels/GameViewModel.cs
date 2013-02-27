using Caliburn.Micro;
using HudlRT.Models;
using HudlRT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    public class GameViewModel : PropertyChangedBase
    {
        private string _thumbNail;
        private string _numPlaylists;
        private double _imageWidth;
        public bool IsLargeView { get; set; }
        public bool IsLastViewed { get; set; }
        public Game GameModel { get; set; }
        public Task FetchPlaylists { get; set; }
        private Visibility playButtonVisibility;
        public Visibility PlayButtonVisibility
        {
            get
            {
                return IsLastViewed ? Visibility.Visible : Visibility.Collapsed;
            }
            set
            {
                playButtonVisibility = value;
            }
        }

        public string Opponent
        {
            get
            {
                return GameModel.opponent;
            }
        }

        public string Date
        {
            get
            {
                return !IsLastViewed ? GameModel.DisplayDate : "Viewed: " + GameModel.DisplayDate;
            }
        }

        public string NumPlaylists
        {
            get
            {
                return !IsLastViewed ? _numPlaylists : "";
            }
            set
            {
                _numPlaylists = value + " playlists";
                NotifyOfPropertyChange(() => NumPlaylists);
            }
        }

        public string Thumbnail
        {
            get { return _thumbNail; }
            set
            {
                _thumbNail = value ;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                _imageWidth = value;
                NotifyOfPropertyChange(() => ImageWidth);
            }
        }

        public GameViewModel(Game game, bool isLarge = false, bool isLastviewed = false)
        {
            GameModel = game;
            IsLargeView = isLarge;
            IsLastViewed = isLastviewed;
            Thumbnail = "ms-appx:///Assets/agile-hudl-logo-light.png";
            if (IsLastViewed)
            {
                ImageWidth = 565;
            }
            else
            {
                ImageWidth = 350;
            }
        }

        public async Task FetchThumbnailsAndPlaylistCounts() 
        {
            if (ServiceAccessor.ConnectedToInternet())
            {
                CategoryResponse response = await ServiceAccessor.GetGameCategories(GameModel.gameId);
                if (response.status == SERVICE_RESPONSE.SUCCESS)
                {
                    GameModel.categories = response.categories;
                    int numLists = 0;
                    foreach (Category cat in GameModel.categories)
                    {
                        PlaylistResponse playResponse = await ServiceAccessor.GetCategoryPlaylists(cat.categoryId);
                        if (response.status == SERVICE_RESPONSE.SUCCESS)
                        {
                            cat.playlists = playResponse.playlists;
                            if (cat.playlists != null && cat.playlists.Count() > 0)
                            {
                                numLists += cat.playlists.Count();
                                //Populate the thumbnail on the hub
                                if (Thumbnail == "ms-appx:///Assets/agile-hudl-logo-light.png")
                                {
                                    if (cat.playlists[0].thumbnailLocation != null)
                                    {
                                        Thumbnail = cat.playlists[0].thumbnailLocation;
                                        ImageWidth = 565;
                                    }
                                }
                            }
                        }
                    }
                    //Populate the numplaylistsfield.
                    NumPlaylists = numLists.ToString();
                }
            }
            else
            {
                int numLists = 0;
                foreach (Category cat in GameModel.categories)
                {
                    numLists += cat.playlists.Count;
                }
                NumPlaylists = numLists.ToString();
                ImageWidth = 565;
            }
        }
    }
}
