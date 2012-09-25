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

namespace App5.Common
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
        public const string URL_BASE = "http://thor3/api/";
        public const string URL_BASE_SECURE = "https://thor3/api/";

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
        /// <param name="header">The header for the JSON request. (optional)</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallGet(string url, string jsonString, string header)
        {
            var req = (HttpWebRequest) WebRequest.Create(url);
            req.ContentType = "application/json";
            req.Method = "GET";

            var authtoken = (string) ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"];
            req.Headers["hudl-authtoken"] = authtoken;

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
        /// <param name="header">The header for the JSON call. (optional)</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallPost(string url, string jsonString)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
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
