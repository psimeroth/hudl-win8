using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

namespace App5.Common
{
    class ServiceAccessor
    {

        public static async Task<string> MakeApiCall(string url, string method, string jsonString, string header)
        {
            var req = (HttpWebRequest) WebRequest.Create(url);
            req.ContentType = "application/json";
            req.Method = method;

            if (req.Method == "POST")
            {
                using (var requestStream = await req.GetRequestStreamAsync())
                {
                    var writer = new StreamWriter(requestStream);
                    writer.Write(jsonString);
                    writer.Flush();
                }
            }

            if (header != string.Empty)
            {
                req.Headers["hudl-authtoken"] = header;
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
