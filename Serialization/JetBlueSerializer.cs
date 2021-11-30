using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
//todo: Utf8Json vs .net 6 source gen sys.txt.json
//use https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
//using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using System.IO;

namespace JetBlue.ESE.Net.Serialization
{
    public static class JetBlueSerializer
    {
        private static readonly JsonSerializer Serializer = JetBlueSerializer.MakeSerializer();

        public static T Deserialize<T>(JsonReader reader) => JetBlueSerializer.Serializer.Deserialize<T>(reader);
        public static T Deserialize<T>(string text) => JetBlueSerializer.Serializer.Deserialize<T>((JsonReader)new JsonTextReader((TextReader)new StringReader(text)));

        public static string Serialize(object value)
        {
            StringWriter stringWriter = new StringWriter();
            JetBlueSerializer.Serializer.Serialize((JsonWriter)new JsonTextWriter((TextWriter)stringWriter), value);
            return stringWriter.ToString();
        }

        private static JsonSerializer MakeSerializer()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new Iso8601TimeSpanConverter());
            jsonSerializer.Converters.Add(new StringEnumConverter());
            jsonSerializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            return jsonSerializer;
        }
    }
}
