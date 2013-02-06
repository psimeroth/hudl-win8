using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class LargeGameViewModel
    {
        public string Opponent { get; set; }
        public string Date { get; set; }
        public string NumPlaylists { get; set; }
        public bool isLargeView { get; set; }

       public static LargeGameViewModel FromGame(Game game, bool isLarge)
       {
           int numplaylists = 0;
           foreach(Category c in game.categories){
               numplaylists += c.cutups.Count();
           }
           LargeGameViewModel largeVM = new LargeGameViewModel()
           {
               Opponent = "vs. " + game.opponent,
               Date = game.date.ToString("d"),
               NumPlaylists = numplaylists.ToString() + " playlists",
               isLargeView = isLarge
           };

           return largeVM;
       }
    }
}
