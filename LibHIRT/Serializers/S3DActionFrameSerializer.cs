using LibHIRT.Data;

namespace LibHIRT.Serializers
{

    public class S3DActionFrameSerializer : SerializerBase<List<S3DActionFrame>>
    {

        protected override void OnDeserialize(BinaryReader reader, List<S3DActionFrame> obj)
        {
            // TODO: This is never used?
            throw new NotImplementedException();
        }

    }

}
