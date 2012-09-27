using Caliburn.Micro;
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

        public void setTeams(BindableCollection<TeamDTO> teamDTOs)// does not currently nullcheck
        {
            //call serviceAccessor and make a get method?

            BindableCollection<Team> teams = new BindableCollection<Team>();
            foreach(TeamDTO tDTO in teamDTOs)
            {
                Team team = new Team();
                team.name = tDTO.Name;
                team.school = tDTO.School.Name;
                team.teamID = tDTO.TeamId;
                team.seasons = new BindableCollection<Season>();
                foreach(SeasonDTO sDTO in tDTO.Seasons)
                {
                    Season s = new Season();
                    s.owningTeam = team;
                    s.name = sDTO.Name;
                    s.seasonID = sDTO.SeasonId;
                    s.year = sDTO.Year;
                    team.seasons.Add(s);
                }
                teams.Add(team);
            }
            this.teams = teams;
        }

    }

    public class Team
    {
        public string school { get; set; }
        public string name { get; set; }
        public long teamID { get; set; }
        public BindableCollection<Season> seasons { get; set; }
    }

    public class Season
    {
        public string name { get; set; }
        public long seasonID { get; set; }
        public int year { get; set; }
        public BindableCollection<Game> games { get; set; }
        public Team owningTeam { get; set; }
        public void setGames(BindableCollection<GameDTO> gameDTOs)//assuming these are only games for this season
        {
            BindableCollection<Game> games = new BindableCollection<Game>();
            foreach (GameDTO gDTO in gameDTOs)
            {
                Game game = new Game();
                game.gameId = gDTO.GameId;
                game.isHome = gDTO.Ishome;
                game.opponent = gDTO.Opponent;
                game.date = gDTO.Date;
                games.Add(game);
            }
            this.games = games;
        }
    }

    public class Game
    {
        public string opponent { get; set; }
        public DateTime date { get; set; }
        public bool isHome { get; set; }
        public BindableCollection<Category> categories { get; set; }
        public long gameId { get; set; }

        public void setCategories(BindableCollection<CategoryDTO> categoryDTOs)
        {
            BindableCollection<Category> categories = new BindableCollection<Category>();
            foreach (CategoryDTO cDTO in categoryDTOs)
            {
                Category category = new Category();
                category.name = cDTO.Name;
                categories.Add(category);
                category.categoryId = cDTO.CategoryId;
            }
            this.categories = categories;
        }
    }

    public class Category
    {
        public string name { get; set; }
        public List<Cutup> cutups { get; set; }
        public long categoryId { get; set; }

        public void setCutups(List<CutupDTO> cutupDTOs)
        {
            List<Cutup> cutups = new List<Cutup>();
            foreach (CutupDTO cDTO in cutupDTOs)
            {
                Cutup cutup = new Cutup();
                cutup.clipCount = cDTO.ClipCount;
                cutup.name = cDTO.Name;
                cutups.Add(cutup);
            }
            this.cutups = cutups;
        }
    }

    public class Cutup
    {
        public string name { get; set; }
        public int clipCount { get; set; }
        public List<Clip> clips { get; set; }

        public void setClips(List<ClipDTO> clipDTOs)
        {
            List<Clip> clips = new List<Clip>();
            foreach (ClipDTO cDTO in clipDTOs)
            {
                
                foreach (AngleDTO aDTO in cDTO.Angles)
                {
                    Clip clip = new Clip();
                    clip.angleName = aDTO.AngleName;
                    clip.clipAngleId = aDTO.ClipAngleID;
                    clip.duration = aDTO.Duration;
                    clip.fileLocation = aDTO.Files.FirstOrDefault().FileName;//multiple files for a single clip?
                    //clip.name do clips have names?
                    clip.thumbnailLocation = aDTO.LargeThumbnailFileName;
                    clips.Add(clip);
                }

                
            }
            this.clips = clips;
        }
    }

    public class Clip
    {
        public string name { get; set; }
        public long clipAngleId { get; set; }
        public string angleName { get; set; }
        public string fileLocation { get; set; }
        public string thumbnailLocation { get; set; }
        public long duration { get; set; }
        public List<BreakdownData> breakdownData { get; set; }
    }
    public class BreakdownData
    {
        public float dist { get; set; }
        public long play { get; set; }//needs to be changed
        public long qtr { get; set; }
    }
}
