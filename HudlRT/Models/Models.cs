using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Model
{
    class Models
    {
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }

    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TeamResponse
    {
        public int teamId { get; set; }
        public string name { get; set; }
        public bool isExchangeOnly { get; set; }
        public School school { get; set; }
        public List<Season> seasons { get; set; }
        public string role { get; set; }
    }

    public class School
    {
        public int schoolId { get; set; }
        public string name { get; set; }
        public string abbreviatedName { get; set; }
    }

    public class Season
    {
        public int seasonId { get; set; }
        public string name { get; set; }
        public bool isCurrentSeason { get; set; }
        public int year { get; set; }
    }

    public class PassToSplit
    {
        public int teamID { get; set; }
        public string fileLocation { get; set; }
        public string Token { get; set; }
    }

    public class Game
    {
        public int gameId { get; set; }
        public DateTime date { get; set; }
        public string opponent { get; set; }
        public bool ishome { get; set; }
        public List<Category> categories { get; set; }
    }

    public class Category
    {
        public int categoryId { get; set; }
        public int classification { get; set; }
        public string name { get; set; }
    }

    public class Cutup
    {
        public int cutupID { get; set; }
        public int categoryID { get; set; }
        public string name { get; set; }
        public string creatorName { get; set; }
        public int clipCount { get; set; }
        public string thumbnailpath { get; set; }
        public Category associatedCategory { get; set; }
    }

    public class ClipResponse
    {
        public ClipsList clipsList { get; set; }
        public string[] displayColumns { get; set; }
    }

    public class ClipsList
    {
        public List<Clip> clips { get; set; }
        public long total { get; set; }
    }

    public class Clip
    {
        public long clipID { get; set; }
        public BreakDownData breakdownData { get; set; }

        public List<Angle> angles { get; set; }
        public long originalOrder { get; set; }
    }

    public class BreakDownData
    {
        public float dIST { get; set; }
        public long pLAY { get; set; }//needs to be changed
        public long qTR { get; set; }
    }

    public class Angle
    {
        public long clipAngleID { get; set; }
        public string angleName { get; set; }
        public long duration { get; set; }
        public string thumbnailFileName { get; set; }
        public string largeThumbnailFileName { get; set; }
        public List<File> files { get; set; }
    }

    public class File
    {
        public long quality { get; set; }
        public string fileName { get; set; }
        public long fileSize { get; set; }
    }
}
