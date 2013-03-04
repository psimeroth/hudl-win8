using Caliburn.Micro;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Models
{
    public class Model
    {
        public BindableCollection<Team> teams { get; set; }

        public Model()
        {
            teams = new BindableCollection<Team>();
        }
    }

    public class SectionModel
    {
        public BindableCollection<Game> games { get; set; }

        public SectionModel()
        {
            games = new BindableCollection<Game>();
        }
    }

    public class Team
    {
        public string school { get; set; }
        public string name { get; set; }
        public string teamID { get; set; }
        public BindableCollection<Season> seasons { get; set; }

        public Team ()
        {
            seasons = new BindableCollection<Season>();
        }

        public static Team FromDTO(TeamDTO teamDTO)
        {
            Team team = new Team();
            team.name = teamDTO.Name;
            team.school = teamDTO.School.Name;
            team.teamID = teamDTO.TeamId;
            foreach (SeasonDTO seasonDTO in teamDTO.Seasons)
            {
                team.seasons.Add(Season.FromDTO(seasonDTO, team));
            }
            return team;
        }
    }

    public class Season : IComparable
    {
        public string name { get; set; }
        public string SchoolName 
        { 
            get 
            { 
                return owningTeam.school; 
            }  
        }

        public string TeamName
        {
            get
            {
                return owningTeam.name;
            }
        }

        public string seasonId { get; set; }
        public int year { get; set; }
        public BindableCollection<Game> games { get; set; }
        public Team owningTeam { get; set; }
        public int CompareTo(object obj)
        {
            Season season = obj as Season;
            if (season == null)
            {
                throw new ArgumentException("Object is not a Season");
            }
            return this.year.CompareTo(season.year);
        }

        public Season()
        {
            games = new BindableCollection<Game>();
        }

        public Season(CategoryDTO cat, Team team)
        {
            seasonId = cat.CategoryId;
            owningTeam = team;

            //this method of making a season doesn't have a year, so we just choose the first year listed in the name
            int yearInt;
            int.TryParse(cat.Name.Substring(0, 4), out yearInt);
            year = yearInt;
            
            name = cat.Name;
            games = new BindableCollection<Game>();
            foreach (CategoryDTO subCat in cat.SubCategories)
            {
                games.Add(Game.FromDTO(subCat));
            }

        }

        public static Season FromDTO(SeasonDTO seasonDTO, Team team)
        {
            Season s = new Season();
            s.owningTeam = team;
            s.name = seasonDTO.Name;
            s.seasonId = seasonDTO.SeasonId;
            s.year = seasonDTO.Year;
            return s;
        }
    }

    public class Game
    {
        public string opponent { get; set; }
        public DateTime date { get; set; }
        public BindableCollection<Category> categories { get; set; }
        public string gameId { get; set; }
        public string Classification { get; set; }
        public string DisplayDate
        {
            get
            {
                return date.ToString("g");
            }
        }

        public Game()
        {
            categories = new BindableCollection<Category>();
        }
        
        public static Game FromDTO(CategoryDTO gameDTO)
        {
            Game game = new Game();
            game.Classification = gameDTO.Classification;
            string[] oppAndDate = gameDTO.Name.Split('-'); //in the call we're using to return this information, opponent and date are concatenated.
            game.gameId = gameDTO.CategoryId;
            game.opponent = oppAndDate[0].Trim();
            if (gameDTO.Classification == "1")
            {
                string[] date = oppAndDate[1].Split('/');
                int year;
                int month;
                int day;
                int.TryParse(date[2], out year);
                int.TryParse(date[1], out day);
                int.TryParse(date[0], out month);
                game.date = new DateTime(year, month, day);
            }
            foreach(CategoryDTO cat in gameDTO.SubCategories)
            {
                game.categories.Add(Category.FromDTO(cat));
            }
            return game;
        }
    }

    public class HubPageGameEntry
    {
        public string Name {get; set;}
        public List<Game> Games {get; set;}

        public HubPageGameEntry()
        {
            Games = new List<Game>();
            Name = "";
        }
    }

    public class Category
    {
        public string name { get; set; }
        public BindableCollection<Playlist> playlists { get; set; }
        public string categoryId { get; set; }

        public Category()
        {
            playlists = new BindableCollection<Playlist>();
        }

        public static Category FromDTO(CategoryDTO categoryDTO)
        {
            Category category = new Category();
            category.categoryId = categoryDTO.CategoryId;
            category.name = categoryDTO.Name;
            return category;
        }
    }

    public class Downloads
    {
        public BindableCollection<Playlist> playlists { get; set; }

        public Downloads()
        {
            playlists = new BindableCollection<Playlist>();
        }
    }

    public class Playlist
    {
        public string name { get; set; }
        public int clipCount { get; set; }
        public string playlistId { get; set; }
        public BindableCollection<Clip> clips { get; set; }
        public string[] displayColumns { get; set; }
        public string downloadedThumbnailLocation { get; set; }
        private string thumnaillocation;
        public string thumbnailLocation
        {
            get
            {
                return downloadedThumbnailLocation != null ? downloadedThumbnailLocation : thumnaillocation;
            }
            set
            {
                thumnaillocation = value;
            }
        }
        public long totalFilesSize { get; set; }
        public DateTime downloadedDate { get; set; }
        public Playlist()
        {
            clips = new BindableCollection<Clip>();
        }

        public static Playlist Copy(Playlist toCopy)
        {
            Playlist playlist = new Playlist { playlistId = toCopy.playlistId, clipCount = toCopy.clipCount, name = toCopy.name, thumbnailLocation = toCopy.thumbnailLocation, displayColumns = toCopy.displayColumns, totalFilesSize = toCopy.totalFilesSize };
            BindableCollection<Clip> clips = new BindableCollection<Clip>();
            foreach (Clip c in toCopy.clips)
            {
                Clip clip = new Clip { breakDownData = c.breakDownData, clipId = c.clipId, order = c.order };
                BindableCollection<Angle> angles = new BindableCollection<Angle>();
                foreach (Angle a in c.angles)
                {
                    Angle angle = new Angle { angleName = a.angleName, angleType = null, clipAngleId = a.clipAngleId, duration = a.duration, fileLocation = a.fileLocation, isPreloaded = a.isPreloaded, preloadFile = a.preloadFile, thumbnailLocation = a.thumbnailLocation };
                    angles.Add(angle);
                }
                clip.angles = angles;
                clips.Add(clip);
            }
            playlist.clips = clips;
            return playlist;
        }

        public static Playlist FromDTO(PlaylistDTO playlistDTO)
        {
            Playlist playlist = new Playlist();
            playlist.playlistId = playlistDTO.PlaylistId;
            playlist.clipCount = playlistDTO.ClipCount;
            playlist.name = playlistDTO.Name;
            playlist.thumbnailLocation = playlistDTO.Thumbnailpath;
            return playlist;
        }
    }

    public class Clip
    {
        public string clipId { get; set; }
        public long order { get; set; }
        public BindableCollection<Angle> angles { get; set; }
        public string[] breakDownData { get; set; }




        public Clip()
        {
            //breakdownData = new Dictionary<string, string>();
            angles = new BindableCollection<Angle>();
        }

        public static Clip FromDTO(ClipDTO clipDTO, string[] displayColumns)
        {
            Clip clip = new Clip();
            clip.breakDownData = new string[displayColumns.Length];
            clip.clipId = clipDTO.ClipID;
            clip.order = clipDTO.OriginalOrder;
            for (int i = 0; i < clip.breakDownData.Length; i++)
            {
                clip.breakDownData[i] = "-";
            }
            foreach(string key in clipDTO.breakdownData.Keys)
            {
                for (int i = 0; i < displayColumns.Length; i++)
                {
                    if (key.ToUpper() == displayColumns[i].ToUpper())
                    {
                        clip.breakDownData[i] = clipDTO.breakdownData[key];
                    }
                }
            }
            //clip.breakdownData = upperedBDD;
            foreach (AngleDTO angleDTO in clipDTO.ClipAngles)
            {
                Angle a = Angle.FromDTO(angleDTO);
                if (a != null)
                {
                    clip.angles.Add(a);
                }
            }
            return clip;
        }
    }

    public class Angle
    {
        public string clipAngleId { get; set; }
        public string angleName { get; set; }
        public string fileLocation { get; set; }
        public string thumbnailLocation { get; set; }
        public long duration { get; set; }
        public AngleType angleType { get; set; }
        public bool isPreloaded { get; set; }
        public string preloadFile { get; set; }

        public Angle()
        {

        }
        public Angle(string clipAngleId, string fileLocation)
        {
            this.fileLocation = fileLocation;
            this.clipAngleId = clipAngleId;
        }

        public static Angle FromDTO(AngleDTO angleDTO)
        {
            if (angleDTO.Files.Any())
            {
                Angle angle = new Angle();
                angle.angleName = angleDTO.AngleName;
                angle.clipAngleId = angleDTO.ClipAngleID;
                angle.duration = angleDTO.Duration;
                if (angleDTO.Files.FirstOrDefault() != null)
                {
                    angle.fileLocation = angleDTO.Files.FirstOrDefault().FileName;//throws error if there is no filename
                }
                else
                {
                    return null;
                }
                angle.thumbnailLocation = angleDTO.LargeThumbnailFileName;
                angle.isPreloaded = false;
                return angle;
            }
            else
            {
                return null;
            }
        }
    }

    public class BreakdownData
    {
        public float dist { get; set; }
        public long play { get; set; }//needs to be changed
        public long qtr { get; set; }
    }

    public class AngleType : PropertyChangedBase
    {
        public VideoPlayerViewModel viewModel { get; set; }
        public string Name { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        public AngleType(string name, VideoPlayerViewModel vm)
        {
            Name = name;
            IsChecked = true;
            viewModel = vm;
        }

        public void checkBox(bool isChecked, Windows.UI.Xaml.RoutedEventArgs eventArgs)
        {
            viewModel.AngleFilter();
        }
    }

    public enum PlaybackType
    {
        once = 0,
        loop = 1,
        next = 2
    }

    public enum NextAngleEvent
    {
        buttonClick = 0,
        mediaEnded = 1
    }
}
