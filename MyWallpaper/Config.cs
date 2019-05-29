using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MyWallpaper {
    [Serializable]
    [XmlRoot("Config")]
    public class Configs {
        [XmlElement("FirstStart")]
        public bool IsFirstStrat { get; set; }
        [XmlElement("ChangeBG")]
        public bool TimelyChangeBackGroundImage { get; set; }
        [XmlElement("ChangeLogImage")]
        public bool TimelyChangeLoginImage { get; set; }
        public Configs() {
            IsFirstStrat = true;
            TimelyChangeBackGroundImage = false;
            TimelyChangeLoginImage = false;
        }
    }
    class ConfigMethods {
        public static async Task<Configs> LoadConfigs() {
            var storageFolder = ApplicationData.Current.RoamingFolder;
            var configFileStream =
                await storageFolder.OpenStreamForReadAsync("app.config");
            configFileStream.Position = 0;
            Stream stream = new MemoryStream();
            await configFileStream.CopyToAsync(stream);
            stream.Position = 0;
            var serializer = new XmlSerializer(typeof(Configs));
            var configs = serializer.Deserialize(stream);
            configFileStream.Close();configFileStream.Dispose();
            stream.Close(); stream.Dispose();
            return (Configs)configs;
        }

        public static async Task<bool> CreateConfigs() {
            try {
                var configs = new Configs();
                var serializer = new XmlSerializer(typeof(Configs));
                Stream stream = new MemoryStream();
                serializer.Serialize(stream, configs);
                var configFileStream =
                    await ApplicationData.Current.RoamingFolder.OpenStreamForWriteAsync("app.config",
                        CreationCollisionOption.ReplaceExisting);
                stream.Position = 0;
                await stream.CopyToAsync(configFileStream, (int)stream.Length);
                stream.Flush();
                stream.Close();
                stream.Dispose();
                configFileStream.Flush();
                configFileStream.Close();
                configFileStream.Dispose();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}