using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Oddin.OddinSdk.Common
{
    internal static class XmlHelper
    {
        internal static bool TryDeserialize<T>(string xml, out T result)
        {
            try
            {
                using var stringReader = new StringReader(xml);
                using var xmlReader = XmlReader.Create(stringReader);

                var serializer = new XmlSerializer(typeof(T));
                if (serializer.CanDeserialize(xmlReader) == false)
                {
                    result = default;
                    return false;
                }

                result = (T)serializer.Deserialize(xmlReader);
                return true;
            }
            catch (InvalidOperationException)
            {
                result = default;
                return false;
            }
        }

        internal static bool TrySerialize<T>(T toBeSerialized, out string result)
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
            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream);

            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, toBeSerialized);
            return stream.ToString();
        }
    }
}
