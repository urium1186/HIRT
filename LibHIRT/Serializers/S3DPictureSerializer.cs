using LibHIRT.Data.Textures;
using LibHIRT.Files;
using LibHIRT.TagReader;
using System.Diagnostics;
using static LibHIRT.Assertions;
using String = System.String;

namespace LibHIRT.Serializers
{

    public class S3DPictureSerializer : SerializerBase<S3DPicture>
    {

        #region Constants

        private const int SIGNATURE_PICT = 0x50494354; // TCIP
        private static ISSpaceFile? _file;

        #endregion

        #region Overrides

        protected override void OnDeserialize(BinaryReader reader, S3DPicture pict)
        {
            tagParse = new TagParseControl("", "bitm", null, reader.BaseStream);
            tagParse.readFile();

            TagInstance bitmap = (TagInstance)tagParse.RootTagInst.GetObjByPath("bitmaps.[0]");
            TagInstance bitmapRH = (TagInstance)bitmap.GetObjByPath("bitmap resource handle.[0]");



            pict.Width = (Int16)tagParse.RootTagInst.GetObjByPath("bitmaps.[0].width").AccessValue;
            pict.Height = (Int16)tagParse.RootTagInst.GetObjByPath("bitmaps.[0].height").AccessValue;
            pict.Depth = (Int16)tagParse.RootTagInst.GetObjByPath("bitmaps.[0].depth").AccessValue;
            pict.Faces = 1;
            var tv = bitmap.GetObjByPath("type");
            pict.Type = (string)tv.GetType().GetProperty("Selected").GetValue(tv);
            var bm = tagParse.RootTagInst.GetObjByPath("bitmaps.[0].format");
            string val = (string)bm.GetType().GetProperty("Selected").GetValue(bm);
            //(new System.Collections.Generic.IDictionaryDebugView<string, object>(new System.Collections.Generic.ICollectionDebugView<object>((new System.Collections.Generic.IDictionaryDebugView<string, object>(((LibHIRT.TagReader.ParentTagInstance)tagParse.RootTagInst).AccessValue).Items[32]).Value).Items[0]).Items[0]).Value
            //Selected bc7_unorm
            var formatValue = 0;

            /*
            Assert(Enum.IsDefined(typeof(S3DPictureFormat), format),
              $"Unknown DDS Format Value: {formatValue:X}");
            */
            pict.Format = S3DPictureFormat.UNSET;
            pict.SFormat = val;
            pict.MipMapCount = (sbyte)tagParse.RootTagInst.GetObjByPath("bitmaps.[0].mipmap count").AccessValue;



            pict.MipMapCount = (sbyte)bitmap.GetObjByPath("bitmap resource handle.[0].mipCountPerArraySlice").AccessValue;

            int pixels = (int)bitmapRH.GetObjByPath("pixels").AccessValue;
            if (pixels == 0)
            {
                ListTagInstance bitmapRH_SD = (ListTagInstance)bitmapRH.GetObjByPath("streamingData");
                Debug.Assert(bitmapRH_SD.Childs.Count != 0);
                //pict.MipMapCount = bitmapRH_SD.Childs.Count;
                int full_size = 0;
                int full_size_f = 0;
                if (bitmapRH_SD.Childs.Count != 0 && _file != null)
                {
                    byte[][] arrays = new byte[bitmapRH_SD.Childs.Count][];
                    for (int i = 0; i < bitmapRH_SD.Childs.Count; i++)
                    {
                        int temp_z = (int)bitmapRH_SD.Childs[i].GetObjByPath("size").AccessValue;
                        /*int temp_offset = (int)bitmapRH_SD.Childs[i].GetObjByPath("offset");
                        int chunkInfo = (byte)bitmapRH_SD.Childs[i].GetObjByPath("mipmap index");*/
                        Int32 chunkInfo = (Int32)bitmapRH_SD.Childs[i].GetObjByPath("chunkInfo").AccessValue;
                        int intValue = chunkInfo;
                        byte[] intBytes = BitConverter.GetBytes(intValue);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(intBytes);
                        byte[] result = intBytes;
                        int index = i;
                        //int index = result[1];
                        full_size += temp_z;
                        string sub_ext = String.Format("[{0}:bitmap resource handle.chunk{0}]", index);
                        string chunkPath = _file.Path_string + sub_ext;
                        if ((_file as SSpaceFile).FileMemDescriptor.ResourceFiles.Count > index)
                        {
                            chunkPath = (_file as SSpaceFile).FileMemDescriptor.ResourceFiles[index].Path_string;
                        }
                        FileDirModel file = HIFileContext.RootDir.GetChildByPath(chunkPath) as FileDirModel;
                        if (file != null && file.File != null)
                        {
                            var stream = file.File.GetStream();
                            if (stream != null)
                            {
                                //Debug.Assert(stream.Length == temp_z);
                                long len = stream.Length;
                                int l_offset = 0;
                                /*if (i == 0) {
                                    len = len - 3472;
                                    l_offset = 3472;
                                }*/

                                stream.Seek(l_offset, SeekOrigin.Begin);
                                arrays[i] = new byte[len];

                                stream.Read(arrays[i], 0, (int)len);
                                full_size_f += (int)len;
                            }
                        }
                    }
                    Array.Reverse<byte[]>(arrays);
                    pict.Data = Utils.Utils.Combine(arrays);
                }
                if (full_size == 0)
                    pict.Data = new byte[full_size_f];
            }
            else
            {
                pict.Data = tagParse.TagFile.TagHeader.getSesion3Bytes(reader.BaseStream);
            }

            /*
      while ( reader.BaseStream.Position < reader.BaseStream.Length )
      {
        var sentinel = ( PictureSentinel ) reader.ReadUInt16();
        var endOffset = reader.ReadUInt32();

        switch ( sentinel )
        {
          case PictureSentinel.Header:
            ReadHeader( reader, pict );
            break;
          case PictureSentinel.Dimensions:
            ReadDimensions( reader, pict );
            break;
          case PictureSentinel.Format:
            ReadFormat( reader, pict );
            break;
          case PictureSentinel.MipMaps:
            ReadMipMaps( reader, pict );
            break;
          case PictureSentinel.TagData:
            ReadData( reader, pict, endOffset );
            break;
          case PictureSentinel.Footer:
            break;
          default:
            Fail( $"Unknown Picture Sentinel: {sentinel:X}" );
            break;
        }

        Assert( reader.BaseStream.Position == endOffset,
          "Reader position does not match the picture sentinel's end offset." );
      }*/
        }

