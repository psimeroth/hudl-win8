using HudlRT.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace HudlRT.Common
{
    class CacheAccessor
    {
        public static string CACHE_CLIPS = "hudl-clipscache-";

        public static async Task CacheCutupClips(CutupViewModel cutup)
        {
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            var folder = ApplicationData.Current.TemporaryFolder;
            var destinationFile = await folder.CreateFileAsync(CACHE_CLIPS + cutup.CutupId, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(destinationFile, JsonConvert.SerializeObject(response));
        }

        public static async Task<ClipResponse> GetCachedCutupClips(CutupViewModel cutup)
        {
            var folder = ApplicationData.Current.TemporaryFolder;
            var sourceFile = await folder.GetFileAsync(CACHE_CLIPS + cutup.CutupId);
            return JsonConvert.DeserializeObject<ClipResponse>(CACHE_CLIPS + cutup.CutupId);
        }
    }
}
