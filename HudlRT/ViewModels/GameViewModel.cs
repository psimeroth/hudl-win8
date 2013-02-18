﻿using Caliburn.Micro;
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
        public bool IsLargeView { get; set; }
        public Game GameModel { get; set; }
        public bool IsLastViewed { get; set; }


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
                return !IsLastViewed ? GameModel.DisplayDate : "Viewed: " + GameModel.DisplayDate;
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

        public string Thumbnail
        {
            get { return _thumbNail; }
            set
            {
                _thumbNail = value ;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        public GameViewModel(Game game, bool isLarge = false, bool isLastviewed = false)
        {
            GameModel = game;
            IsLargeView = isLarge;
            IsLastViewed = isLastviewed;
            Thumbnail = "ms-appx:///Assets/agile-hudl-logo-dark.png";
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
                            if (Thumbnail == "ms-appx:///Assets/agile-hudl-logo-dark.png")
                            {
                                Thumbnail = cat.playlists[0].thumbnailLocation;
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