        #endregion

        #region Public Methods

        public static S3DPicture Deserialize(Stream stream, ISSpaceFile? file = null)
        {
            var reader = new BinaryReader(stream);
            _file = file;
            return new S3DPictureSerializer().Deserialize(reader);
        }

        #endregion

        #region Private Methods

        private void ReadHeader(BinaryReader reader, S3DPicture pict)
        {
            Assert(reader.ReadInt32() == SIGNATURE_PICT, "Invalid PICT signature.");
        }

        private void ReadDimensions(BinaryReader reader, S3DPicture pict)
        {
            pict.Width = reader.ReadInt32();
            pict.Height = reader.ReadInt32();
            pict.Depth = reader.ReadInt32();
            pict.Faces = reader.ReadInt32();
        }

        private void ReadFormat(BinaryReader reader, S3DPicture pict)
        {
            var formatValue = reader.ReadInt32();
            var format = (S3DPictureFormat)formatValue;

            Assert(Enum.IsDefined(typeof(S3DPictureFormat), format),
              $"Unknown DDS Format Value: {formatValue:X}");

            pict.Format = format;
        }

        private void ReadMipMaps(BinaryReader reader, S3DPicture pict)
        {
            pict.MipMapCount = reader.ReadInt32();
        }

        private void ReadData(BinaryReader reader, S3DPicture pict, long endOffset)
        {
            var dataSize = endOffset - reader.BaseStream.Position;

            pict.Data = new byte[dataSize];
            reader.Read(pict.Data, 0, (int)dataSize);
        }

        #endregion

        #region Embedded Types

        private enum PictureSentinel : ushort
        {
            Header = 0xF0,
            Dimensions = 0x0102,
            Format = 0xF2,
            MipMaps = 0xF9,
            Data = 0xFF,
            Footer = 0x01
        }

        #endregion

    }

}
