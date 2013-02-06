using Caliburn.Micro;
using HudlRT.Models;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using HudlRT.Parameters;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using System.Linq;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        INavigationService navigationService;

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

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            navigationService.NavigateToViewModel<SectionViewModel>();
        }

        public HubViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Groups = new BindableCollection<HubGroupViewModel>();
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }
    }
}