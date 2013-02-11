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
using HudlRT.Parameters;

namespace HudlRT.Common
{
    class Response
    {
        public SERVICE_RESPONSE status { get; set; }
    }

    public enum SERVICE_RESPONSE { SUCCESS, NO_CONNECTION, NULL_RESPONSE, DESERIALIZATION, CREDENTIALS, PRIVILEGE };

    struct LoginSender
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class LoginResponse: Response
    {
    }

    class TeamResponse: Response
    {
        public BindableCollection<Team> teams { get; set; }
    }

    class GameResponse: Response
    {
        public BindableCollection<Game> games { get; set; }
    }

    class CategoryResponse: Response
    {
        public BindableCollection<Category> categories { get; set; }
    }

    class CutupResponse: Response
    {
        public BindableCollection<Cutup> cutups { get; set; }
    }

    class ClipResponse: Response
    {
        public BindableCollection<Clip> clips { get; set; }
    }

    public class NoInternetConnectionException : Exception
    {

    }

    public class GeneralInternetException : Exception
    {
    }
    /// <summary>
    /// Class used make API calls.
    /// </summary>
    class ServiceAccessor
    {
#if DEBUG
        private const string URL_BASE = "http://www.staghudl.com/api/v2/";
        private const string URL_BASE_SECURE = "https://www.staghudl.com/api/v2/";
#else
        private const string URL_BASE = "http://www.hudl.com/api/v2/";
        private const string URL_BASE_SECURE = "https://www.hudl.com/api/v2/";
#endif
        public const string URL_SERVICE_LOGIN = "login";
        public const string URL_SERVICE_GET_TEAMS = "teams";
        public const string URL_SERVICE_GET_SCHEDULE = "teams/{0}/schedule";//returns games
        public const string URL_SERVICE_GET_SCHEDULE_BY_SEASON = "teams/{0}/schedule?season={1}";//returns games
        public const string URL_SERVICE_GET_CATEGORIES_FOR_GAME = "games/{0}/categories";//returns categories
        public const string URL_SERVICE_GET_CUTUPS_BY_CATEGORY = "categories/{0}/playlists";//returns cutups
        public const string URL_SERVICE_GET_CLIPS = "playlists/{0}/clips?startIndex={1}";//returns clips

        public static bool ConnectedToInternet()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return !(InternetConnectionProfile == null || InternetConnectionProfile.GetNetworkConnectivityLevel() == 0);
        }

        public static async Task<LoginResponse> Login(string loginArgs)
        {
            //var loginResponse = await ServiceAccessor.MakeApiCallGet("athlete");

            if (!ConnectedToInternet())
            {
                return new LoginResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            var loginResponse = await ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOGIN, loginArgs, false);
            if (!string.IsNullOrEmpty(loginResponse))
            {
                var obj = JsonConvert.DeserializeObject<LoginResponseDTO>(loginResponse);
                AppDataAccessor.SetAuthToken(obj.Token);
                string urlExtension = "users/" + obj.UserId.ToString() + "/privileges/";
#if DEBUG
#else
                var privilegesResponse = await ServiceAccessor.MakeApiCallGet(urlExtension, false);
                if (!string.IsNullOrEmpty(privilegesResponse))
                {
                    if (privilegesResponse.Contains("Win8App"))
                    {
                        return new LoginResponse { status = SERVICE_RESPONSE.SUCCESS };
                    }
                    else
                    {
                        return new LoginResponse { status = SERVICE_RESPONSE.PRIVILEGE };
                    }
                }
                else
                {
                    return new LoginResponse { status = SERVICE_RESPONSE.PRIVILEGE };
                }
#endif
                return new LoginResponse { status = SERVICE_RESPONSE.SUCCESS };
            }
            else
            {
                return new LoginResponse { status = SERVICE_RESPONSE.CREDENTIALS };
            }
        }

