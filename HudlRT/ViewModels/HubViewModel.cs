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

        private BindableCollection<Season> seasonsForDropDown;
        public BindableCollection<Season> SeasonsDropDown
        {
            get { return seasonsForDropDown; }
            set
            {
                seasonsForDropDown = value;
                NotifyOfPropertyChange(() => SeasonsDropDown);
            }
        }

        private Season selectedSeason;
        public Season SelectedSeason
        {
            get { return selectedSeason; }
            set
            {
                selectedSeason = value;
                NotifyOfPropertyChange(() => SelectedSeason);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            
            CachedParameter.InitializeForFrontend();
            GameViewModel previous = GameViewModel.FromGame(CachedParameter.hubViewPreviousGame, true);
            GameViewModel next = GameViewModel.FromGame(CachedParameter.hubViewNextGame, true);
            SeasonsDropDown = CachedParameter.seasonsDropDown;
            SelectedSeason = CachedParameter.seasonSelected;
            previous.isLargeView = true;
            next.isLargeView = true;
            HubGroupViewModel NextGame = new HubGroupViewModel() { Name = "Next Game", Games = new BindableCollection<GameViewModel>() };
            NextGame.Games.Add(previous);
            HubGroupViewModel LastGame = new HubGroupViewModel() { Name = "Last Game", Games = new BindableCollection<GameViewModel>() };
            LastGame.Games.Add(next);
            Groups.Add(NextGame);
            Groups.Add(LastGame);

            HubGroupViewModel schedule = new HubGroupViewModel() { Name = "Schedule", Games = new BindableCollection<GameViewModel>() };
            for (int i = 0; i < 5; i++)
            {
                GameViewModel temp = GameViewModel.FromGame(CachedParameter.hubViewPreviousGame, false);
                GameViewModel temp2 = GameViewModel.FromGame(CachedParameter.hubViewNextGame, false);
                schedule.Games.Add(temp);
                schedule.Games.Add(temp2);
            }
            Groups.Add(schedule);

        }

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            CachedParameter.gameId = ((GameViewModel)eventArgs.ClickedItem).GameId;
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