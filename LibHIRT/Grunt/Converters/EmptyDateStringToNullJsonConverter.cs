using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibHIRT.Grunt.Converters
{
    //
    // Summary:
    //     Converts an empty date string to a null. 343i is returning some ISO8601 dates
    //     that we wrap in OpenSpartan.Grunt.Models.APIFormattedDate as empty, which in
    //     turn breaks System.Text.Json deserialization.
    public class EmptyDateStringToNullJsonConverter : JsonConverter<DateTime?>
    {
        //
        // Summary:
        //     Read content from the JSON parser.
        //
        // Parameters:
        //   reader:
        //     Instance of System.Text.Json.Utf8JsonReader used to read the JSON content.
        //
        //   typeToConvert:
        //     JSON data to convert.
        //
        //   options:
        //     JSON serialization options.
        //
        // Returns:
        //     If successful, returns an instance of System.DateTime containing the date and
        //     time. Otherwise, returns null.
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string @string = reader.GetString();
            if (!string.IsNullOrWhiteSpace(@string))
            {
                return DateTime.Parse(@string);
            }

            return null;
        }

        //
        // Summary:
        //     Writes content through a JSON parser.
        //
        // Parameters:
        //   writer:
        //     Instance of System.Text.Json.Utf8JsonWriter that will be writing the JSON data.
        //
        //   value:
        //     Instance of System.DateTime containing the date and time to be written into JSON.
        //
        //   options:
        //     JSON serialization options.
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
