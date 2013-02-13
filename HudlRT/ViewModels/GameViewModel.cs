using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class GameViewModel
    {
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
                return !isLastViewed ? numplaylists + " playlists" : "";
            }
        }
        public bool isLargeView { get; set; }
        public bool isLastViewed { get; set; }
        public Game GameModel {get; set; }
        private int numplaylists = 0;

        public GameViewModel(Game game, bool isLarge = false, bool isLastviewed = false)
        {
            GameModel = game;
            isLargeView = isLarge;
            isLastViewed = isLastviewed;
            int numplaylists = 0;
            foreach (Category c in game.categories)
            {
                numplaylists += c.playlists.Count();
            }
            
        }
    }
}
