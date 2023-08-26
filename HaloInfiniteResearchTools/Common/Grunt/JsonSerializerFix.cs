using HaloInfiniteResearchTools.Common.Grunt.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
