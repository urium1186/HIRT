using LibHIRT.Common;
using LibHIRT.Data;
using static LibHIRT.Assertions;

namespace LibHIRT.Serializers
{

    public class S3DGeometryGraphSerializer : SerializerBase<S3DGeometryGraph>
    {

        #region Constants

        private const uint SIGNATURE_OGM1 = 0x314D474F; //OGM1

        #endregion

        protected override void OnDeserialize(BinaryReader reader, S3DGeometryGraph graph)
        {

            //ReadSignature(reader, SIGNATURE_OGM1);

            // TODO: These are guesses.
            /* The "GraphType" seems to denote the presence of some of these properties.
             * ReadObjectPsProperty() is very hacky, and doesn't seem to correlate with
             * either of these types.
             */
            /*
            var graphType = (GraphType)reader.ReadUInt16();
            var unk_02 = reader.ReadUInt16(); // TODO
            var unk_03 = reader.ReadUInt16(); // TODO

            ReadObjectsProperty(reader, graph);

            if (graphType == GraphType.Props)
                ReadObjectPropsProperty(reader, graph);


            if (graphType == GraphType.Grass)
            {
                ReadObjectPsProperty(reader, graph); // TODO: This is hacky.
                ReadLodRootsProperty(reader, graph);
            }
            */
            ReadData(reader, graph);
        }

        #region Property Read Methods

        private void ReadObjectsProperty(BinaryReader reader, S3DGeometryGraph graph)
        {
            // Read Sentinel
            if (reader.ReadByte() == 0)
                return;

            var objectSerializer = new S3DObjectSerializer(graph);
            graph.Objects = objectSerializer.Deserialize(reader);
        }

        private void ReadObjectPropsProperty(BinaryReader reader, S3DGeometryGraph graph)
        {
            // This section only seems to be present in ss_prop__h.tpl
            var count = reader.ReadByte();

            var unk_01 = reader.ReadByte(); // TODO
            var unk_02 = reader.ReadByte(); // TODO
            var unk_03 = reader.ReadByte(); // TODO
            var unk_04 = reader.ReadByte(); // TODO

            var props = graph.ObjectProps = new string[count];

            for (var i = 0; i < count; i++)
            {
                var unk_05 = reader.ReadUInt32(); // TODO
                props[i] = reader.ReadPascalString32();
            }
        }

        private void ReadObjectPsProperty(BinaryReader reader, S3DGeometryGraph graph)
        {
            var count = reader.ReadInt32();
            var unk_01 = reader.ReadByte(); // TODO

            // TODO: This is a hack.
            // Not yet sure how to tell if this property is present.
            if (unk_01 != 0x1)
            {
                reader.BaseStream.Position -= 5;
                return;
            }

            for (var i = 0; i < count; i++)
            {
                _ = reader.ReadInt32(); // TODO
                _ = reader.ReadPascalString32(); // TODO
            }

        }

        private void ReadLodRootsProperty(BinaryReader reader, S3DGeometryGraph graph)
        {
            var serializer = new S3DObjectLodRootSerializer();
            graph.LodRoots = serializer.Deserialize(reader);
        }

        #endregion

        #region Data Read Methods

        private void ReadData(BinaryReader reader, S3DGeometryGraph graph)
        {
            ReadBufferData(reader, graph);
            ReadMeshData(reader, graph);
            ReadSubMeshData(reader, graph);
        }

        private void ReadHeaderData(BinaryReader reader, S3DGeometryGraph graph)
        {
            var endOffset = reader.ReadUInt32();

            graph.RootNodeIndex = reader.ReadInt16();
            graph.NodeCount = reader.ReadUInt32();
            graph.BufferCount = reader.ReadUInt32();
            graph.MeshCount = reader.ReadUInt32();
            graph.SubMeshCount = reader.ReadUInt32();

            var unk_01 = reader.ReadUInt32(); // TODO
            var unk_02 = reader.ReadUInt32(); // TODO

            Assert(reader.BaseStream.Position == endOffset,
                "Reader position does not match data header's end offset.");
        }

        private void ReadBufferData(BinaryReader reader, S3DGeometryGraph graph)
        {
            var serializer = new S3DGeometryBufferSerializer(graph);
            serializer.TagParse = TagParse;
            graph.Buffers = serializer.Deserialize(reader);
        }

        private void ReadMeshData(BinaryReader reader, S3DGeometryGraph graph)
        {
            var serializer = new S3DGeometryMeshSerializer(graph);
            serializer.TagParse = TagParse;
            graph.Meshes = serializer.Deserialize(reader);
        }

        private void ReadSubMeshData(BinaryReader reader, S3DGeometryGraph graph)
        {
            var serializer = new S3DGeometrySubMeshSerializer(graph);
            serializer.TagParse = TagParse;
            graph.SubMeshes = serializer.Deserialize(reader);
        }

        private void ReadEndOfData(BinaryReader reader, S3DGeometryGraph graph)
        {
            var endOffset = reader.ReadUInt32();
            Assert(reader.BaseStream.Position == endOffset,
                "Reader position does not match data's end offset.");
        }

        #endregion

        #region Embedded Types

        private enum GraphType : ushort
        {
            Default = 1,
            Props = 3,
            Grass = 4
        }

        private enum DataSentinel : ushort
        {
            Header = 0x0000,
            Buffers = 0x0002,
            Meshes = 0x0003,
            SubMeshes = 0x0004,
            EndOfData = 0xFFFF
        }

        #endregion

    }

}
