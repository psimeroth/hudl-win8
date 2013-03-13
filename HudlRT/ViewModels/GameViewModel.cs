using Caliburn.Micro;
using HudlRT.Models;
using HudlRT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class GameViewModel : PropertyChangedBase
    {
        private string _thumbNail;
        private string _numPlaylists;
        private string _stretch;
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
        public bool IsNextGame { get; set; }
        public bool IsPreviousGame { get; set; }

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
                if (IsLastViewed)
                {
                    return "Viewed: " + GameModel.DisplayDate;
                }
                if (GameModel.Classification != 1)
                {
                    return "-";
                }
                else
                {
                    return GameModel.DisplayDate;
                }
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
                _thumbNail = value;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        public string Stretch
        {
            get
            {
                return _stretch;
            }
            set
            {
                _stretch = value;
                NotifyOfPropertyChange(() => Stretch);
            }
        }

        public GameViewModel(Game game, bool isLarge = false, bool isLastviewed = false, bool isNextGame = false, bool isPreviousGame = false)
        {
            GameModel = game;
            IsLargeView = isLarge;
            IsLastViewed = isLastviewed;
            IsNextGame = isNextGame;
            IsPreviousGame = isPreviousGame;
            Thumbnail = "ms-appx:///Assets/hudl-mark-gray.png";
            Stretch = "None";
        }

        public async Task FetchThumbnailsAndPlaylistCounts()
        {
            int numLists = 0;
            if (ServiceAccessor.ConnectedToInternet())
            {
                foreach (Category cat in GameModel.categories)
                {
                    PlaylistResponse playResponse = await ServiceAccessor.GetCategoryPlaylists(cat.categoryId);
                    if (playResponse.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        cat.playlists = playResponse.playlists;
                        if (cat.playlists != null && cat.playlists.Count() > 0)
                        {
                            numLists += cat.playlists.Count();
                            //Populate the thumbnail on the hub
                            if (Thumbnail == "ms-appx:///Assets/hudl-mark-gray.png")
                            {
                                if (cat.playlists[0].thumbnailLocation != null)
                                {
                                    Thumbnail = cat.playlists[0].thumbnailLocation;
                                    Stretch = "UniformToFill";
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Category cat in GameModel.categories)
                {
                    numLists += cat.playlists.Count;
                }
            }
            //Populate the NumPlaylists field with the counter
            NumPlaylists = numLists.ToString();
        }
    }
}