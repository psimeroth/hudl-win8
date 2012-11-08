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
using HudlRT.Models;
using Caliburn.Micro;

namespace HudlRT.Common
{
    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    struct LoginResponse
    {
        public bool success { get; set; }
        public string reason { get; set; }
    }

    struct TeamResponse
    {
        public BindableCollection<Team> teams { get; set; }
        public bool success { get; set; }
        public string reason { get; set; }
    }

    struct GameResponse
    {
        public BindableCollection<Game> games { get; set; }
        public bool success { get; set; }
        public string reason { get; set; }
    }

    struct CategoryResponse
    {
        public BindableCollection<Category> categories { get; set; }
        public bool success { get; set; }
        public string reason { get; set; }
    }

    struct CutupResponse
    {
        public BindableCollection<Cutup> cutups { get; set; }
        public bool success { get; set; }
        public string reason { get; set; }
    }

    struct ClipResponse
    {
        public BindableCollection<Clip> clips { get; set; }
        public bool success { get; set; }
        public string reason { get; set; }
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


        public static async Task<LoginResponse> Login(string loginArgs)
        {
            var loginResponse = await ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOGIN, loginArgs);
            if (!string.IsNullOrEmpty(loginResponse))
            {
                var obj = JsonConvert.DeserializeObject<LoginResponseDTO>(loginResponse);
                ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"] = obj.Token;
                ApplicationData.Current.RoamingSettings.Values["hudl-userId"] = obj.UserId;
                string urlExtension = "privileges/" + obj.UserId.ToString();
                var privilegesResponse = await ServiceAccessor.MakeApiCallGet(urlExtension);
                if (!string.IsNullOrEmpty(privilegesResponse))
                {
                    //Needs to be improved in the future if we want to 
                    if (privilegesResponse.Contains("Win8App"))
                    {
                        return new LoginResponse { success = true, reason = null };
                    }
                    else
                    {
                        return new LoginResponse { success = false, reason = "privilege" };
                    }
                }
                else
                {
                    return new LoginResponse { success = false, reason = "null response" };
                }
            }
            else
            {
                return new LoginResponse { success = false, reason = "credentials" };
            }
        }

        public static async Task<TeamResponse> GetTeams()
        {
            var teams = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_TEAMS);
            if (!string.IsNullOrEmpty(teams))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<TeamDTO>>(teams);
                    BindableCollection<Team> teamCollection = new BindableCollection<Team>();
                    foreach (TeamDTO teamDTO in obj)
                    {
                        teamCollection.Add(Team.FromDTO(teamDTO));
                    }
                    return new TeamResponse { success = true, reason = null, teams = teamCollection};
                }
                catch (Exception)
                {
                    return new TeamResponse { success = false, reason = "deserialization", teams = null };
                }
            }
            else
            {
                return new TeamResponse { success = false, reason = "null response", teams = null };
            }
        }

        public static async Task<GameResponse> GetGames(string teamId, string seasonId)
        {
            var games = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_SCHEDULE_BY_SEASON.Replace("#", teamId).Replace("%", seasonId));
            if (!string.IsNullOrEmpty(games))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<GameDTO>>(games);
                    BindableCollection<Game> gameCollection = new BindableCollection<Game>();
                    foreach (GameDTO gameDTO in obj)
                    {
                        gameCollection.Add(Game.FromDTO(gameDTO));
                    }
                    return new GameResponse { success = true, reason = null, games = gameCollection };
                }
                catch (Exception)
                {
                    return new GameResponse { success = false, reason = "deserialization", games = null };
                }
            }
            else
            {
                return new GameResponse { success = false, reason = "null response", games = null };
            }
        }

        public static async Task<CategoryResponse> GetGameCategories(string gameId)
        {
            var categories = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME.Replace("#", gameId));
            if (!string.IsNullOrEmpty(categories))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<CategoryDTO>>(categories);
                    BindableCollection<Category> categoryCollection = new BindableCollection<Category>();
                    foreach (CategoryDTO categoryDTO in obj)
                    {
                        categoryCollection.Add(Category.FromDTO(categoryDTO));
                    }
                    return new CategoryResponse { success = true, reason = null, categories = categoryCollection };
                }
                catch (Exception)
                {
                    return new CategoryResponse { success = false, reason = "deserialization", categories = null };
                }
            }
            else
            {
                return new CategoryResponse { success = false, reason = "null response", categories = null };
            }
        }

        public static async Task<CutupResponse> GetCategoryCutups(string categoryId)
        {
            var cutups = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY.Replace("#", categoryId));
            if (!string.IsNullOrEmpty(cutups))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<List<CutupDTO>>(cutups);
                    BindableCollection<Cutup> cutupCollection = new BindableCollection<Cutup>();
                    foreach (CutupDTO cutupDTO in obj)
                    {
                        cutupCollection.Add(Cutup.FromDTO(cutupDTO));
                    }
                    return new CutupResponse { success = true, reason = null, cutups = cutupCollection };
                }
                catch (Exception)
                {
                    return new CutupResponse { success = false, reason = "deserialization", cutups = null };
                }
            }
            else
            {
                return new CutupResponse { success = false, reason = "null response", cutups = null };
            }
        }

        public static async Task<ClipResponse> GetCutupClips(Cutup cutup)
        {
            var clips = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CLIPS.Replace("#", cutup.cutupId.ToString()));
            if (!string.IsNullOrEmpty(clips))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
                    BindableCollection<Clip> clipCollection = new BindableCollection<Clip>();
                    cutup.displayColumns = obj.DisplayColumns;
                    foreach (ClipDTO clipDTO in obj.ClipsList.Clips)
                    {
                        Clip c = Clip.FromDTO(clipDTO, obj.DisplayColumns);
                        if (c != null)
                        {
                            clipCollection.Add(c);
                        }
                    }
                    return new ClipResponse { success = true, reason = null, clips = clipCollection };
                }
                catch (Exception)
                {
                    return new ClipResponse { success = false, reason = "deserialization", clips = null };
                }
            }
            else
            {
                return new ClipResponse { success = false, reason = "null response", clips = null };
            }
        }
        
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
                //response.StatusCode 404 500 401
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
