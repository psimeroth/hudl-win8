using HudlRT.Models;
using HudlRT.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Common
{
    class Logger
    {
        public const string LOG_GAME_NEXT = "NextGame";
        public const string LOG_GAME_PREVIOUS = "PreviousGame";

        public enum ErrorLevel
        {
            Audit = 0,
            Info = 1,
            Debug = 2,
            Warn = 3,
            Error = 4,
            Fatal = 5
        }

        public enum Function
        {
            View,
            Load,
            Insert,
            Rename,
            Update,
            Delete,
            Merge,
            Enable,
            Disable,
            Corrupt,
            Share,
            Unshare,
            Login,
            AccountSetup,
            Order,
            ReferAFriend,
            Send,
            Import,
            Upload,
            Search,
            Invite,
            Export,
            Link,
            Unlink,
            Index,
            Download,
            Select,
            Repair,
            Apply,
            Create,
            Start,
            Paid,
            Lookup,
            Decide,
            Copy,
            Cancel,
            Refresh,
            AutoMatch,
            TemplateAutoMatch,
            ManualSearch,
            SearchTemplateMatch,
            Facebook,
            List,
            Click,
            Redirect,
        }

        public enum Operation
        {
            AutoAdvance,
            AcademicDocument,
            Attachment,
            SavedAngles,
            CopyCutup,
            MoveCutup,
            ReorderCutup,
            AddClipsToCutup,
            AddCutupClipsToCutup,
            Game,
            TeamPage,
            LibraryPage,
            LibraryManagementPage,
            GamePage,
            SchoolPage,
            HomePage,
            AthletePage,
            Clip,
            PresentationClip,
            HighlightClip,
            HighlightClips,
            HighlightReel, // General reel log - prefer using TopPlay, PremiumHighlight, or GameHighlight when possible.
            TopPlay,
            PremiumHighlight,
            GameHighlight,
            Annotation,
            Annotations,
            Cutup,
            ClipList,
            Report,
            CustomReport,
            PrintReport,
            ReportTemplate,
            Presentation,
            FormationDiagram,
            Library,
            AllCutupClips,
            CutupClips,
            Profile,
            ChangePassword,
            Avatar,
            Group,
            GroupsManagementPage,
            ClipData,
            ClipDataColumn,
            Team,
            School,
            SchoolAdmin,
            Filter,
            FilterData,
            OverlayColumns,
            ExchangeFilm,
            AddTeamByAdmin,
            AddTeamByCoach,
            AddTeamByAdminOrCoach,
            Media, // means either a playlist, report, or (eventually) presentation... anything in the library tree
            NormalLogin,
            OverrideLogin,
            ReferAFriend,
            PublishClipAngle, // previously PublishClipFile
            Recruiter,
            RecruitingPackage,
            User,
            Oppponent,
            OppponentParticipant,
            Participant,
            SubstituteParticipant,
            Participation,
            RewardInfo,
            RewardCode,
            MarkPaidSchool,
            MarkPaidAccount,
            MarkUnpaidAccount,
            RecruitedAthlete, // this pertains to the RecruitedAthlete for recruiting boards and inboxes
            RecruitingPackageAthlete, // this is for recruiting packages
            ReAddRecruiterAthlete,
            Invoice,
            BillingAccount,
            Message,
            Configuration,
            MaxPrepsTeamPage,
            MaxPreps,
            PublicHighlights,
            PublicAngles,
            Dvd,
            DvdOrder,
            Slide,
            Season,
            CurrentSeason,
            Coach,
            BaseUri,
            Role,
            TeamAndSeasons,
            ClipAngleUri, // previously ClipFileUri
            ContentService,
            ClipContent,
            Schedule,
            Category,
            Opponent,
            OpponentParticipant,
            CDNCache,
            TeamHighlightTree,
            PreferredColumns,
            ColumnSet,
            ResetPassword,
            ClipDataKnownValue,
            ClipDataValueRange,
            Image,
            Roster,
            Reward,
            PrivateStorage,
            Remote,
            Achievement,
            NewAchievementLevel,
            VideoQuality,
            Announcement,
            OriginalSourceVideo,
            MaxPrepsAthletePage,
            MaxPrepsHighlightClip,
            MoveReport, MovePresentation,
            FeedItem,
            PayInvoice,
            UpdateRecurringPaymentAccount,
            CurrentVersion,
            FeatureTeamPrivilege,
            FeatureTemplate,
            SupportCase,
            SendSms,
            SendEmail,
            DownloadPlaylist,
            DownloadHighlight,
            Audio,
            CopyClips,
            TeamAllotment,
            Fundraising,
            ExchangePool,
            GameView,
            RecruiterTeam,
            CutupWithoutGame,
            LightningLibrary,
            FilmStudyCategory,
            Inbox,
            ClipRating,
            UpgradeInvoice,
            RequestUpgradeQuote,
            IntercutPlaylist,
            CreateClips,
            HighlightReels,
            HighlightMusic,
            HighlightAuxInfo,
            ClipsForTeams,
            PlayCard,
            PracticeScript,
            BackfieldDiagram,
            ReScrape,
            RecruitAthleteIndex,
            RankAthlete,
            Video,
            BillingRepresentative,
            SalesTerritory,
            Event,
            Marks,
            UnmatchedMarks,
            Whistles,
            ReviewListItem,
            MaxPrepsRoster,
            MaxPrepsSchedule,
            MaxPrepsTeam,
            Suggestion,
            ReviewList,
            Elecard,
            ClipAngle,
            Facets,
            PossessionSplitter,
            AthleteProfile
        }

        public class LogEntry
        {
            public string Function { get; set; }
            public string Operation { get; set; }
            public string Error { get; set; }
            public string Method { get; set; }
            public string ErrorLevel { get; set; }
            public string Attributes { get; internal set; }
            public Dictionary<string, object> AttributesDictionary { get; set; }
        }

        private static readonly Lazy<Logger> logger = new Lazy<Logger>(() => new Logger());

        public static Logger Instance
        {
            get { return logger.Value; }
        }

        private Logger()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IsoDateTimeConverter());
            settings.Converters.Add(new StringEnumConverter());
            settings.NullValueHandling = NullValueHandling.Ignore;
        }

        public async void LogLastViewedClick(Playlist playlist)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Click.ToString();
            entry.Operation = Operation.Cutup.ToString();
            entry.Error = null;
            entry.Method = "LastViewed";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "LastViewed");
            entry.AttributesDictionary.Add("Name", playlist.name);
            entry.AttributesDictionary.Add("Playlist", playlist.playlistId);
            entry.AttributesDictionary.Add("ClickedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogGameSelected(Game game, string description = "GameSelected")
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Click.ToString();
            entry.Operation = Operation.Game.ToString();
            entry.Error = null;
            entry.Method = description;
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", description);
            entry.AttributesDictionary.Add("Game", game.gameId);
            entry.AttributesDictionary.Add("ClickedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogPlaylistSelected(Playlist playlist)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Click.ToString();
            entry.Operation = Operation.Cutup.ToString();
            entry.Error = null;
            entry.Method = "PlaylistSelected";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "PlaylistSelected");
            entry.AttributesDictionary.Add("Playlist", playlist.playlistId);
            entry.AttributesDictionary.Add("ClickedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogSeasonChanged(Season season)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Click.ToString();
            entry.Operation = Operation.Season.ToString();
            entry.Error = null;
            entry.Method = "SeasonChanged";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "SeasonChanged");
            entry.AttributesDictionary.Add("Season", season.seasonID);
            entry.AttributesDictionary.Add("Year", season.year);
            entry.AttributesDictionary.Add("ClickedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogPlaylistDownloadStart(Playlist playlist)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Download.ToString();
            entry.Operation = Operation.Cutup.ToString();
            entry.Error = null;
            entry.Method = "PlaylistDownloadStart";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "PlaylistDownloadStart");
            entry.AttributesDictionary.Add("Playlist", playlist.playlistId);
            entry.AttributesDictionary.Add("StartedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogPlaylistDownloadComplete(Playlist playlist)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Download.ToString();
            entry.Operation = Operation.Cutup.ToString();
            entry.Error = null;
            entry.Method = "PlaylistDownloadComplete";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "PlaylistDownloadComplete");
            entry.AttributesDictionary.Add("Playlist", playlist.playlistId);
            entry.AttributesDictionary.Add("CompletedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogPlaylistDownloadRemoved(Playlist playlist)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Download.ToString();
            entry.Operation = Operation.Cutup.ToString();
            entry.Error = null;
            entry.Method = "PlaylistDownloadRemoved";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "PlaylistDownloadRemoved");
            entry.AttributesDictionary.Add("Playlist", playlist.playlistId);
            entry.AttributesDictionary.Add("RemovedOn", DateTime.Now);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogSortApplied(FilterViewModel filterVM)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.View.ToString();
            entry.Operation = Operation.ClipDataColumn.ToString();
            entry.Error = null;
            entry.Method = "Sort";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "Sort");
            entry.AttributesDictionary.Add("Sort", filterVM.sortType.ToString());
            entry.AttributesDictionary.Add("Column", filterVM.ColumnHeaderName);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogFilterApplied(List<String> filters)
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.View.ToString();
            entry.Operation = Operation.ClipDataColumn.ToString();
            entry.Error = null;
            entry.Method = "Filter";
            entry.ErrorLevel = ErrorLevel.Info.ToString();

            entry.AttributesDictionary = new Dictionary<string, object>();
            entry.AttributesDictionary.Add("Method", "Filter");
            entry.AttributesDictionary.Add("Filters", filters);

            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallLog(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry));
        }

        public async void LogMediaKeyUse()
        {
        }

        public async void LogKeyUse()
        {
        }
    }
}
