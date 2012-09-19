﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App5.Common
{
    static class AppData
    {
        public const string LOGIN_USERNAME = "jacobataylor09@gmail.com";
        public const string LOGIN_PASSWORD = "abcd";

        public const string URL_BASE = "http://thor3/api/";
        public const string URL_BASE_SECURE = "https://thor3/api/";

        public const string URL_SERVICE_LOGIN = "login";
        public const string URL_SERVICE_GET_TEAMS = "teams";
        public const string URL_SERVICE_GET_SCHEDULE = "teams/#/schedule";
        public const string URL_SERVICE_GET_CUTUPS_BY_CATEGORY = "categories/#/playlists";
        public const string URL_SERVICE_GET_CLIPS = "playlists/#/clips";

    }
}