using Caliburn.Micro;
using HudlRT.Models;
using HudlRT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class GameViewModel : PropertyChangedBase
    {
        private string _thumbNail;
        private string _numPlaylists;
        private double _imageWidth;
        public bool isLargeView { get; set; }
        public Game GameModel { get; set; }
        public bool isLastViewed { get; set; }


        public string Opponent
        {
            get
            {
                return "vs. " + GameModel.opponent;
            }
        }

        public string Date
        {
            get
            {
                return !isLastViewed ? GameModel.DisplayDate : "Viewed: " + GameModel.DisplayDate;
            }
        }

        public string NumPlaylists
        {
            get
            {
                return !isLastViewed ? _numPlaylists : "";
            }
            set
            {
                _numPlaylists = value + " playlists";
                NotifyOfPropertyChange(() => NumPlaylists);
            }
        }

        public string ThumbNail
        {
            get { return _thumbNail; }
            set
            {
                _thumbNail = value ;
                NotifyOfPropertyChange(() => ThumbNail);
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
            isLargeView = isLarge;
            isLastViewed = isLastviewed;
            ThumbNail = "ms-appx:///Assets/agile-hudl-logo-light.png";
            ImageWidth = 400;
        }

        public async void FetchThumbnailsAndPlaylistCounts() 

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
                            if (ThumbNail == "ms-appx:///Assets/agile-hudl-logo-light.png")
                            {
                                ThumbNail = cat.playlists[0].thumbnailLocation;
                                ImageWidth = 565;
                            }
                        }
                    }
                }
                //Populate the numplaylistsfield.
                NumPlaylists = numLists.ToString();
            }
        }

    }
}
