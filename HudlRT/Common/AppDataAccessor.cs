using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Common
{
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

        public static T GetRoamingSetting<T>(string keyName)
        {
            return (T)Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName];
        }

        public static void SetRoamingSetting<T>(string keyName, T value)
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName] = value;
        }

        public static bool RoamingSettingExists(string keyName)
        {
            return Windows.Storage.ApplicationData.Current.RoamingSettings.Values[keyName] != null;
        }
    }
}
