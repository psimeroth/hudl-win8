﻿using Caliburn.Micro;
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
            await DownloadAccessor.Instance.GetDownloads();
            SeasonsDropDown = await GetSortedSeasons();
            string savedSeasonId = AppDataAccessor.GetTeamContext().seasonID;

            if (savedSeasonId != null)
            {
                SelectedSeason = SeasonsDropDown.Where(u => u.seasonID == savedSeasonId).FirstOrDefault();
            }
            else
            {
                SelectedSeason = SeasonsDropDown.LastOrDefault(u => u.year >= DateTime.Now.Year) ?? SeasonsDropDown[0];
                AppDataAccessor.SetTeamContext(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;

            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID != null)
            {
                Game LastViewedGame = new Game { gameId = response.ID, opponent = response.name, date = DateTime.Parse(response.timeStamp) };//this is actually a playlist - not a game
                GameViewModel lastViewed = new GameViewModel(LastViewedGame, true, true);
                lastViewed.Thumbnail = response.thumbnail;
                LastViewedVM = new HubGroupViewModel() { Name = "Last Viewed", Games = new BindableCollection<GameViewModel>() };
                LastViewedVM.Games.Add(lastViewed);
                if (Groups.Count >= 3)
                {
                    
                    HubGroupViewModel oldLastViewed = Groups.Where(u => u.Name == "Last Viewed").FirstOrDefault();
                    if (oldLastViewed != null)
                    {
                        Groups[Groups.IndexOf(oldLastViewed)] = LastViewedVM;
                    }
                    else
                    {
                        Groups.Insert(2, LastViewedVM);
                    }
                }
            }
        }


        private async void PopulateGroups()
        {
            games = await GetGames();
            //If these aren't set here, if there is no schedule, these still link to another season's next and last games.
            previousGame = null;
            nextGame = null;

            GetNextPreviousGames();
            NextGameVM.Games = new BindableCollection<GameViewModel>();
            LastGameVM.Games = new BindableCollection<GameViewModel>();

            if (previousGame != null)
            {
                GameViewModel previous = new GameViewModel(previousGame, true);
                previous.FetchPlaylists =  previous.FetchThumbnailsAndPlaylistCounts();
                previous.IsLargeView = true;
                NextGameVM.Games.Add(previous);
            }
            if (nextGame != null)
            {
                GameViewModel next = new GameViewModel(nextGame, true);
                //next.isLargeView = true;
                next.FetchPlaylists = next.FetchThumbnailsAndPlaylistCounts();
                LastGameVM.Games.Add(next);
            }

            BindableCollection<HubGroupViewModel> NewGroups = new BindableCollection<HubGroupViewModel>();
            if(NextGameVM.Games.Count() > 0)
            {
                NewGroups.Add(NextGameVM);
            }
            if (LastGameVM.Games.Count() > 0)
            {
                NewGroups.Add(LastGameVM);
            }

            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID != null)
            {
                NewGroups.Add(LastViewedVM);
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
            GameViewModel gameViewModel = (GameViewModel)eventArgs.ClickedItem;
            Game parameter = gameViewModel.GameModel;
            
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