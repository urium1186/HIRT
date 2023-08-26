using OpenSpartan.Grunt.Models.HaloInfinite;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Common.Grunt.Converters
{
    internal class SkipNodeConverter : JsonConverter<SkipNode?>
    {
        public override SkipNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Skip();
                return new SkipNode();
            }
            else
            {
                reader.Read();
                return null;
            }

        }

        public override void Write(Utf8JsonWriter writer, SkipNode? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
