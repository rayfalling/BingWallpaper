using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using static MyWallpaper.GetUrl;

namespace MyWallpaper {
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class YesterdayPic {
        public YesterdayPic() {
            InitializeComponent();
            MainPage.NavView.AlwaysShowHeader = true;
            LoadProcess();
        }
        private async void LoadProcess() {
            var date = DateTime.Now;
            date = date.AddDays(-1);
            if (date.Day == 1) date = date.AddDays(-1);
            var key = int.Parse(date.Year + date.Month.ToString("D2") + date.Day.ToString("D2"));
            if (App.Caches.ListOfPath.ContainsKey(key)) {
                var task = await Task.Run(async () => await App.Caches.LoadPicPath(key));
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    BingImage.Source = new BitmapImage(new Uri(task));
                    MainPage.NavView.Header = App.Caches.LoadDescription(key);
                });
            } else {
                var jsonObj = Json_Decode(Initialize("idx=1&num=1"))[0] ?? new JsonObj { Description = "网络连接失败！", Url = new Uri("") };
                jsonObj.Description = jsonObj.Description.Remove(jsonObj.Description.IndexOf('，') < 0 ? (jsonObj.Description.IndexOf('(') < 0 ? jsonObj.Description.IndexOf('（') : jsonObj.Description.IndexOf('(')) : jsonObj.Description.IndexOf('，'));
                var task = await Task.Run(async () => {
                    await App.Caches.Download(key, jsonObj.Url, jsonObj.Description);
                    var path = await App.Caches.LoadPicPath(key);
                    return path;
                });
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    BingImage.Source = new BitmapImage(new Uri(task));
                    MainPage.NavView.Header = jsonObj.Description;
                });
            }
        }
    }
}
