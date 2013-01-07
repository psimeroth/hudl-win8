using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Models
{
    class DTOModels
    {
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

    public class TeamDTO
    {
        public string TeamId { get; set; }
        public string Name { get; set; }
        public bool IsExchangeOnly { get; set; }
        public SchoolDTO School { get; set; }
        public List<SeasonDTO> Seasons { get; set; }
        public string Role { get; set; }
    }

    public class SchoolDTO
    {
        public string SchoolId { get; set; }
        public string Name { get; set; }
        public string AbbreviatedName { get; set; }
    }

    public class SeasonDTO
    {
        public string SeasonId { get; set; }
        public string Name { get; set; }
        public bool IsCurrentSeason { get; set; }
        public int Year { get; set; }
    }

    public class GameDTO
    {
        public string GameId { get; set; }
        public DateTime Date { get; set; }
        public string Opponent { get; set; }
        public bool Ishome { get; set; }
        public List<CategoryDTO> Categories { get; set; }
        public string seasonID { get; set; }//this would be ideal
    }

    public class CategoryDTO
    {
        public string CategoryId { get; set; }
        public int Classification { get; set; }
        public string Name { get; set; }
    }

    public class CutupDTO
    {
        public string PlaylistId { get; set; }
        public string CategoryID { get; set; }
        public string Name { get; set; }
        public string CreatorName { get; set; }
        public int ClipCount { get; set; }
        public string Thumbnailpath { get; set; }
        public CategoryDTO AssociatedCategory { get; set; }
    }

    public class ClipResponseDTO
    {
        public ClipsListDTO ClipsList { get; set; }
        public string[] DisplayColumns { get; set; }
    }

    public class ClipsListDTO
    {
        public List<ClipDTO> Clips { get; set; }
        public long Total { get; set; }
    }

    public class ClipDTO
    {
        public string ClipID { get; set; }
        public Dictionary<string, string> breakdownData { get; set; }

        public List<AngleDTO> ClipAngles { get; set; }
        public long OriginalOrder { get; set; }
    }


    public class AngleDTO
    {
        public string ClipAngleID { get; set; }
        public string AngleName { get; set; }
        public long Duration { get; set; }
        public string ThumbnailFileName { get; set; }
        public string LargeThumbnailFileName { get; set; }
        public List<FileDTO> Files { get; set; }//shouldn't this just be one file
    }

    public class FileDTO
    {
        public long Quality { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }

    public class FeaturePrivilege
    {
        public string FeaturePrivilegeId { get; set; }
        public string Name { get; set; }
        public bool IsLabs { get; set; }
    }
}
