using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using Windows.Storage;

namespace HudlRT.Common
{
    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Class used make API calls.
    /// </summary>
    class ServiceAccessor
    {
        private const string URL_BASE = "http://thor3/api/";
        private const string URL_BASE_SECURE = "https://thor3/api/";

        public const string URL_SERVICE_LOGIN = "login";
        public const string URL_SERVICE_GET_TEAMS = "teams";
        public const string URL_SERVICE_GET_SCHEDULE = "teams/#/schedule";//returns games
        public const string URL_SERVICE_GET_CATEGORIES_FOR_GAME = "games/#/categories";//returns categories
        public const string URL_SERVICE_GET_CUTUPS_BY_CATEGORY = "categories/#/playlists";//returns cutups
        public const string URL_SERVICE_GET_CLIPS = "playlists/#/clips";//returns clips

        /// <summary>
        /// Makes an API call to the base URL defined in AppData.cs using the GET method.
        /// </summary>
        /// <param name="url">The API function to hit.</param>
        /// <param name="jsonString">Any necesary data required to make the call.</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallGet(string url)
        {
            var req = (HttpWebRequest) WebRequest.Create(URL_BASE + url);
            req.ContentType = "application/json";
            req.Method = "GET";

            // Get the auth token from the App Data and make sure it's not null
            var authtoken = ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"];
            if (authtoken != null)
            {
                req.Headers["hudl-authtoken"] = (string) authtoken;
            }

            using (var response = await req.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream);
                    var answer = reader.ReadToEnd();
                    return answer;
                }
            }
        }

        /// <summary>
        /// Makes an API call to the base URL defined in AppData.cs using the POST method.
        /// </summary>
        /// <param name="url">The API function to hit.</param>
        /// <param name="jsonString">Any necesary data required to make the call.</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallPost(string url, string jsonString)
        {
            var req = (HttpWebRequest)WebRequest.Create(URL_BASE_SECURE + url);
            req.ContentType = "application/json";
            req.Method = "POST";

            using (var requestStream = await req.GetRequestStreamAsync())
            {
                var writer = new StreamWriter(requestStream);
                writer.Write(jsonString);
                writer.Flush();
            }

            using (var response = await req.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream);
                    var answer = reader.ReadToEnd();
                    return answer;
                }
            }
        }
    }
}
