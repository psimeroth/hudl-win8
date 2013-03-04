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
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        INavigationService navigationService;

        private string _noScheduleEntriesText;
        public string NoScheduleEntriesText
        {
            get { return _noScheduleEntriesText; }
            set
            {
                _noScheduleEntriesText = value;
                NotifyOfPropertyChange(() => NoScheduleEntriesText);
            }
        }

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

        private bool _pageIsEnabled;
        public bool PageIsEnabled
        {
            get { return _pageIsEnabled; }
            set
            {
                _pageIsEnabled = value;
                NotifyOfPropertyChange(() => PageIsEnabled);
            }
        }

        private Visibility _progressRingVisibility;
        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set
            {
                _progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        private bool _progressRingIsActive;
        public bool ProgressRingIsActive
        {
            get { return _progressRingIsActive; }
            set
            {
                _progressRingIsActive = value;
                NotifyOfPropertyChange(() => ProgressRingIsActive);
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
                AppDataAccessor.SetTeamContext(selectedSeason.seasonId, selectedSeason.owningTeam.teamID);
                PopulateGroups();
            }
        }

        private BindableCollection<Game> games { get; set; }

        private Game _nextGame {get; set;}
        private Game _previousGame { get; set; }
        private BindableCollection<Game> _otherItems { get; set; }
        private HubGroupViewModel NextGameVM = new HubGroupViewModel() { Name = "Next Game", Games = new BindableCollection<GameViewModel>() };
        private HubGroupViewModel LastGameVM = new HubGroupViewModel() { Name = "Last Game", Games = new BindableCollection<GameViewModel>() };
        private HubGroupViewModel LastViewedVM = new HubGroupViewModel() { Name = "Last Viewed", Games = new BindableCollection<GameViewModel>() };

        protected override async void OnInitialize()
        {
            base.OnInitialize();
            await DownloadAccessor.Instance.GetDownloads();
            if (ServiceAccessor.ConnectedToInternet())
            {
                SeasonsDropDown = await GetSortedSeasons();
            }
            else
            {
                SeasonsDropDown = await DownloadAccessor.Instance.GetDownloadsModel();  
            }
            string savedSeasonId = AppDataAccessor.GetTeamContext().seasonID;

            if (savedSeasonId != null && SeasonsDropDown.Any())
            {
                SelectedSeason = SeasonsDropDown.Where(u => u.seasonId == savedSeasonId).FirstOrDefault() ?? SeasonsDropDown[0];
            }
            else
            {
                SelectedSeason = SeasonsDropDown.LastOrDefault(u => u.year >= DateTime.Now.Year) ?? SeasonsDropDown[0];
                AppDataAccessor.SetTeamContext(SelectedSeason.seasonId, SelectedSeason.owningTeam.teamID);
            }
            if (!SeasonsDropDown.Any())
            {
                //show message here if no downloads
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;

            PageIsEnabled = true;

            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID != null && ServiceAccessor.ConnectedToInternet())
            {
                Game LastViewedGame = new Game { gameId = response.ID, opponent = response.name, date = DateTime.Parse(response.timeStamp) };//this is actually a playlist - not a game
                GameViewModel lastViewed = new GameViewModel(LastViewedGame, true, true);
                lastViewed.Thumbnail = response.thumbnail;
                LastViewedVM = new HubGroupViewModel() { Name = "Last Viewed", Games = new BindableCollection<GameViewModel>() };
                LastViewedVM.Games.Add(lastViewed);

                if (Groups.Count == 0 && (NoScheduleEntriesText == null || NoScheduleEntriesText == ""))
                {
                    ProgressRingVisibility = Visibility.Visible;
                    ProgressRingIsActive = true;
                }

                if (Groups.Count >= 3)
                {
                    
                    HubGroupViewModel oldLastViewed = Groups.Where(u => u.Name == "Last Viewed").FirstOrDefault();
                    if (oldLastViewed != null)
                    {
                        Groups[Groups.IndexOf(oldLastViewed)] = LastViewedVM;
                    }
                    else
                    {
                        Groups.Insert(1, LastViewedVM);
                    }
                }
            }
        }


        private void PopulateGroups()
        {
            BindableCollection<HubGroupViewModel> NewGroups = new BindableCollection<HubGroupViewModel>();

            //If these aren't set here, if there is no schedule, these still link to another season's next and last games.
            _previousGame = null;
            _nextGame = null;
            _otherItems = null;

            //This is used for extra spacing in the Gridview
            HubGroupViewModel FirstEntryVM = new HubGroupViewModel() { Name = null, Games = new BindableCollection<GameViewModel>() };

            games = selectedSeason.games;

            //Find the other items if present
            _otherItems = games.Where(g => g.Classification != "1") as BindableCollection<Game>;
            foreach(Game g in _otherItems)
            {
                games.Remove(g);
            }

            if (ServiceAccessor.ConnectedToInternet())
            {
                games = selectedSeason.games;

                GetNextPreviousGames();
                NextGameVM.Games = new BindableCollection<GameViewModel>();
                LastGameVM.Games = new BindableCollection<GameViewModel>();

                if (_previousGame != null)
                {
                    GameViewModel previous = new GameViewModel(_previousGame, true);
                    previous.FetchPlaylists = previous.FetchThumbnailsAndPlaylistCounts();
                    previous.IsLargeView = true;
                    LastGameVM.Games.Add(previous);
                }
                if (_nextGame != null)
                {
                    GameViewModel next = new GameViewModel(_nextGame, true);
                    next.IsLargeView = true;
                    next.FetchPlaylists = next.FetchThumbnailsAndPlaylistCounts();
                    NextGameVM.Games.Add(next);
                }

                LastViewedResponse response = AppDataAccessor.GetLastViewed();
                if (response.ID != null)
                {
                    NewGroups.Add(LastViewedVM);
                }

                if (NextGameVM.Games.Count() > 0)
                {
                    NewGroups.Add(NextGameVM);
                }
                if (LastGameVM.Games.Count() > 0)
                {
                    NewGroups.Add(LastGameVM);
                }
            }

            HubGroupViewModel schedule = new HubGroupViewModel() { Name = "Schedule", Games = new BindableCollection<GameViewModel>() };
            foreach (Game g in games)
            {
                GameViewModel gamevm = new GameViewModel(g);
                gamevm.FetchPlaylists = gamevm.FetchThumbnailsAndPlaylistCounts();
                schedule.Games.Add(gamevm);
            }
            if (schedule.Games.Count > 0)
            {
                NewGroups.Add(schedule);
            }

            HubGroupViewModel otherItems = new HubGroupViewModel() { Name = "Other", Games = new BindableCollection<GameViewModel>() };
            foreach (Game g in _otherItems)
            {
                GameViewModel gamevm = new GameViewModel(g);
                otherItems.Games.Add(gamevm);
            }
            if(otherItems.Games.Count > 0)
            {
                NewGroups.Add(otherItemsGroup);
            }
            

            ProgressRingVisibility = Visibility.Collapsed;
            ProgressRingIsActive = false;

            if (NewGroups.Count == 0)
            {
                NoScheduleEntriesText = "There are no schedule entries for this season";
            }
            else
            {
                NoScheduleEntriesText = "";
                //Needed for left padding
                NewGroups.Insert(0,FirstEntryVM);
            }

            Groups = NewGroups;
            
        }

        public void GetNextPreviousGames()
        {
            List<Game> sortedGames = new List<Game>();

            sortedGames.AddRange(games);
            sortedGames.Sort((x, y) => DateTime.Compare(y.date, x.date));//most recent to least recent
            DateTime fakeNow = new DateTime(2012, 10, 8);

            if (sortedGames.Count > 0)
            {
                if (DateTime.Compare(fakeNow, sortedGames[sortedGames.Count - 1].date) <= 0)
                {
                    _nextGame = sortedGames[sortedGames.Count - 1];
                    _previousGame = null;
                }
                else if (DateTime.Compare(fakeNow, sortedGames[0].date) >= 0)
                {
                    _nextGame = null;
                    _previousGame = sortedGames[0];
                }
                else
                {
                    _nextGame = sortedGames.Where(game => DateTime.Compare(fakeNow, game.date) < 0).LastOrDefault();
                    _previousGame = sortedGames.Where(game => DateTime.Compare(fakeNow, game.date) > 0).FirstOrDefault();
                }
            }
            else
            {
                _nextGame = null;
                _previousGame = null;
            }
        }

        //this returns seasons populated down to the category level.
        public async Task<BindableCollection<Season>> GetSortedSeasons()
        {
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                BindableCollection<Team> teams = response.teams;
                BindableCollection<Season> seasons = new BindableCollection<Season>();
                foreach (Team team in teams)
                {
                    seasons.AddRange(await GetPopulatedSeasons(team));
                }
                return new BindableCollection<Season>(seasons.OrderByDescending(s => s.year));
            }
            return null;
        }

        public async Task<BindableCollection<Season>> GetPopulatedSeasons(Team team)
        {
            SeasonsResponse response = await ServiceAccessor.GetPopulatedSeasons(team);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                return response.Seasons;
            }
            return null;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView categoriesGrid = (GridView)sender;
            categoriesGrid.SelectedIndex = -1;

        }

        public async void GameSelected(ItemClickEventArgs eventArgs)
        {
            PageIsEnabled = false;
            ProgressRingIsActive = true;
            ProgressRingVisibility = Visibility.Visible;

            GameViewModel gameViewModel = (GameViewModel)eventArgs.ClickedItem;
            Season parameter = SelectedSeason;
            parameter.games = new BindableCollection<Game>();
            parameter.games.Add(gameViewModel.GameModel);
            
            if (!gameViewModel.IsLastViewed)
            {
                await gameViewModel.FetchPlaylists;
                navigationService.NavigateToViewModel<SectionViewModel>(parameter);
            }
            else
            {
                Playlist downloadedPlaylist = DownloadAccessor.Instance.downloadedPlaylists.Where(u => u.playlistId == gameViewModel.GameModel.gameId).FirstOrDefault();
                if (downloadedPlaylist != null)
                {
                    navigationService.NavigateToViewModel<VideoPlayerViewModel>(downloadedPlaylist);
                }
                else
                {
                    ClipResponse response = await ServiceAccessor.GetPlaylistClipsAndHeaders(gameViewModel.GameModel.gameId);
                    Playlist lastViewedPlaylist = new Playlist { playlistId = gameViewModel.GameModel.gameId, name = gameViewModel.GameModel.opponent, thumbnailLocation = gameViewModel.Thumbnail, clips = response.clips, displayColumns = response.DisplayColumns, clipCount = response.clips.Count};
                    navigationService.NavigateToViewModel<VideoPlayerViewModel>(lastViewedPlaylist);
                }
            }
            
        }

        public HubViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Groups = new BindableCollection<HubGroupViewModel>();
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
        }
    }
}