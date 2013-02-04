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
        /*INavigationService navigationService;
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
        }*/

        public BindableCollection<HubGroupViewModel> Groups
        {
            get;
            private set;
        }

        protected override void OnActivate()
        {
            base.OnInitialize();
            
            CachedParameter.InitializeForFrontend();
            LargeGameViewModel previous = LargeGameViewModel.FromGame(CachedParameter.hubViewPreviousGame, true);
            LargeGameViewModel next = LargeGameViewModel.FromGame(CachedParameter.hubViewNextGame, true);
            previous.isLargeView = true;
            next.isLargeView = true;
            HubGroupViewModel NextGame = new HubGroupViewModel() { Name = "Next Game", Games = new BindableCollection<LargeGameViewModel>() };
            NextGame.Games.Add(previous);
            HubGroupViewModel LastGame = new HubGroupViewModel() { Name = "Last Game", Games = new BindableCollection<LargeGameViewModel>() };
            LastGame.Games.Add(next);
            Groups.Add(NextGame);
            Groups.Add(LastGame);

            HubGroupViewModel schedule = new HubGroupViewModel() { Name = "Schedule", Games = new BindableCollection<LargeGameViewModel>() };
            for (int i = 0; i < 5; i++)
            {
                LargeGameViewModel temp = LargeGameViewModel.FromGame(CachedParameter.hubViewPreviousGame, false);
                LargeGameViewModel temp2 = LargeGameViewModel.FromGame(CachedParameter.hubViewNextGame, false);
                schedule.Games.Add(temp);
                schedule.Games.Add(temp2);
            }
            Groups.Add(schedule);

        }

        /*public void Test(ItemClickEventArgs eventArgs)
        {
            var clip = (HubPageGameEntry)eventArgs.ClickedItem;
        }*/

        public HubPrototypeViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Groups = new BindableCollection<HubGroupViewModel>();
            //this.navigationService = navigationService;
            //CharmsData.navigationService = navigationService;
            //this.OnActivate();
            //SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }
    }
}