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
        public string Opponent { get; set; }
        public string Date { get; set; }
        public string NumPlaylists { get; set; }
        public bool isLargeView { get; set; }
        public string GameId { get; set; }

        public static GameViewModel FromGame(Game game, bool isLarge = false)
        {
            int numplaylists = 0;
            foreach (Category c in game.categories)
            {
                numplaylists += c.cutups.Count();
            }

            GameViewModel largeVM = new GameViewModel()
            {
                Opponent = "vs. " + game.opponent,
                Date = game.date.ToString("d"),
                NumPlaylists = numplaylists.ToString() + " playlists",
                isLargeView = isLarge,
                GameId = game.gameId
            };
            return largeVM;
        }
    }
}
