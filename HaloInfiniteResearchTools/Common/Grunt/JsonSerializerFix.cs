using HaloInfiniteResearchTools.Common.Grunt.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Common.Grunt
{
    public static class JsonSerializerFix
    {
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                (JsonConverter)new EmptyDateStringToNullJsonConverter(),
                (JsonConverter)new OnlineUriReferenceConverter(),
                (JsonConverter)new AcknowledgementTypeConverter(),
                (JsonConverter)new XmlDurationToTimeSpanJsonConverter()
            }
        };

        public static JsonSerializerOptions SerializerOptions => serializerOptions;
    }
}
