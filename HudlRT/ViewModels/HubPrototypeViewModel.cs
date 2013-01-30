using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class HubPrototypeViewModel : ViewModelBase
    {
        INavigationService navigationService;
        private LargeGameViewModel _previousGame;
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
        }


        protected override void OnActivate()
        {
            CachedParameter.InitializeForFrontend();
            PreviousGame = LargeGameViewModel.FromGame(CachedParameter.hubViewPreviousGame);
            NextGame = LargeGameViewModel.FromGame(CachedParameter.hubViewNextGame);
            base.OnActivate();
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