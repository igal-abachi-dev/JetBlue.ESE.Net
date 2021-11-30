
using Newtonsoft.Json;
//using System.Text.Json.Serialization;
using System;
using System.Xml;

namespace JetBlue.ESE.Net.Serialization
{
    internal class Iso8601TimeSpanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                string str = XmlConvert.ToString((TimeSpan)value);
                writer.WriteValue(str);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = reader.Value as string;
            return string.IsNullOrWhiteSpace(s) ? (object)null : (object)XmlConvert.ToTimeSpan(s);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }
    }
}
