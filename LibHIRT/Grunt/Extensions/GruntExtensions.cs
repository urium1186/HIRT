using OpenSpartan.Grunt.Models;
using System.Reflection;

namespace LibHIRT.Grunt.Extensions
{
    public static class GruntExtensions
    {
        public static string? GetHeaderValue(this ApiContentType value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                ContentTypeAttribute[] array = field.GetCustomAttributes(typeof(ContentTypeAttribute), inherit: false) as ContentTypeAttribute[];
                if (array != null)
                {
                    if (array.Length == 0)
                    {
                        return null;
                    }

                    return array[0].HeaderValue;
                }

                return null;
            }

            return null;
        }
    }
}
