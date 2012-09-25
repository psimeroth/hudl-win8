using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Models
{
    class Models
    {
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }

    public class Team : INotifyPropertyChanged
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public bool IsExchangeOnly { get; set; }
        public School School { get; set; }
        public List<Season> Seasons { get; set; }
        public string Role { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class School
    {
        public int SchoolId { get; set; }
        public string Name { get; set; }
        public string AbbreviatedName { get; set; }
    }

    public class Season
    {
        public int SeasonId { get; set; }
        public string Name { get; set; }
        public bool IsCurrentSeason { get; set; }
        public int Year { get; set; }
    }

    public class PassToSplit
    {
        public int TeamID { get; set; }
        public string FileLocation { get; set; }
        public string Token { get; set; }
    }

    public class Game
    {
        public int GameId { get; set; }
        public DateTime Date { get; set; }
        public string Opponent { get; set; }
        public bool Ishome { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public int Classification { get; set; }
        public string Name { get; set; }
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
