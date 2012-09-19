﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App5.Common
{
    class Models
    {
    }

    struct LoginProfileDTO
    {
        public string Token { get; set; }
    }

    public class TeamResponse
    {
        public int teamId { get; set; }
        public string name { get; set; }
        public bool isExchangeOnly { get; set; }
        public School school { get; set; }
        public List<Season> seasons { get; set; }
        public string role { get; set; }
        public string Title { get; set; }
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

    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PassToSplit
    {
        public int teamID { get; set; }
        public string Token { get; set; }
        public string Title { get; set; }
    }

    public class Game
    {
        public int gameId { get; set; }
        public DateTime date { get; set; }
        public string opponent { get; set; }
        public bool ishome { get; set; }
        public List<Category> categories { get; set; }

        public string Title { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public int classification { get; set; }
        public string name { get; set; }
        //public List<SubCategory> subcategoryList { get; set; }
    }

    public class Cutup
    {
        public int cutupID { get; set; }
        public int categoryID { get; set; }
        public string name{ get; set; }
        public string creatorName { get; set; }
        public int clipCount { get; set; }
        public string thumbnailpath { get; set; }
        public Category associatedCategory { get; set; }

        public string Title { get; set; }
        public string Image { get; set; }
    }

    public class Clip
    {
        public int ClipID { get; set; }
        //public BreakDownData [] breakDownData { get; set; }
        public string Title { get; set; }
    }

    /*public class BreakDownData
    {
        public float dist { get; set; }
        public int playNum { get; set; }
        public int quarter { get; set; }
    }*/

    public class Group
    {
        public string Title { get; set; }
    }
}