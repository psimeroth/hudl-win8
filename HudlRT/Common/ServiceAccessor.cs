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
using Windows.Networking.Connectivity;
using HudlRT.ViewModels;

namespace HudlRT.Common
{

    public enum SERVICE_RESPONSE { SUCCESS, NO_CONNECTION, NULL_RESPONSE, DESERIALIZATION, CREDENTIALS, PRIVILEGE };

    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    struct LoginResponse
    {
        public SERVICE_RESPONSE status { get; set; }
    }

    struct TeamResponse
    {
        public BindableCollection<Team> teams { get; set; }
        public SERVICE_RESPONSE status { get; set; }
    }

    struct GameResponse
    {
        public BindableCollection<Game> games { get; set; }
        public SERVICE_RESPONSE status { get; set; }
    }

    struct CategoryResponse
    {
        public BindableCollection<Category> categories { get; set; }
        public SERVICE_RESPONSE status { get; set; }
    }

    struct CutupResponse
    {
        public BindableCollection<Cutup> cutups { get; set; }
        public SERVICE_RESPONSE status { get; set; }
    }

    struct ClipResponse
    {
        public BindableCollection<Clip> clips { get; set; }
        public SERVICE_RESPONSE status { get; set; }
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
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new LoginResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }

            var loginResponse = await ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOGIN, loginArgs);
            if (!string.IsNullOrEmpty(loginResponse))
            {
                var obj = JsonConvert.DeserializeObject<LoginResponseDTO>(loginResponse);
                AppDataAccessor.SetRoamingSetting<string>(AppDataAccessor.AUTH_TOKEN, obj.Token);
                AppDataAccessor.SetRoamingSetting<string>(AppDataAccessor.USER_ID, obj.UserId);
                string urlExtension = "privileges/" + obj.UserId.ToString();
                var privilegesResponse = await ServiceAccessor.MakeApiCallGet(urlExtension);
                if (!string.IsNullOrEmpty(privilegesResponse))
                {
#if DEBUG
                    return new LoginResponse { status = SERVICE_RESPONSE.SUCCESS };
#else
                    if (privilegesResponse.Contains("Win8App"))
                    {
                        return new LoginResponse { status = SERVICE_RESPONSE.SUCCESS };
                    }
                    else
                    {
                        return new LoginResponse { status = SERVICE_RESPONSE.PRIVILEGE};
                    }

#endif
                    //Needs to be improved in the future if we want to 

                }
                else
                {
                    return new LoginResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
                }
            }
            else
            {
                return new LoginResponse { status = SERVICE_RESPONSE.CREDENTIALS };
            }
        }

        public static async Task<TeamResponse> GetTeams()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new TeamResponse { status = SERVICE_RESPONSE.NO_CONNECTION, teams = null };
            }

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
                    return new TeamResponse { status = SERVICE_RESPONSE.SUCCESS, teams = teamCollection };
                }
                catch (Exception)
                {
                    return new TeamResponse { status = SERVICE_RESPONSE.DESERIALIZATION, teams = null };
                }
            }
            else
            {
                return new TeamResponse { status = SERVICE_RESPONSE.NULL_RESPONSE, teams = null };
            }
        }

        public static async Task<GameResponse> GetGames(string teamId, string seasonId)
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new GameResponse { status = SERVICE_RESPONSE.NO_CONNECTION, games = null };
            }

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
                    return new GameResponse { status = SERVICE_RESPONSE.SUCCESS, games = gameCollection };
                }
                catch (Exception)
                {
                    return new GameResponse { status = SERVICE_RESPONSE.DESERIALIZATION, games = null };
                }
            }
            else
            {
                return new GameResponse { status = SERVICE_RESPONSE.NULL_RESPONSE, games = null };
            }
        }

        public static async Task<CategoryResponse> GetGameCategories(string gameId)
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new CategoryResponse { status = SERVICE_RESPONSE.NO_CONNECTION, categories = null };
            }

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
                    return new CategoryResponse { status = SERVICE_RESPONSE.SUCCESS, categories = categoryCollection };
                }
                catch (Exception)
                {
                    return new CategoryResponse { status = SERVICE_RESPONSE.DESERIALIZATION, categories = null };
                }
            }
            else
            {
                return new CategoryResponse { status = SERVICE_RESPONSE.NULL_RESPONSE, categories = null };
            }
        }

        public static async Task<CutupResponse> GetCategoryCutups(string categoryId)
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new CutupResponse { status = SERVICE_RESPONSE.NO_CONNECTION, cutups = null };
            }
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
                    return new CutupResponse { status = SERVICE_RESPONSE.SUCCESS, cutups = cutupCollection };
                }
                catch (Exception)
                {
                    return new CutupResponse { status = SERVICE_RESPONSE.DESERIALIZATION, cutups = null };
                }
            }
            else
            {
                return new CutupResponse { status = SERVICE_RESPONSE.NULL_RESPONSE, cutups = null };
            }
        }

        public static async Task<ClipResponse> GetCutupClips(CutupViewModel cutup)
        {

            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0)
            {
                return new ClipResponse { status = SERVICE_RESPONSE.NO_CONNECTION, clips = null };
            }

            var clips = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CLIPS.Replace("#", cutup.CutupId.ToString()));
            if (!string.IsNullOrEmpty(clips))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
                    BindableCollection<Clip> clipCollection = new BindableCollection<Clip>();
                    cutup.DisplayColumns = obj.DisplayColumns;
                    foreach (ClipDTO clipDTO in obj.ClipsList.Clips)
                    {
                        Clip c = Clip.FromDTO(clipDTO, obj.DisplayColumns);
                        if (c != null)
                        {
                            clipCollection.Add(c);
                        }
                    }
                    return new ClipResponse { status = SERVICE_RESPONSE.SUCCESS, clips = clipCollection };
                }
                catch (Exception)
                {
                    return new ClipResponse { status = SERVICE_RESPONSE.DESERIALIZATION, clips = null };
                }
            }
            else
            {
                return new ClipResponse { status = SERVICE_RESPONSE.NULL_RESPONSE, clips = null };
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
                httpRequestMessage.Headers.Add("hudl-authtoken", AppDataAccessor.GetRoamingSetting<string>(AppDataAccessor.AUTH_TOKEN));
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
