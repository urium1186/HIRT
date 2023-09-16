using OpenSpartan.Grunt.Models.ApiIngress;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloInfiniteResearchTools.Common.Grunt.Converters
{
    internal class OnlineUriReferenceConverter : JsonConverter<OnlineUriReference?>
    {
        public override OnlineUriReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Skip();
                return new OnlineUriReference();
            }
            else
            {
                reader.Read();
                return null;

            }

            //string @string = reader.Read();
            //if (!string.IsNullOrWhiteSpace(@string))
            //{

            //}



        }

        public override void Write(Utf8JsonWriter writer, OnlineUriReference? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
