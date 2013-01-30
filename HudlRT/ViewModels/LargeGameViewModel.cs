using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class LargeGameViewModel : PropertyChangedBase
    {
        private string _opponent;
        private string _date;
        private string _numPlaylists;


        public string Opponent
        {
            get { return _opponent; }
            set
            {
                if (value == _opponent) return;
                _opponent = value;
                NotifyOfPropertyChange(() => Opponent);
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                if (value == _date) return;
                _date = value;
                NotifyOfPropertyChange(() => Date);
            }
        }

        public string NumPlaylists
        {
            get { return _numPlaylists; }
            set
            {
                if (value == _numPlaylists) return;
                _numPlaylists = value;
                NotifyOfPropertyChange(() => NumPlaylists);
            }
        }

       public static LargeGameViewModel FromGame(Game game)
       {
           int numplaylists = 0;
           foreach(Category c in game.categories){
               numplaylists += c.cutups.Count();
           }
           LargeGameViewModel largeVM = new LargeGameViewModel()
           {
               Opponent = "vs. " + game.opponent,
               Date = game.date.ToString("d"),
               NumPlaylists = numplaylists.ToString() + " playlists"
           };

           return largeVM;
       }
    }
}
