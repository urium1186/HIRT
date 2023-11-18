using LibHIRT.Grunt.Converters;
using System.Text.Json;

namespace LibHIRT.Grunt
{
    public static class JsonSerializerFix
    {
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new EmptyDateStringToNullJsonConverter(),
                new OnlineUriReferenceConverter(),
                new AcknowledgementTypeConverter(),
                new XmlDurationToTimeSpanJsonConverter()
            }
        };

        public static JsonSerializerOptions SerializerOptions => serializerOptions;
    }
}
