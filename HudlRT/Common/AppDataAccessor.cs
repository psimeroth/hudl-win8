using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Security.Credentials;

namespace HudlRT.Common
{
    public struct TeamContextResponse
    {
        public string teamID { get; set; }
        public string seasonID { get; set; }
    }

    public struct LastViewedResponse
    {
        public string name { get; set; }
        public string timeStamp { get; set; }
        public string ID { get; set; }
        public string thumbnail { get; set; }
    }

    public struct SplashScreenResponse
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }

    class AppDataAccessor
    {
        public static string AUTH_TOKEN = "hudl-authtoken";
        public static string USER_ID = "userId";
        public static string LAST_VIEWED_NAME = "hudl-lastViewedPlaylistName";
        public static string LAST_VIEWED_TIMESTAMP = "hudl-lastViewedPlaylistTimestamp";
        public static string LAST_VIEWED_ID = "hudl-lastViewedPlaylistId";
        public static string LAST_VIEWED_THUMBNAIL = "hudl-lastViewedPlaylistThumbnail";
        public static string TEAM_ID = "hudl-teamID";
        public static string SEASON_ID = "hudl-seasonID";
        public static string USERNAME = "UserName";
        public static string PLAYBACK = "hudl-playbackType";
        public static string PASSWORD = "hudl-password";
        public static string LOGINDATE = "hudl-loginDate";
        public static string SPLASH_X = "hudl-app-splash-x";
        public static string SPLASH_Y = "hudl-app-splash-y";
        public static string SPLASH_HEIGHT = "hudl-app-splash-height";
        public static string SPLASH_WIDTH = "hudl-app-splash-width";

        private static T GetUserRoamingSetting<T>(string keyName){
            string username = GetUsername();
            string userKeyName = username + keyName;
            return (T)Windows.Storage.ApplicationData.Current.RoamingSettings.Values[userKeyName];
        }

        private static void SetUserRoamingSetting<T>(string keyName, T value)
        {
            string username = GetUsername();
            string userKeyName = username + keyName;
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values[userKeyName] = value;
        }

        private static T GetRoamingSetting<T>(string keyName)
        {
            return (T)Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName];
        }

        private static void SetRoamingSetting<T>(string keyName, T value)
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName] = value;
        }

        public static string GetAuthToken()
        {
            return GetRoamingSetting<string>(AUTH_TOKEN);
        }

        public static void SetAuthToken(string token)
        {
            SetRoamingSetting<string>(AUTH_TOKEN, token);
        }

        public static string GetUsername()
        {
            return GetRoamingSetting<string>(USERNAME);
        }

        public static string GetLoginDate()
        {
            string username = GetUsername();
            return GetRoamingSetting<string>(username+LOGINDATE);
        }

        public static void SetUsername(string username)
        {
            SetRoamingSetting<string>(USERNAME, username);
        }

        public static void SetLoginDate(string loginDate)
        {
            string username = GetUsername();
            SetRoamingSetting<string>(username+LOGINDATE, loginDate);
        }

        public static TeamContextResponse GetTeamContext() {
            TeamContextResponse response = new TeamContextResponse();
            
            //needed for api v2 switch
            try
            {
                response.seasonID = GetUserRoamingSetting<string>(SEASON_ID);
                response.teamID = GetUserRoamingSetting<string>(TEAM_ID);
            }
            catch (InvalidCastException e)
            {
                response.seasonID = null;
                response.teamID = null;
            }
            return response;
        }

        public static void SetTeamContext(string seasonID, string teamID)
        {
            SetUserRoamingSetting<string>(SEASON_ID, seasonID);
            SetUserRoamingSetting<string>(TEAM_ID, teamID);
        }

        public static LastViewedResponse GetLastViewed()
        {
            LastViewedResponse response = new LastViewedResponse();
            response.name = GetUserRoamingSetting<string>(LAST_VIEWED_NAME);
            response.timeStamp = GetUserRoamingSetting<string>(LAST_VIEWED_TIMESTAMP);
            response.thumbnail = GetUserRoamingSetting<string>(LAST_VIEWED_THUMBNAIL);
            //needed for the change to string id's for api_v2
            try
            {
                response.ID = GetUserRoamingSetting<string>(LAST_VIEWED_ID);
            }
            catch (InvalidCastException e)
            {
                response.ID = null;
            }
            return response;
        }

        public static void SetLastViewed(string name, string time, string ID, string thumbnail)
        {
            SetUserRoamingSetting<string>(LAST_VIEWED_NAME, name);
            SetUserRoamingSetting<string>(LAST_VIEWED_TIMESTAMP, time);
            SetUserRoamingSetting<string>(LAST_VIEWED_ID, ID);
            SetUserRoamingSetting<string>(LAST_VIEWED_THUMBNAIL, thumbnail);
        }

        public static void SetAnglePreference(string angleName, bool value)
        {
            string teamID = GetUserRoamingSetting<string>(TEAM_ID);
            SetUserRoamingSetting<bool>(teamID + "-" + angleName, value);
        }

        public static bool? GetAnglePreference(string key)
        {
            try
            {
                string teamID = GetUserRoamingSetting<string>(TEAM_ID);
                return GetUserRoamingSetting<bool?>(teamID + "-" + key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int? GetPlaybackType()
        {
            return GetRoamingSetting<int?>(PLAYBACK);
        }

        public static void SetPlaybackType(int type)
        {
            SetRoamingSetting<int>(PLAYBACK, type);
        }

        public static PasswordCredential GetPassword()
        {
            string username = GetUsername();
            if (username != null)
            {
                try
                {
                    PasswordVault vault = new PasswordVault();
                    IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(PASSWORD);
                    return vault.Retrieve(PASSWORD, credentials[0].UserName);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        public static void SetPassword(string password)
        {
            string username = GetUsername();
            PasswordCredential cred = new PasswordCredential(PASSWORD, username, password);
            PasswordVault vault = new PasswordVault();
            vault.Add(cred);
        }

        public static void RemovePasswords()
        {
            PasswordVault vault = new PasswordVault();
            try
            {
                IReadOnlyList<PasswordCredential> credentials = vault.FindAllByResource(PASSWORD);
                foreach (PasswordCredential cred in credentials)
                {
                    vault.Remove(cred);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void SetSplashScreen(SplashScreen splash)
        {
            SetRoamingSetting<double>(SPLASH_X, splash.ImageLocation.Left);
            SetRoamingSetting<double>(SPLASH_Y, splash.ImageLocation.Top);
            SetRoamingSetting<double>(SPLASH_HEIGHT, splash.ImageLocation.Height);
            SetRoamingSetting<double>(SPLASH_WIDTH, splash.ImageLocation.Width);
        }

        public static Nullable<SplashScreenResponse> GetSplashScreen()
        {
            try
            {
                SplashScreenResponse response = new SplashScreenResponse();
                response.X = GetRoamingSetting<double>(SPLASH_X);
                response.Y = GetRoamingSetting<double>(SPLASH_Y);
                response.Height = GetRoamingSetting<double>(SPLASH_HEIGHT);
                response.Width = GetRoamingSetting<double>(SPLASH_WIDTH);
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
