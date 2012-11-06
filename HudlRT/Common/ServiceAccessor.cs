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
#if DEBUG
        private const string URL_BASE = "http://thor7/api/";
        private const string URL_BASE_SECURE = "https://thor7/api/";
#else
        private const string URL_BASE = "http://www.hudl.com/api/";
        private const string URL_BASE_SECURE = "https://www.hudl.com/api/";
#endif
        public const string URL_SERVICE_LOGIN = "login";
        public const string URL_SERVICE_GET_TEAMS = "teams";
        public const string URL_SERVICE_GET_SCHEDULE = "teams/#/schedule";//returns games
        public const string URL_SERVICE_GET_SCHEDULE_BY_SEASON = "teams/#/schedule?season=%";//returns games
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
            try
            {
                var httpClient = new HttpClient();
                Uri uri = new Uri(URL_BASE + url);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMessage.Headers.Add("hudl-authtoken", ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"].ToString());
                httpRequestMessage.Headers.Add("User-Agent", "HudlWin8/1.0.0");
                var response = await httpClient.SendAsync(httpRequestMessage);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "";//how to handle exceptions?
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
            try
            {
                var httpClient = new HttpClient();
                Uri uri = new Uri(URL_BASE_SECURE + url);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                httpRequestMessage.Headers.Add("User-Agent", "HudlWin8/1.0.0");
                httpRequestMessage.Content = new StringContent(jsonString);
                httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await httpClient.SendAsync(httpRequestMessage);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
