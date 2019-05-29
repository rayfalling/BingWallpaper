using System.Collections.Generic;
using System.Xml.Serialization;

namespace MyWallpaper {
    public class SerializeList {
        public SerializeList(int pairKey, string pairValue) {
            Key = pairKey;
            Value = pairValue;
        }
        public SerializeList() { }
        [XmlElement("key")]
        public int Key { get; set; }
        [XmlElement("value")]
        public string Value { get; set; }
    }
}