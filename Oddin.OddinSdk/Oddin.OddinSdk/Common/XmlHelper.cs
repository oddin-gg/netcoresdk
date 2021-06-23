using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Oddin.OddinSdk.Common
{
    internal static class XmlHelper
    {
        public static bool TryDeserialize<T>(string xml, out T result)
        {
            try
            {
                result = Deserialize<T>(xml);
            }
            catch (InvalidOperationException)
            {
                result = default;
                return false;
            }
            return true;
        }

        private static T Deserialize<T>(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        public static bool TrySerialize<T>(T toBeSerialized, out string result)
        {
            try
            {
                result = Serialize(toBeSerialized);
            }
            catch (InvalidOperationException)
            {
                result = default;
                return false;
            }
            return true;
        }

        private static string Serialize<T>(T toBeSerialized)
        {
            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, toBeSerialized);
                return stream.ToString();
            }
        }
    }
}
