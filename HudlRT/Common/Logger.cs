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
            public Function Function { get; set; }
            public Operation Operation { get; set; }
            public string Error { get; set; }
            public string Method { get; set; }
            public ErrorLevel ErrorLevel { get; set; }
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

        public void LogLastViewedClick()
        {
            LogEntry entry = new LogEntry();
            entry.Function = Function.Click;
            entry.Operation = Operation.Cutup;
            entry.Error = null;
            entry.Method = "Last Viewed Button Clicked.";
            entry.ErrorLevel = ErrorLevel.Info;
            entry.Attributes = JsonConvert.SerializeObject(entry.AttributesDictionary);
            entry.AttributesDictionary = null;

            ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOG, JsonConvert.SerializeObject(entry), false);
        }
    }
}
