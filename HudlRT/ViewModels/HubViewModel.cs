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
        private readonly INavigationService navigationService;
        public CachedParameter Parameter { get; set; }
        private long? lastViewedId = null;
        private bool lastViewedCached = false;

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
            UpdateCachedParameter();
            navigationService.NavigateToViewModel<SectionViewModel>(Parameter);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Parameter != null)
            {
                SeasonsDropDown = Parameter.seasonsDropDown;
                SelectedSeason = Parameter.seasonSelected;
                if (Parameter.hubViewNextGame != null && Parameter.hubViewPreviousGame != null)
                {
                    NextGame = Parameter.hubViewNextGame;
                    PreviousGame = Parameter.hubViewPreviousGame;
                    NextGameCategories = NextGame.categories;
                    PreviousGameCategories = PreviousGame.categories;
                }
                else
                {
                    FindNextPreviousGames(SelectedSeason);
                }
            }
            else
            {
                PopulateDropDown();
            }

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
                lastViewedId = (long)response.ID;

                LoadLastViewedCutup();
            }

            ColVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        private async Task LoadLastViewedCutup()
        {
            CutupViewModel cutup = new CutupViewModel { CutupId = lastViewedId.Value, Name = LastViewedName };
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                UpdateCachedParameter();
                Parameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = Convert.ToInt32(cutup.ClipCount), name = cutup.Name };
                lastViewedCached = true;
            }
        }

        public void UpdateCachedParameter()
        {
            if (Parameter == null)
            {
                Parameter = new CachedParameter();
            }
            Parameter.seasonsDropDown = SeasonsDropDown;
            Parameter.seasonSelected = SelectedSeason;
            Parameter.hubViewNextGame = NextGame;
            Parameter.hubViewPreviousGame = PreviousGame;
        }

        public void UpdateParameterOnSeasonChange()
        {
            if (Parameter != null)
            {
                Parameter.sectionViewCategories = null;
                Parameter.sectionViewCategorySelected = null;
                Parameter.sectionViewCutups = null;
                Parameter.sectionViewGames = null;
                Parameter.sectionViewGameSelected = null;
                Parameter.selectedCutup = null;
            }
        }

        public async void PopulateDropDown()
        {
            TeamResponse response = await ServiceAccessor.GetTeams();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                Teams = response.teams;
                long teamID = -1;
                long seasonID = -1;
                bool foundSavedSeason = false;
                TeamContextResponse teamContext = AppDataAccessor.GetTeamContext();
                if (teamContext.seasonID != null && teamContext.teamID != null)
                {
                    teamID = (long)teamContext.teamID;
                    seasonID = (long)teamContext.seasonID;
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
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
                        Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
                        Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
            else//could better handle exceptions
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
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
            
            var category = (Category)eventArgs.ClickedItem;
            UpdateCachedParameter();
            Parameter.categoryId = category.categoryId;
            Parameter.gameId = NextGame.gameId;
            navigationService.NavigateToViewModel<SectionViewModel>(Parameter);

        }

        public void PreviousCategorySelected(ItemClickEventArgs eventArgs)
        {

            var category = (Category)eventArgs.ClickedItem;
            UpdateCachedParameter();
            Parameter.categoryId = category.categoryId;
            Parameter.gameId = PreviousGame.gameId;
            navigationService.NavigateToViewModel<SectionViewModel>(Parameter);

        }

        public async void LastViewedSelected()
        {
            if (lastViewedId.HasValue)
            {
                ProgressRingVisibility = "Visible";
                /*if (!lastViewedCached)
                {
                    await LoadLastViewedCutup();
                }*/
                while (!lastViewedCached)
                {
                }
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(Parameter);
                ProgressRingVisibility = "Collapsed";
            }
            else
            {
                UpdateCachedParameter();
                navigationService.NavigateToViewModel<SectionViewModel>(Parameter);
            }
        }

        
    }
}