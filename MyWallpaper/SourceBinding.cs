using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyWallpaper {
    public class SourceBinding {
        public ImageSource Source { get; set; }
        public SourceBinding(string url) {
            Source = new BitmapImage(new Uri(url));
        }
    }
}