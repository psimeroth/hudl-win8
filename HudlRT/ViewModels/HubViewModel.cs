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
        private readonly INavigationService navigationService;
        private string lastViewedId = null;
        private string lastViewedThumbnail = null;

        private bool lastViewedLocal = false;

        private Task<ClipResponse> loadLastViewed;
        private CutupViewModel lastViewedCutup;

        private bool noGamesGrid;
        public bool NoGamesGrid
        {
            get { return noGamesGrid; }
            set
            {
                noGamesGrid = value;
                NotifyOfPropertyChange(() => NoGamesGrid);
            }
        }
        private string recentWeeks_GridVisibility;
        public string RecentWeeks_GridVisibility
        {
            get { return recentWeeks_GridVisibility; }
            set
            {
                recentWeeks_GridVisibility = value;
                NotifyOfPropertyChange(() => RecentWeeks_GridVisibility);
            }
        }

        private string lastViewedVisibility;
        public string LastViewedVisibility
        {
            get { return lastViewedVisibility; }
            set
            {
                lastViewedVisibility = value;
                NotifyOfPropertyChange(() => LastViewedVisibility);
            }
        }

        private string lastViewedName;
        public string LastViewedName
        {
            get { return lastViewedName; }
            set
            {
                lastViewedName = value;
                NotifyOfPropertyChange(() => LastViewedName);
            }
        }

        private string lastViewedTimeStamp;
        public string LastViewedTimeStamp
        {
            get { return lastViewedTimeStamp; }
            set
            {
                lastViewedTimeStamp = value;
                NotifyOfPropertyChange(() => LastViewedTimeStamp);
            }
        }

        private string downloadedCutupSize;
        public string DownloadedCutupSize
        {
            get { return downloadedCutupSize; }
            set
            {
                downloadedCutupSize = value;
                NotifyOfPropertyChange(() => DownloadedCutupSize);
            }
        }

        private string downloadedCutupCount;
        public string DownloadedCutupCount
        {
            get { return downloadedCutupCount; }
            set
            {
                downloadedCutupCount = value;
                NotifyOfPropertyChange(() => DownloadedCutupCount);
            }
        }

        private BindableCollection<Team> teams;
        public BindableCollection<Team> Teams
        {
            get { return teams; }
            set
            {
                teams = value;
                NotifyOfPropertyChange(() => Teams);
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

        private Game nextGame;
        public Game NextGame
        {
            get { return nextGame; }
            set
            {
                nextGame = value;
                NotifyOfPropertyChange(() => NextGame);
            }
        }

        private Game previousGame;
        public Game PreviousGame
        {
            get { return previousGame; }
            set
            {
                previousGame = value;
                NotifyOfPropertyChange(() => PreviousGame);
            }
        }


        private BindableCollection<Category> nextGameCategories;
        public BindableCollection<Category> NextGameCategories
        {
            get { return nextGameCategories; }
            set
            {
                nextGameCategories = value;
                NotifyOfPropertyChange(() => NextGameCategories);
            }
        }

        private BindableCollection<Category> previousGameCategories;
        public BindableCollection<Category> PreviousGameCategories
        {
            get { return previousGameCategories; }
            set
            {
                previousGameCategories = value;
                NotifyOfPropertyChange(() => PreviousGameCategories);
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

        // Bound to the visibility of the progress ring. Swaps with 
        private string progressRingVisibility;
        public string ProgressRingVisibility
        {
            get { return progressRingVisibility; }
            set
            {
                progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        // Bound to the visibility of the login form stack panel
        private string colVisibility;
        public string ColVisibility
        {
            get { return colVisibility; }
            set
            {
                colVisibility = value;
                NotifyOfPropertyChange(() => ColVisibility);
            }
        }

        public HubViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_HubCommandsRequested;
        }

        public async void NavigateToSectionPage()
        {
            if (ServiceAccessor.ConnectedToInternet())
            {
                UpdateCachedParameter();
                navigationService.NavigateToViewModel<SectionViewModel>();
            }
            else
            {
                APIExceptionDialog.ShowNoInternetConnectionDialog(null, null);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            if (CachedParameter.isInitialized)
            {
                SeasonsDropDown = CachedParameter.seasonsDropDown;
                SelectedSeason = CachedParameter.seasonSelected;
                if (CachedParameter.hubViewNextGame != null && CachedParameter.hubViewPreviousGame != null)
                {
                    NextGame = CachedParameter.hubViewNextGame;
                    PreviousGame = CachedParameter.hubViewPreviousGame;
                    NextGameCategories = NextGame.categories;
                    PreviousGameCategories = PreviousGame.categories;
                }
                else
                {
                    FindNextPreviousGames(SelectedSeason);
                    CachedParameter.isInitialized = true;
                }
            }
            else
            {
                PopulateDropDown();
            }

            ColVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
            await DownloadAccessor.Instance.GetDownloads();
            DownloadedCutupSize = CachedParameter.hubViewDownloadsSizeInMB;
            DownloadedCutupCount = CachedParameter.hubViewDownloadsCount;


            LastViewedResponse response = AppDataAccessor.GetLastViewed();
            if (response.ID == null)
            {
                LastViewedName = "Hey Rookie!";
                LastViewedTimeStamp = "You haven't watched anything yet!";
            }
            else
            {
                LastViewedName = response.name;
                LastViewedTimeStamp = "Viewed: " + response.timeStamp;
                lastViewedId = response.ID;
                lastViewedThumbnail = response.thumbnail;

                loadLastViewed = LoadLastViewedCutup();
            }
        }

        private async Task<ClipResponse> LoadLastViewedCutup()
        {
            lastViewedCutup = new CutupViewModel { CutupId = lastViewedId, Name = LastViewedName, Thumbnail = lastViewedThumbnail };
            if (CachedParameter.downloadedCutups != null)
            {
                foreach (CutupViewModel cVM in CachedParameter.downloadedCutups)
                {
                    if (cVM.CutupId == lastViewedId)
                    {
                        lastViewedCutup.DisplayColumns = cVM.DisplayColumns;
                        lastViewedCutup.ClipCount = cVM.ClipCount;
                        lastViewedLocal = true;
                        return new ClipResponse { clips = cVM.Clips, status = SERVICE_RESPONSE.SUCCESS };
                    }
                }
            }
            lastViewedLocal = false;
            return await ServiceAccessor.GetCutupClips(lastViewedCutup);
        }

        public void UpdateCachedParameter()
        {
            CachedParameter.seasonsDropDown = SeasonsDropDown;
            CachedParameter.seasonSelected = SelectedSeason;
            CachedParameter.hubViewNextGame = NextGame;
            CachedParameter.hubViewPreviousGame = PreviousGame;
            CachedParameter.isInitialized = true;
        }

        public void UpdateParameterOnSeasonChange()
        {
                CachedParameter.sectionViewCategories = null;
                CachedParameter.sectionViewCategorySelected = null;
                CachedParameter.sectionViewCutups = null;
                CachedParameter.sectionViewGames = null;
                CachedParameter.sectionViewGameSelected = null;
                CachedParameter.selectedCutup = null;
        }

        public async void PopulateDropDown()
        {
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                Teams = response.teams;
                string teamID = null;
                string seasonID = null;
                bool foundSavedSeason = false;
                TeamContextResponse teamContext = AppDataAccessor.GetTeamContext();
                if (teamContext.seasonID != null && teamContext.teamID != null)
                {
                    teamID = teamContext.teamID;
                    seasonID = teamContext.seasonID;
                }
                SeasonsDropDown = new BindableCollection<Season>();
                foreach (Team team in Teams)
                {
                    foreach (Season season in team.seasons)
                    {
                        if (teamID == season.owningTeam.teamID && seasonID == season.seasonID)
                        {
                            SelectedSeason = season;
                            foundSavedSeason = true;
                        }
                        SeasonsDropDown.Add(season);
                    }
                }
                BindableCollection<Season> SeasonsDropDownSort = new BindableCollection<Season>(SeasonsDropDown.OrderByDescending(season => season.year));
                SeasonsDropDown = SeasonsDropDownSort;
                if (foundSavedSeason)
                {
                    FindNextPreviousGames(SelectedSeason);
                    NotifyOfPropertyChange(() => SelectedSeason);
                }
                if (!foundSavedSeason && SeasonsDropDown.Count > 0)
                {
                    int year = DateTime.Now.Year;
                    SelectedSeason = SeasonsDropDown.LastOrDefault(u => u.year >= year) ?? SeasonsDropDown[0];

                    FindNextPreviousGames(SelectedSeason);
                    NotifyOfPropertyChange(() => SelectedSeason);

                    AppDataAccessor.SetTeamContext(SelectedSeason.seasonID, SelectedSeason.owningTeam.teamID);
                }

                //populate this/next game
                
            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }
            else//could better handle exceptions
            {
                Teams = null;
            }
        }

        public async void FindNextPreviousGames(Season s)//sets gameThisWeek and gameNextWeek
        {
            GameResponse response = await ServiceAccessor.GetGames(s.owningTeam.teamID.ToString(), s.seasonID.ToString());
            NoGamesGrid = false;
            NextGame = null;
            PreviousGame = null;
            NextGameCategories = null;
            PreviousGameCategories = null;
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                if (response.games.Count > 0)
                {
                    List<Game> sortedGames = new List<Game>();
                    foreach (Game game in response.games)
                    {
                        sortedGames.Add(game);
                    }
                    sortedGames.Sort((x, y) => DateTime.Compare(y.date, x.date));//most recent to least recent
                    DateTime lastGameDate = sortedGames[0].date;

                    if (DateTime.Compare(DateTime.Now, lastGameDate) >= 0)
                    {
                        NextGame = sortedGames[0];
                        if (sortedGames.Count >= 2)
                        {
                            PreviousGame = sortedGames[1];
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
                                    NextGame = sortedGames[i];
                                }
                                else
                                {
                                    NextGame = sortedGames[i - 1];
                                    PreviousGame = sortedGames[i];
                                }
                                break;
                            }
                        }
                    }
                }
                if (NextGame != null)
                {
                    CategoryResponse catResponse = await ServiceAccessor.GetGameCategories(NextGame.gameId.ToString());
                    if (catResponse.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        NextGameCategories = catResponse.categories;
                        NextGame.categories = NextGameCategories;
                    }
                    else//could better handle exceptions
                    {
                        //Common.APIExceptionDialog.ShowGeneralExceptionDialog(null, null);
                        NextGameCategories = null;
                    }
                }
                if (PreviousGame != null)
                {
                    CategoryResponse catResponse = await ServiceAccessor.GetGameCategories(PreviousGame.gameId.ToString());
                    if (catResponse.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        PreviousGameCategories = catResponse.categories;
                        PreviousGame.categories = PreviousGameCategories;
                    }
                    else//could better handle exceptions
                    {
                        PreviousGameCategories = null;
                    }
                }
                if (PreviousGame == null && NextGame == null)
                {
                    NoGamesGrid = true;
                    RecentWeeks_GridVisibility = "Collapsed";
                }
                else {
                    RecentWeeks_GridVisibility = "Visible";
                }

            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                navigationService.NavigateToViewModel<DownloadsViewModel>();
            }
            else//could better handle exceptions
            {
                NextGame = null;
            }
        }

        internal void SeasonSelected(object p)
        {
            var selectedSeason = (Season)p;
            UpdateParameterOnSeasonChange();
            AppDataAccessor.SetTeamContext(selectedSeason.seasonID, selectedSeason.owningTeam.teamID);
            FindNextPreviousGames(selectedSeason);
        }

        public void NextCategorySelected(ItemClickEventArgs eventArgs)
        {

            if (ServiceAccessor.ConnectedToInternet())
            {
                var category = (Category)eventArgs.ClickedItem;
                UpdateCachedParameter();
                CachedParameter.categoryId = category.categoryId;
                CachedParameter.gameId = NextGame.gameId;
                navigationService.NavigateToViewModel<SectionViewModel>();
            }
            else
            {
                APIExceptionDialog.ShowNoInternetConnectionDialog(null, null);
            }

        }

        public void PreviousCategorySelected(ItemClickEventArgs eventArgs)
        {

            if (ServiceAccessor.ConnectedToInternet())
            {
                var category = (Category)eventArgs.ClickedItem;
                UpdateCachedParameter();
                CachedParameter.categoryId = category.categoryId;
                CachedParameter.gameId = PreviousGame.gameId;
                navigationService.NavigateToViewModel<SectionViewModel>();
            }
            else
            {
                APIExceptionDialog.ShowNoInternetConnectionDialog(null, null);
            }

        }

        public async void LastViewedSelected()
        {
            if (ServiceAccessor.ConnectedToInternet() || lastViewedLocal)
            {
                if (lastViewedId != null)
                {
                    ProgressRingVisibility = "Visible";

                    ClipResponse response = await loadLastViewed;
                    if (response.status == SERVICE_RESPONSE.SUCCESS)
                    {
                        lastViewedCutup.Clips = response.clips;
                        UpdateCachedParameter();
                        CachedParameter.selectedCutup = new Cutup
                        {
                            cutupId = lastViewedCutup.CutupId,
                            clips = lastViewedCutup.Clips,
                            displayColumns = lastViewedCutup.DisplayColumns,
                            clipCount = lastViewedCutup.ClipCount,
                            name = lastViewedCutup.Name,
                            thumbnailLocation = lastViewedCutup.Thumbnail
                        };
                        navigationService.NavigateToViewModel<VideoPlayerViewModel>();
                    }

                    ProgressRingVisibility = "Collapsed";
                }
                else
                {
                    UpdateCachedParameter();
                    navigationService.NavigateToViewModel<SectionViewModel>();
                }
            }
            else
            {
                APIExceptionDialog.ShowNoInternetConnectionDialog(null, null);
            }
        }

        public void ViewDownloads()
        {
            UpdateCachedParameter();
            navigationService.NavigateToViewModel<DownloadsViewModel>();
        }

        
    }
}