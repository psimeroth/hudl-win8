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
                AppDataAccessor.SetTeamContext(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
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
            BindableCollection<Season> downloadedSeasons = await DownloadAccessor.Instance.GetDownloadsModel(true);
            if (ServiceAccessor.ConnectedToInternet())
            {
                SeasonsDropDown = await GetSortedSeasons();
            }
            else
            {
                SeasonsDropDown = downloadedSeasons;
            }
            string savedSeasonId = AppDataAccessor.GetTeamContext().seasonID;

            if (savedSeasonId != null && SeasonsDropDown.Any())
            {
                SelectedSeason = SeasonsDropDown.Where(u => u.seasonID == savedSeasonId).FirstOrDefault() ?? SeasonsDropDown[0];
            }
            else
            {
                SelectedSeason = SeasonsDropDown.LastOrDefault(u => u.year >= DateTime.Now.Year) ?? SeasonsDropDown[0];
                AppDataAccessor.SetTeamContext(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID);
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


        private async void PopulateGroups()
        {
            

            BindableCollection<HubGroupViewModel> NewGroups = new BindableCollection<HubGroupViewModel>();

            //If these aren't set here, if there is no schedule, these still link to another season's next and last games.
            previousGame = null;
            nextGame = null;
            HubGroupViewModel FirstEntryVM = new HubGroupViewModel() { Name = null, Games = new BindableCollection<GameViewModel>() };

            if (ServiceAccessor.ConnectedToInternet())
            {
                games = await GetGames();

                if (games != null)
                {
                    GetNextPreviousGames();
                    NextGameVM.Games = new BindableCollection<GameViewModel>();
                    LastGameVM.Games = new BindableCollection<GameViewModel>();
                }

                if (previousGame != null)
                {
                    GameViewModel previous = new GameViewModel(previousGame, true);
                    previous.FetchPlaylists = previous.FetchThumbnailsAndPlaylistCounts();
                    previous.IsLargeView = true;
                    LastGameVM.Games.Add(previous);
                }
                if (nextGame != null)
                {
                    GameViewModel next = new GameViewModel(nextGame, true);
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
            else
            {
                games = SelectedSeason.games;
            }

            if (games != null)
            {
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

            if (sortedGames.Count > 0)
            {
                if (DateTime.Compare(DateTime.Now, sortedGames[sortedGames.Count - 1].date) <= 0)
                {
                    nextGame = sortedGames[sortedGames.Count - 1];
                    previousGame = null;
                }
                else if (DateTime.Compare(DateTime.Now, sortedGames[0].date) >= 0)
                {
                    nextGame = null;
                    previousGame = sortedGames[0];
                }
                else
                {
                    nextGame = sortedGames.Where(game => DateTime.Compare(DateTime.Now, game.date) < 0).LastOrDefault();
                    previousGame = sortedGames.Where(game => DateTime.Compare(DateTime.Now, game.date) > 0).FirstOrDefault();
                }
            }
            else
            {
                nextGame = null;
                previousGame = null;
            }
        }

        public async Task<BindableCollection<Game>> GetGames()
        {
            GameResponse response = await ServiceAccessor.GetGames(SelectedSeason.owningTeam.teamID.ToString(), SelectedSeason.seasonID.ToString());
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
                BindableCollection<Team> teams = response.teams;
                BindableCollection<Season> seasons = new BindableCollection<Season>();
                foreach (Team team in teams)
                {
                    foreach (Season season in team.seasons)
                    {
                        seasons.Add(season);
                    }
                }
                return new BindableCollection<Season>(seasons.OrderByDescending(s => s.year));
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
            //Game parameter = gameViewModel.GameModel;
            
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