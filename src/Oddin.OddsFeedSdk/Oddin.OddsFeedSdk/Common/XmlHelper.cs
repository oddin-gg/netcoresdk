using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Oddin.OddsFeedSdk.Common;

internal static class XmlHelper
{
    internal static bool TryDeserialize<T>(string xml, out T result)
    {
        try
        {
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);

            var serializer = new XmlSerializer(typeof(T));
#if DEBUG
            serializer.UnknownAttribute += (s, args) =>
                Console.WriteLine(
                    $"[{typeof(T)}] Unknown attribute {args.Attr.Name}='{args.Attr.Value}' Raw XML:{xml}");
            serializer.UnknownNode += (s, args) =>
                Console.WriteLine($"[{typeof(T)}] Unknown Node:{args.Name}  {args.Text} Raw XML:{xml}");
            serializer.UnknownElement += (s, args) =>
                Console.WriteLine(
                    $"[{typeof(T)}] Unknown Element:{args.Element.Name} | {args.Element.InnerXml} Raw XML:{xml}");
            serializer.UnreferencedObject += (s, args) =>
                Console.WriteLine(
                    $"[{typeof(T)}] Unreferenced Object:{args.UnreferencedId} | {args.UnreferencedObject} Raw XML:{xml}");
#endif
            if (serializer.CanDeserialize(xmlReader) == false)
            {
                result = default;
                return false;
            }

            result = (T)serializer.Deserialize(xmlReader);
            return true;
        }
        catch (InvalidOperationException e)
        {
#if DEBUG
            Console.WriteLine($"[{typeof(T)}] Exception when Deserializing {e}");
#endif
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