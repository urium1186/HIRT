using OpenSpartan.Grunt.Models.ApiIngress;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibHIRT.Grunt.Converters
{
    public class OnlineUriReferenceConverter : JsonConverter<OnlineUriReference?>
    {
        public override OnlineUriReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                //reader.Skip();
                var result = new OnlineUriReference();
                reader.Read();
                reader.Read();
                result.AuthorityId = reader.GetString();
                reader.Read();
                reader.Read();
                result.Path = reader.GetString();
                reader.Read();
                reader.Read();
                result.RetryPolicyId = reader.GetString();
                reader.Read();
                reader.Read();
                result.TopicName = reader.GetString();
                reader.Read();
                reader.Read();
                string strAcknowledgementType = reader.GetString();
                //result.AcknowledgementTypeId =  AcknowledgementType reader.GetString();
                reader.Read();
                reader.Read();
                result.AuthenticationLifetimeExtensionSupported = reader.GetBoolean();
                reader.Read();
                reader.Read();
                result.ClearanceAware = reader.GetBoolean();
                reader.Read();
                return result;
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
