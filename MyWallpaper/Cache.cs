using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyWallpaper {
    public class Cache {
        public SortedList<int, string> ListOfPath;
        public SortedList<int, string> ListOfDescription;
        private readonly StorageFolder _cachefolder = ApplicationData.Current.LocalCacheFolder;
        private readonly StorageFolder _localfolder = ApplicationData.Current.LocalFolder;

        private async Task<IStorageItem> GetMapFile() {
            var mapFile = await _localfolder.TryGetItemAsync("map.xml");
            return mapFile;

        }

        private async Task<bool> IsMapFileExist() {
            var mapFile = await _localfolder.TryGetItemAsync("map.xml");
            return mapFile != null;//文件存在返回true
        }
        private async Task<IStorageItem> GetDescriptionFile() {
            var descriptionFile = await _localfolder.TryGetItemAsync("description.xml");
            return descriptionFile;
        }

        private async Task<bool> IsDescriptionFileExist() {
            var descriptionFile = await _localfolder.TryGetItemAsync("description.xml");
            return descriptionFile != null;//文件存在返回true
        }
        public async void CreateCacheAsync() {
            var isExist = await IsMapFileExist();
            if (isExist) {
                if (!(await GetMapFile() is StorageFile map)) return;
                var streamAsync = await map.OpenAsync(FileAccessMode.Read);
                using (var stream = streamAsync.AsStream()) {
                    ListOfPath = new SortedList<int, string>();
                    if (stream.Length == 0) return;
                    var xmlSerializer = new XmlSerializer(typeof(List<SerializeList>));
                    var temp = xmlSerializer.Deserialize(stream) as List<SerializeList>;
                    temp?.ForEach(pair => ListOfPath.Add(pair.Key, pair.Value));
                    stream.Dispose();
                }
                streamAsync.Dispose();
            } else ListOfPath = new SortedList<int, string>();
            isExist = await IsDescriptionFileExist();
            if (isExist) {
                if (!(await GetDescriptionFile() is StorageFile descrirtion)) return;
                var streamAsync = await descrirtion.OpenAsync(FileAccessMode.Read);
                using (var stream = streamAsync.AsStream()) {
                    ListOfDescription = new SortedList<int, string>();
                    if (stream.Length == 0) return;
                    var xmlSerializer = new XmlSerializer(typeof(List<SerializeList>));
                    var temp = xmlSerializer.Deserialize(stream) as List<SerializeList>;
                    temp?.ForEach(pair => ListOfDescription.Add(pair.Key, pair.Value));
                    stream.Dispose();
                }
                streamAsync.Dispose();
            } else ListOfDescription = new SortedList<int, string>();
        }
        public async Task SaveCacheAsync() {
            {
                var map = await GetMapFile() as StorageFile ?? await _localfolder.CreateFileAsync("map.xml");
                var streamAsync = await map.OpenAsync(FileAccessMode.ReadWrite);
                using (var stream = streamAsync.GetOutputStreamAt(0)) {
                    var temp = new List<SerializeList>();
                    foreach (var pair in ListOfPath) {
                        temp.Add(new SerializeList(pair.Key, pair.Value));
                    }
                    var xmlSerializer = new XmlSerializer(typeof(List<SerializeList>));
                    xmlSerializer.Serialize(stream.AsStreamForWrite(), temp);
                    stream.Dispose();
                }
                streamAsync.Dispose();
            }
            {
                var descrirtion = await GetDescriptionFile() as StorageFile ?? await _localfolder.CreateFileAsync("description.xml");
                var streamAsync = await descrirtion.OpenAsync(FileAccessMode.ReadWrite);
                using (var stream = streamAsync.GetOutputStreamAt(0)) {
                    var temp = new List<SerializeList>();
                    foreach (var pair in ListOfDescription) {
                        temp.Add(new SerializeList(pair.Key, pair.Value));
                    }
                    var xmlSerializer = new XmlSerializer(typeof(List<SerializeList>));
                    xmlSerializer.Serialize(stream.AsStreamForWrite(), temp);
                    stream.Dispose();
                }
                streamAsync.Dispose();
            }
        }

        private void Add(int key, string value, string description) {
            if (!ListOfPath.ContainsKey(key)) ListOfPath.Add(key, value);
            if (!ListOfDescription.ContainsKey(key)) ListOfDescription.Add(key, description);
        }

        private string Load(int key) {
            return ListOfPath[key];
        }
        public string LoadDescription(int key) {
            return ListOfDescription[key];
        }

        public async Task Download(int key, Uri url, string description) {
            var name = url.ToString().Substring(url.ToString().LastIndexOf('/')).AesStr().Replace('/', 'a') + ".jpg";
            if (await _cachefolder.TryGetItemAsync(name) != null) return;
            var storageFile = await _cachefolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            var httpclient = new HttpClient();
            var data = await httpclient.GetStreamAsync(url);
            var stream = await storageFile.OpenStreamForWriteAsync();
            stream.Position = 0;
            await data.CopyToAsync(stream);//将获取的HttpClient的Stream流复制到文件的stream流   
            await stream.FlushAsync();//异步刷新
            stream.Dispose();
            Add(key, storageFile.Name, description);
            await SaveCacheAsync();
        }

        public async Task<string> LoadPicPath(int key) {
            var result = await _cachefolder.GetFileAsync(Load(key));
            return result.Path;
        }

    }
    internal static class ImageExtend {

        public static async Task<byte[]> SaveToBytesAsync(this ImageSource imageSource) {
            byte[] imageBuffer;
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync("temp.jpg", CreationCollisionOption.ReplaceExisting);
            using (var ras = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None)) {
                if (imageSource is WriteableBitmap bitmap) {
                    var stream = bitmap.PixelBuffer.AsStream();
                    var buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ras);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, buffer);
                    await encoder.FlushAsync();
                }
                var imageStream = ras.AsStream();
                imageStream.Seek(0, SeekOrigin.Begin);
                imageBuffer = new byte[imageStream.Length];
                await imageStream.ReadAsync(imageBuffer, 0, imageBuffer.Length);
            }
            await file.DeleteAsync(StorageDeleteOption.Default);
            return imageBuffer;
        }
        public static async Task<ImageSource> SaveToImageSource(this byte[] imageBuffer) {
            ImageSource imageSource = null;
            using (var stream = new MemoryStream(imageBuffer)) {
                var ras = stream.AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ras);
                var provider = await decoder.GetPixelDataAsync();
                var buffer = provider.DetachPixelData();
                var bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await bitmap.PixelBuffer.AsStream().WriteAsync(buffer, 0, buffer.Length);
                imageSource = bitmap;
            }
            return imageSource;
        }
    }
    internal class Keys {
        internal static string Keyval = "adguij2kh3@5u4do%e5f!ui_af1=+23h";
        internal static string Ivval = "wanghaiwei990310";
    }
    public static class Aes {
        #region AES 加密解密

        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="value">待加密字段</param>  
        /// <returns></returns>  
        public static string AesStr(this string value) {
            string keyVal = Keys.Keyval, ivVal = Keys.Ivval;
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }
            var encoding = Encoding.UTF8;
            var btKey = encoding.GetBytes(keyVal);
            var btIv = encoding.GetBytes(ivVal);
            var byteArray = encoding.GetBytes(value);
            string encrypt;
            var aes = Rijndael.Create();
            using (var mStream = new MemoryStream()) {
                using (var cStream = new CryptoStream(mStream, aes.CreateEncryptor(btKey, btIv), CryptoStreamMode.Write)) {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(mStream.ToArray());
                }
            }
            aes.Clear();
            return encrypt.Remove(16);
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="value">待加密字段</param>   
        /// <returns></returns>  
        public static string UnAesStr(this string value) {
            string keyVal = Keys.Keyval, ivVal = Keys.Ivval;
            var encoding = Encoding.UTF8;
            var btKey = encoding.GetBytes(keyVal);
            var btIv = encoding.GetBytes(ivVal);
            var byteArray = Convert.FromBase64String(value);
            string decrypt;
            var aes = Rijndael.Create();
            using (var mStream = new MemoryStream()) {
                using (var cStream = new CryptoStream(mStream, aes.CreateDecryptor(btKey, btIv), CryptoStreamMode.Write)) {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    decrypt = encoding.GetString(mStream.ToArray());
                }
            }
            aes.Clear();
            return decrypt;
        }
        #endregion
    }
}