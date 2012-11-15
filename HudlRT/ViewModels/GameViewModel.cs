using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    /// <summary>
    /// Used for binding to a list of games
    /// </summary>
    public class GameViewModel : PropertyChangedBase
    {
        private string _opponent { get; set; }
        private string _date { get; set; }
        private bool _isHome { get; set; }
        private long _gameId { get; set; }
        private BindableCollection<CategoryViewModel> _categories { get; set; }
        private CategoryViewModel _selectedCategory { get; set; }

        public static GameViewModel FromDTO(GameDTO gameDTO)
        {
            GameViewModel game = new GameViewModel();
            game._gameId = gameDTO.GameId;
            game._isHome = gameDTO.Ishome;
            game._opponent = gameDTO.Opponent;
            game._date = gameDTO.Date.ToString("d");
            game._categories = new BindableCollection<CategoryViewModel>();
            return game;
        }

        public static GameViewModel FromGame(Game gameModel)
        {
            GameViewModel game = new GameViewModel();
            game._gameId = gameModel.gameId;
            game._isHome = gameModel.isHome;
            game._opponent = gameModel.opponent;
            game._date = gameModel.date.ToString("d");
            game._categories = new BindableCollection<CategoryViewModel>();
            return game;
        }

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

        public bool IsHome
        {
            get { return _isHome; }
            set
            {
                if (value == _isHome) return;
                _isHome = value;
                NotifyOfPropertyChange(() => IsHome);
            }
        }

        public long GameId
        {
            get { return _gameId; }
            set
            {
                if (value == _gameId) return;
                _gameId = value;
                NotifyOfPropertyChange(() => GameId);
            }
        }

        public BindableCollection<CategoryViewModel> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                NotifyOfPropertyChange(() => Categories);
            }
        }

        public CategoryViewModel SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                NotifyOfPropertyChange(() => SelectedCategory);
            }
        }
    }
}
