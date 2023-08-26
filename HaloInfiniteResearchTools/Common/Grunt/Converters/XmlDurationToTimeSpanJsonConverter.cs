using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace HaloInfiniteResearchTools.Common.Grunt.Converters
{
    //
    // Summary:
    //     Helper class used for conversion of XML duration into standard time span.
    internal class XmlDurationToTimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        //
        // Summary:
        //     Read the XML duration.
        //
        // Parameters:
        //   reader:
        //     JSON reader.
        //
        //   typeToConvert:
        //     Type that needs to be converted.
        //
        //   options:
        //     Additional reading options.
        //
        // Returns:
        //     If successful, returns a System.TimeSpan instance.
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string @string = reader.GetString();
            if (!string.IsNullOrWhiteSpace(@string))
            {
                return XmlConvert.ToTimeSpan(@string);
            }

            return TimeSpan.Zero;
        }

        //
        // Summary:
        //     Writes the TimeSpan data to a pre-defined writer.
        //
        // Parameters:
        //   writer:
        //     JSON writer.
        //
        //   value:
        //     Instance of System.TimeSpan that needs to be written.
        //
        //   options:
        //     Additional writing options.
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(XmlConvert.ToString(value));
        }
    }
}