        public static async Task<TeamResponse> GetTeams()
        {
            var teams = await MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_TEAMS, true);
            if (!string.IsNullOrEmpty(teams) && teams != "NoConnection")
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
                    return new TeamResponse { status = SERVICE_RESPONSE.DESERIALIZATION };
                }
            }
            else if (teams == "NoConnection")
            {
                return new TeamResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            else
            {
                return new TeamResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
            }
        }

        public static async Task<GameResponse> GetGames(string teamId, string seasonId)
        {
            var games = await MakeApiCallGet(String.Format(ServiceAccessor.URL_SERVICE_GET_SCHEDULE_BY_SEASON, teamId, seasonId), true);
            if (!string.IsNullOrEmpty(games) && games != "NoConnection")
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
                    return new GameResponse { status = SERVICE_RESPONSE.DESERIALIZATION };
                }
            }
            else if (games == "NoConnection")
            {
                return new GameResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            else
            {
                return new GameResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
            }
        }

        public static async Task<CategoryResponse> GetGameCategories(string gameId)
        {
            var categories = await MakeApiCallGet(String.Format(ServiceAccessor.URL_SERVICE_GET_CATEGORIES_FOR_GAME,gameId), true);
            if (!string.IsNullOrEmpty(categories) && categories != "NoConnection")
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
                    return new CategoryResponse { status = SERVICE_RESPONSE.DESERIALIZATION };
                }
            }
            else if (categories == "NoConnection")
            {
                return new CategoryResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            else
            {
                return new CategoryResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
            }
        }

        public static async Task<CutupResponse> GetCategoryCutups(string categoryId)
        {
            var cutups = await MakeApiCallGet(String.Format(ServiceAccessor.URL_SERVICE_GET_CUTUPS_BY_CATEGORY, categoryId), true);
            if (!string.IsNullOrEmpty(cutups) && cutups != "NoConnection")
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
                    return new CutupResponse { status = SERVICE_RESPONSE.DESERIALIZATION };
                }
            }
            else if (cutups == "NoConnection")
            {
                return new CutupResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            else
            {
                return new CutupResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
            }
        }

        public static async Task<List<Clip>> GetAdditionalCutupClips(string cutupID, int startIndex)
        {
            var clips = await MakeApiCallGet(String.Format(ServiceAccessor.URL_SERVICE_GET_CLIPS, cutupID, startIndex.ToString()), true);
            var clipResponseDTO = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
            List<Clip> clipCollection = new List<Clip>();
            if (clipResponseDTO.ClipsList.Clips.Count == 100)
            {
                foreach (ClipDTO clipDTO in clipResponseDTO.ClipsList.Clips)
                {
                    Clip c = Clip.FromDTO(clipDTO, clipResponseDTO.DisplayColumns);
                    if (c != null)
                    {
                        clipCollection.Add(c);
                    }
                }
                var additionalClips = await GetAdditionalCutupClips(cutupID, startIndex + 100);
                foreach (Clip c in additionalClips)
                {
                    clipCollection.Add(c);
                }
                return clipCollection;
            }
            else
            {
                foreach (ClipDTO clipDTO in clipResponseDTO.ClipsList.Clips)
                {
                    Clip c = Clip.FromDTO(clipDTO, clipResponseDTO.DisplayColumns);
                    if (c != null)
                    {
                        clipCollection.Add(c);
                    }
                }
                return clipCollection;
            }

        }

        public static async Task<ClipResponse> GetCutupClips(CutupViewModel cutup)
        {
            var clips = await MakeApiCallGet(String.Format(ServiceAccessor.URL_SERVICE_GET_CLIPS, cutup.CutupId.ToString(), "0"), true);
            if (!string.IsNullOrEmpty(clips) && clips != "NoConnection")
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
                catch (Exception e)
                {
                    return new ClipResponse { status = SERVICE_RESPONSE.DESERIALIZATION };
                }
            }
            else if (clips == "NoConnection")
            {
                return new ClipResponse { status = SERVICE_RESPONSE.NO_CONNECTION };
            }
            else
            {
                return new ClipResponse { status = SERVICE_RESPONSE.NULL_RESPONSE };
            }
        }

        /// <summary>
        /// Makes an API call to the base URL defined in AppData.cs using the GET method.
        /// </summary>
        /// <param name="url">The API function to hit.</param>
        /// <param name="jsonString">Any necesary data required to make the call.</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallGet(string url, bool showDialog)
        {
            if (!ConnectedToInternet())
            {
                APIExceptionDialog.ShowNoInternetConnectionDialog();
                return "NoConnection";
            }
            var httpClient = new HttpClient();
            Uri uri = new Uri(URL_BASE + url);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            httpRequestMessage.Headers.Add("hudl-authtoken", ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"].ToString());
            httpRequestMessage.Headers.Add("User-Agent", "HudlWin8/1.0.0");
            var response = await httpClient.SendAsync(httpRequestMessage);
            if (showDialog)
            {
                if (!response.IsSuccessStatusCode)
                {
                    APIExceptionDialog.ShowStatusCodeExceptionDialog(response.StatusCode.ToString(), uri.ToString());
                    return null;
                }
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Makes an API call to the base URL defined in AppData.cs using the POST method.
        /// </summary>
        /// <param name="url">The API function to hit.</param>
        /// <param name="jsonString">Any necesary data required to make the call.</param>
        /// <returns>The string response returned from the API call.</returns>
        public static async Task<string> MakeApiCallPost(string url, string jsonString, bool showDialog)
        {
            var httpClient = new HttpClient();
            Uri uri = new Uri(URL_BASE_SECURE + url);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            httpRequestMessage.Headers.Add("User-Agent", "HudlWin8/1.0.0");
            httpRequestMessage.Content = new StringContent(jsonString);
            httpRequestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await httpClient.SendAsync(httpRequestMessage);
            //response.StatusCode 404 500 401
            if(!response.IsSuccessStatusCode)
            {
                APIExceptionDialog.ShowStatusCodeExceptionDialog(response.StatusCode.ToString(), uri.ToString());
                return null;
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}
