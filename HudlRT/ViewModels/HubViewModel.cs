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

        private BindableCollection<HubGroupViewModel> _groups;
        public BindableCollection<HubGroupViewModel> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                NotifyOfPropertyChange(() => Groups);
            }
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
                CachedParameter.seasonSelected = selectedSeason;
                PopulateGroups();
            }
        }

        private BindableCollection<Game> games { get; set; }

        private Game nextGame {get; set;}
        private Game previousGame { get; set; }
        private HubGroupViewModel NextGameVM = new HubGroupViewModel() { Name = "Next Game", Games = new BindableCollection<GameViewModel>() };
        private HubGroupViewModel LastGameVM = new HubGroupViewModel() { Name = "Last Game", Games = new BindableCollection<GameViewModel>() };
        private HubGroupViewModel LastViewedVM = new HubGroupViewModel() { Name = "Last Viewed", Games = new BindableCollection<GameViewModel>() };

        protected override async void OnInitialize()
        {
            base.OnInitialize();

            CachedParameter.seasonsDropDown = await GetSortedSeasons();
            CachedParameter.seasonSelected = CachedParameter.seasonsDropDown.LastOrDefault(u => u.year >= DateTime.Now.Year) ?? CachedParameter.seasonsDropDown[0];
            
            SeasonsDropDown = CachedParameter.seasonsDropDown;
            SelectedSeason = CachedParameter.seasonSelected;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID != null)
            {
                Game LastViewedGame = new Game { gameId = response.ID, opponent = response.name, date = DateTime.Parse(response.timeStamp) };
                GameViewModel lastViewed = new GameViewModel(LastViewedGame, true, true);
                lastViewed.Thumbnail = response.thumbnail;
                LastViewedVM = new HubGroupViewModel() { Name = "Last Viewed", Games = new BindableCollection<GameViewModel>() };
                LastViewedVM.Games.Add(lastViewed);
                if (Groups.Count >= 3)
                {
                    Groups[2] = LastViewedVM;
                }
            }
        }


        private async void PopulateGroups()
        {
            games = await GetGames();

            GetNextPreviousGames();
            NextGameVM.Games = new BindableCollection<GameViewModel>();
            LastGameVM.Games = new BindableCollection<GameViewModel>();

            if (previousGame != null)
            {
                GameViewModel previous = new GameViewModel(previousGame, true);
                previous.FetchThumbnailsAndPlaylistCounts();
                previous.IsLargeView = true;
                NextGameVM.Games.Add(previous);
            }
            if (nextGame != null)
            {
                GameViewModel next = new GameViewModel(nextGame, true);
                //next.isLargeView = true;
                next.FetchThumbnailsAndPlaylistCounts();
                LastGameVM.Games.Add(next);
            }

            BindableCollection<HubGroupViewModel> NewGroups = new BindableCollection<HubGroupViewModel>();
            NewGroups.Add(NextGameVM);
            NewGroups.Add(LastGameVM);

            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID != null)
            {
                NewGroups.Add(LastViewedVM);
            }
            

            HubGroupViewModel schedule = new HubGroupViewModel() { Name = "Schedule", Games = new BindableCollection<GameViewModel>() };
            foreach (Game g in games)
            {
                GameViewModel gamevm = new GameViewModel(g);
                gamevm.FetchThumbnailsAndPlaylistCounts();
                schedule.Games.Add(gamevm);
            }
            NewGroups.Add(schedule);
            Groups = NewGroups;
        }

        public void GetNextPreviousGames()
        {
            List<Game> sortedGames = new List<Game>();
            DateTime lastGameDate;

            foreach (Game game in games)
            {
                sortedGames.Add(game);
            }
            sortedGames.Sort((x, y) => DateTime.Compare(y.date, x.date));//most recent to least recent
            if (sortedGames.Count > 0)
            {
                lastGameDate = sortedGames[0].date;


                if (DateTime.Compare(DateTime.Now, lastGameDate) >= 0)
                {
                    nextGame = sortedGames[0];
                    if (sortedGames.Count >= 2)
                    {
                        previousGame = sortedGames[1];
                    }
                }
                else
                {
                    for (int i = 0; i < sortedGames.Count; i++)
                    {
                        if (DateTime.Compare(sortedGames[i].date, DateTime.Now) < 0)
                        {
                            if (i == 0)
                            {
                                nextGame = sortedGames[i];
                            }
                            else
                            {
                                nextGame = sortedGames[i - 1];
                                previousGame = sortedGames[i];
                            }
                            break;
                        }
                    }
                }
            }
        }

        public async Task<BindableCollection<Game>> GetGames()
        {
            GameResponse response = await ServiceAccessor.GetGames(CachedParameter.seasonSelected.owningTeam.teamID.ToString(), CachedParameter.seasonSelected.seasonID.ToString());
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                
                return response.games;
            }
            return null;
        }

        public async Task<BindableCollection<Season>> GetSortedSeasons()
        {
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                CachedParameter.teams = response.teams;
                CachedParameter.seasonsDropDown = new BindableCollection<Season>();
                foreach (Team team in CachedParameter.teams)
                {
                    foreach (Season season in team.seasons)
                    {
                        CachedParameter.seasonsDropDown.Add(season);
                    }
                }
                return new BindableCollection<Season>(CachedParameter.seasonsDropDown.OrderByDescending(season => season.year));
            }
            return null;
        }

        public void GameSelected(ItemClickEventArgs eventArgs)
        {
            GameViewModel gameViewModel = (GameViewModel)eventArgs.ClickedItem;
            string parameter = gameViewModel.GameModel.gameId;
            
            //CachedParameter.gameId = ((GameViewModel)eventArgs.ClickedItem).GameModel.gameId;
            if (!gameViewModel.IsLastViewed)
            {
                navigationService.NavigateToViewModel<SectionViewModel>(parameter);
            }
            else
            {

            }
            
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