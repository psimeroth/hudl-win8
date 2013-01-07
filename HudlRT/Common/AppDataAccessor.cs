using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;using Windows.Security.Credentials;

namespace HudlRT.Common
{
    public struct TeamContextResponse
    {
        public String teamID { get; set; }
        public String seasonID { get; set; }
    }

    public struct LastViewedResponse
    {
        public String name { get; set; }
        public String timeStamp { get; set; }
        public String ID { get; set; }
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
        public static string LAST_VIEWED_NAME = "hudl-lastViewedCutupName";
        public static string LAST_VIEWED_TIMESTAMP = "hudl-lastViewedCutupTimestamp";
        public static string LAST_VIEWED_ID = "hudl-lastViewedCutupId";
        public static string TEAM_ID = "hudl-teamID";
        public static string SEASON_ID = "hudl-seasonID";
        public static string USERNAME = "UserName";
        public static string PLAYBACK = "hudl-playbackType";
        public static string PASSWORD = "hudl-password";

        public static string SPLASH_X = "hudl-app-splash-x";
        public static string SPLASH_Y = "hudl-app-splash-y";
        public static string SPLASH_HEIGHT = "hudl-app-splash-height";
        public static string SPLASH_WIDTH = "hudl-app-splash-width";

        private static T GetRoamingSetting<T>(string keyName)
        {
            return (T)Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName];
        }

        private static void SetRoamingSetting<T>(string keyName, T value)
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName] = value;
        }

        public static String GetAuthToken()
        {
            return (String)GetRoamingSetting<String>(AUTH_TOKEN);
        }

        public static void SetAuthToken(String token)
        {
            SetRoamingSetting<String>(AUTH_TOKEN, token);
        }

        public static String GetUsername()
        {
            return GetRoamingSetting<String>(USERNAME);
        }

        public static void SetUsername(String username)
        {
            SetRoamingSetting<String>(USERNAME, username);
        }

        public static TeamContextResponse GetTeamContext() {
            string username = GetUsername();
            TeamContextResponse response = new TeamContextResponse();
            
            //needed for api v2 switch
            try
            {
                response.seasonID = GetRoamingSetting<string>(username + SEASON_ID);
                response.teamID = GetRoamingSetting<string>(username + TEAM_ID);
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
            string username = GetUsername();
            SetRoamingSetting<string>(username + SEASON_ID, seasonID);
            SetRoamingSetting<string>(username + TEAM_ID, teamID);
        }

        public static LastViewedResponse GetLastViewed()
        {
            string username = GetUsername();
            LastViewedResponse response = new LastViewedResponse();
            response.name = GetRoamingSetting<String>(username+LAST_VIEWED_NAME);
            response.timeStamp = GetRoamingSetting<String>(username+LAST_VIEWED_TIMESTAMP);

            //needed for the change to string id's for api_v2
            try
            {
                response.ID = GetRoamingSetting<String>(username + LAST_VIEWED_ID);
            }
            catch (InvalidCastException e)
            {
                response.ID = null;
            }
            return response;
        }

        public static void SetLastViewed(String name, String time, String ID)
        {
            string username = GetUsername();
            SetRoamingSetting<String>(username+LAST_VIEWED_NAME, name);
            SetRoamingSetting<String>(username + LAST_VIEWED_TIMESTAMP, time);
            SetRoamingSetting<String>(username + LAST_VIEWED_ID, ID);
        }

        public static void SetAnglePreference(string angleName, bool value)
        {
            string username = GetUsername();
            string teamID = GetRoamingSetting<string>(username+TEAM_ID);
            SetRoamingSetting<bool>(username + teamID + "-" + angleName, value);
        }

        public static bool? GetAnglePreference(string key)
        {
            try
            {
                string username = GetUsername();
                string teamID = GetRoamingSetting<string>(username + TEAM_ID);
                return GetRoamingSetting<bool?>(username + teamID + "-" + key);
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
            String username = GetUsername();
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
            String username = GetUsername();
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
