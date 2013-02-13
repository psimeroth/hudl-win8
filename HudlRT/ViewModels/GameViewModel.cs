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
                return GameModel.DisplayDate;
            }
        }
        public string NumPlaylists
        {
            get;
            set;
        }
        public bool isLargeView { get; set; }
        public Game GameModel {get; set; }

        public GameViewModel(Game game, bool isLarge = false)
        {
            GameModel = game;
            isLargeView = isLarge;
            int numplaylists = 0;
            foreach (Category c in game.categories)
            {
                numplaylists += c.playlists.Count();
            }
            NumPlaylists = numplaylists + " playlists";
        }
    }
}
