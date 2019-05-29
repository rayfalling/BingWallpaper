using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyWallpaper {
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage {
        public static NavigationView NavView;
        public MainPage() {
            InitializeComponent();
            NavView = NvSample;
            NvSample.AlwaysShowHeader = true;
            NvSample.SelectedItem = MainItem;
            ContentFrame.Navigate(typeof(BingPic));
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void NvSample_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
            if (args.IsSettingsSelected) {
                ContentFrame.Navigate(typeof(SettingsPage));
            } else {
                var item = (NavigationViewItem)args.SelectedItem;
                var str = item.Tag;
                var pages = new List<Type> { typeof(BingPic), typeof(YesterdayPic), typeof(PicHistory) };
                var type = pages.Find(i => i.ToString() == "MyWallpaper." + (string)str);
                ContentFrame.Navigate(type);
            }
        }

        private void SetBg(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

        }

        private void SetLockScreen(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

        }
        private void SaveLocal(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

        }
    }
}
