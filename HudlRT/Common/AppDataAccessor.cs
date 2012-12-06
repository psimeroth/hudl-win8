using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace HudlRT.Common
{
    public struct TeamContextResponse
    {
        public long? teamID { get; set; }
        public long? seasonID { get; set; }
    }

    public struct LastViewedResponse
    {
        public String name { get; set; }
        public String timeStamp { get; set; }
        public long? ID { get; set; }
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
            response.seasonID = GetRoamingSetting<long?>(username + SEASON_ID);
            response.teamID = GetRoamingSetting<long?>(username + TEAM_ID);
            return response;
        }

        public static void SetTeamContext(long seasonID, long teamID)
        {
            string username = GetUsername();
            SetRoamingSetting<long>(username + SEASON_ID, seasonID);
            SetRoamingSetting<long>(username + TEAM_ID, teamID);
        }

        public static LastViewedResponse GetLastViewed()
        {
            string username = GetUsername();
            LastViewedResponse response = new LastViewedResponse();
            response.name = GetRoamingSetting<String>(username+LAST_VIEWED_NAME);
            response.timeStamp = GetRoamingSetting<String>(username+LAST_VIEWED_TIMESTAMP);
            response.ID = GetRoamingSetting<long?>(username+LAST_VIEWED_ID);
            return response;
        }

        public static void SetLastViewed(String name, String time, long ID)
        {
            string username = GetUsername();
            SetRoamingSetting<String>(username+LAST_VIEWED_NAME, name);
            SetRoamingSetting<String>(username + LAST_VIEWED_TIMESTAMP, time);
            SetRoamingSetting<long>(username + LAST_VIEWED_ID, ID);
        }

        public static void SetAnglePreference(string angleName, bool value)
        {
            string username = GetUsername();
            long teamID = GetRoamingSetting<long>(username+TEAM_ID);
            SetRoamingSetting<bool>(username + teamID + "-" + angleName, value);
        }

        public static bool? GetAnglePreference(string key)
        {
            string username = GetUsername();
            long teamID = GetRoamingSetting<long>(username+TEAM_ID);
            return GetRoamingSetting<bool?>(username + teamID + "-" + key);
        }

        public static int? GetPlaybackType()
        {
            return GetRoamingSetting<int?>(PLAYBACK);
        }

        public static void SetPlaybackType(int type)
        {
            SetRoamingSetting<int>(PLAYBACK, type);
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
