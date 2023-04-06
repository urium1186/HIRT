using LibHIRT.Domain;
using LibHIRT.Domain.RenderModel;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Serializers
{
    public class RenderGeometrySerializer : SerializerBase<RenderGeometry>
    {
        private static string _filePath;

        public static RenderGeometry Deserialize(Stream stream,string filePath)
        {
            
            _filePath = filePath;
            var reader = new BinaryReader(stream);
            return new RenderGeometrySerializer().Deserialize(reader);
        }

        protected override void OnDeserialize(BinaryReader reader, RenderGeometry obj)
        {
            
        }
    }
}
