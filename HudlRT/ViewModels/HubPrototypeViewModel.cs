using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class HubPrototypeViewModel : ViewModelBase
    {
        INavigationService navigationService;
        /*private LargeGameViewModel _previousGame;
        private LargeGameViewModel _nextGame;

        public LargeGameViewModel NextGame
        {
            get { return _nextGame; }
            set
            {
                if (value == _nextGame) return;
                _nextGame = value;
                NotifyOfPropertyChange(() => NextGame);
            }
        }

        public LargeGameViewModel PreviousGame
        {
            get { return _previousGame; }
            set
            {
                if (value == _previousGame) return;
                _previousGame = value;
                NotifyOfPropertyChange(() => PreviousGame);
            }
        }*/

        private BindableCollection<HubPageGameEntry> _hubGames;

        public BindableCollection<HubPageGameEntry> HubGames
        {
            get { return _hubGames; }
            set
            {
                if (value == _hubGames) return;
                _hubGames = value;
                NotifyOfPropertyChange(() => HubGames);
            }
        }

        protected override void OnActivate()
        {
            CachedParameter.InitializeForFrontend();
            //PreviousGame = LargeGameViewModel.FromGame(CachedParameter.hubViewPreviousGame);
            //NextGame = LargeGameViewModel.FromGame(CachedParameter.hubViewNextGame);
            HubGames = new BindableCollection<HubPageGameEntry>();
            HubPageGameEntry NextGame = new HubPageGameEntry() { Name = "Next Game" };
            NextGame.Games.Add(CachedParameter.hubViewNextGame);
            HubPageGameEntry LastGame = new HubPageGameEntry() { Name = "Last Game" };
            LastGame.Games.Add(CachedParameter.hubViewPreviousGame);
            HubGames.Add(NextGame);
            HubGames.Add(LastGame);


            base.OnActivate();
        }

        public void Test(ItemClickEventArgs eventArgs)
        {
            var clip = (HubPageGameEntry)eventArgs.ClickedItem;
        }

        public HubPrototypeViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            this.navigationService = navigationService;
            //CharmsData.navigationService = navigationService;
            //this.OnActivate();
            //SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }
    }
}