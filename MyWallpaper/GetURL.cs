using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace MyWallpaper {
    internal class GetUrl {
        public static string Initialize(string data = "", string url = "http://disk.jszhihui.com/bing/index.php") {
            try {
                //创建Get请求
                url = url + (data == "" ? "" : "?") + data;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "text;charset=UTF-8";
                //接受返回来的数据
                var response = (HttpWebResponse)request.GetResponse();
                var stream = response.GetResponseStream();
                var streamReader = new StreamReader(stream ?? throw new InvalidOperationException(),
                    Encoding.GetEncoding("utf-8"));
                var retString = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                response.Close();
                return retString;
            } catch (WebException webEx) {
                Console.WriteLine(webEx.Message);
            }
            return null;
        }

        public static List<JsonObj> Json_Decode(string jsonStr) {
            if (jsonStr == null)
                return null;
            var jsonArray = JsonArray.Parse(jsonStr);
            return jsonArray.GetArray().Select(item => new JsonObj {
                Url = new Uri(item.GetObject().GetNamedValue("url").ToString().Replace("\"", "")),
                Description = item.GetObject().GetNamedValue("description").ToString().Replace("\"", "")
            }).ToList();
        }
    }
}