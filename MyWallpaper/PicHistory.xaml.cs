using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static MyWallpaper.GetUrl;
namespace MyWallpaper {
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 默认加载已缓存的图片和月份
    /// 不存在的要点击刷新后加载
    public sealed partial class PicHistory : Page {
        private readonly List<string> _monthList;
        //public SourceBinding ViewModel { get; set; }
        public PicHistory() {
            InitializeComponent();
            //ViewModel = new SourceBinding(null);
            _monthList = new List<string>();
            MainPage.NavView.AlwaysShowHeader = false;
            PivotHub.Title = DateTime.Now.Year + "年历史壁纸";
            LoadMonth();
            LoadCurrentMonth(DateTime.Now.Month + "月");
        }


        public void LoadMonth() {
            foreach (var key in App.Caches.ListOfPath.Keys) {
                if (!key.ToString().Contains(DateTime.Now.Year.ToString("D2"))) continue;
                var temp = key.ToString().Replace(DateTime.Now.Year.ToString(), "");
                temp = int.Parse(temp.Remove(2)) + "月";
                if (_monthList.Exists(str => str == temp)) continue;
                _monthList.Add(temp);
                // ReSharper disable once PossibleNullReferenceException
                PivotHub.Items.Add(new PivotItem { Header = temp });
            }
        }

        public async void LoadCurrentMonth(string current) {
            if (!_monthList.Contains(current)) {
                _monthList.Add(current);
                PivotHub.Items?.Insert(0, new PivotItem { Header = current });
            }
            int num, idx = 0, currentmonth = int.Parse(current.Replace("月", ""));
            if (DateTime.Now.Month == currentmonth) {
                num = DateTime.Now.Day;
            } else {
                var month = DateTime.Now.Month;
                num = GetDays(month);
                for (; currentmonth < DateTime.Now.Month; currentmonth++) idx += GetDays(currentmonth);
                idx += DateTime.Now.Day;
            }

            var jsonObj = Json_Decode(Initialize("idx=" + idx + "&num=" + num));
            if (jsonObj.Count > 0) {
                for (var index = jsonObj.Count - 1; index >= 0; index--) {
                    var json = jsonObj[index];
                    var key = int.Parse(DateTime.Now.Year + currentmonth.ToString("D2") +
                                        (index + 1).ToString("D2"));
                    json.Description = json.Description.Remove(json.Description.IndexOf('，') < 0 ? (json.Description.IndexOf('(') < 0 ? json.Description.IndexOf('（') : json.Description.IndexOf('(')) : json.Description.IndexOf('，'));
                    if (!App.Caches.ListOfPath.ContainsKey(key)) {
                        var task = await Task.Run(function: async () => {
                            await App.Caches.Download(key, json.Url, json.Description);
                            var path = await App.Caches.LoadPicPath(key);
                            return path;
                        });
                    }
                    //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    var a = (PivotItem[])PivotHub.ItemsPanelRoot?.Children.ToArray();
                    //    PivotItem temp = a?.Where(item => item.Header as string == current).GetEnumerator().Current;

                    //});
                }
            }
        }

        private static int GetDays(int month) {
            var cnt = -1;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (month) {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    cnt = 31;
                    break;
                case 4:
                case 6:
                case 9:
                case 11:
                    cnt = 30;
                    break;
                case 2 when DateTime.IsLeapYear(DateTime.Now.Year):
                    cnt = 29;
                    break;
                case 2 when !DateTime.IsLeapYear(DateTime.Now.Year):
                    cnt = 28;
                    break;
            }

            return cnt;
        }
    }
}
