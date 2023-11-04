using LibHIRT.TagReader;
using static LibHIRT.Assertions;

namespace LibHIRT.Serializers
{

    public abstract class SerializerBase<T>
    where T : class, new()
    {
        #region Public Methods

        public T Deserialize(BinaryReader reader)
        {
            var obj = new T();
            OnDeserialize(reader, obj);
            return obj;
        }

        public T Deserialize(TagInstance tagInstance)
        {
            var obj = new T();
            OnDeserialize(tagInstance, obj);
            return obj;
        }

        #endregion

        #region Abstract Methods

        protected abstract void OnDeserialize(BinaryReader reader, T obj);
        protected virtual void OnDeserialize(TagInstance tagInstance, T obj) { 
        
        }

        #endregion

        #region Helper Methods

        protected uint ReadSignature(BinaryReader reader, uint expectedSignature)
        {
            var actualSignature = reader.ReadUInt32();
            Assert(actualSignature == expectedSignature,
              $"The signature that was read ({actualSignature:X}) does not match " +
              $"the signature that was provided ({expectedSignature:X}).");

            return actualSignature;
        }

        #endregion

    }

}
