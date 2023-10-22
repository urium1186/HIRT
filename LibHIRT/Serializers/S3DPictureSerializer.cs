using LibHIRT.Data.Textures;
using LibHIRT.Files;
using LibHIRT.Files.FileTypes;
using LibHIRT.TagReader;
using LibHIRT.TagReader.Headers;
using System;
using System.Diagnostics;
using System.Drawing;
using static LibHIRT.Assertions;
using String = System.String;

namespace LibHIRT.Serializers
{

    public class S3DPictureSerializer : SerializerBase<S3DPicture>
    {

        #region Constants

        private const int SIGNATURE_PICT = 0x50494354; // TCIP
        private static PictureFile? _file;

        #endregion

        #region Overrides

        protected override void OnDeserialize(BinaryReader reader, S3DPicture pict)
        {
            //tagParse = new TagParseControl("", "bitm", null, reader.BaseStream);
            //tagParse.readFile();
            tagParse = _file.Deserialized().TagParse;

            Tagblock bitmaps = (Tagblock)tagParse.RootTagInst["bitmaps"];

            if (bitmaps == null || !(_file.CurrentBitmapIndex < bitmaps.Count))
                return;
            _file.BitmapsCount = bitmaps.Count;

            TagInstance bitmap_i = bitmaps[_file.CurrentBitmapIndex];
            TagInstance bitmapRH = null;
            var bitmap_resource_handle = bitmap_i["bitmap resource handle"] as ResourceHandle;
            SSpaceFile getChunkFrom = _file as SSpaceFile;
            TagFile toUse = null;
            SSpaceFile temp_file = null;

            if (bitmap_resource_handle == null || bitmap_resource_handle.Count == 0)
            {
                Debug.Assert(bitmaps.Count >= 1);
                temp_file = (SSpaceFile)_file.GetResourceAt(_file.CurrentBitmapIndex);
                if (temp_file==null)
                    throw new IndexOutOfRangeException("Index out of range.");
                TagParseControl tagParseTemp = temp_file.Deserialized().TagParse;
                string hash = BitConverter.ToString(BitConverter.GetBytes(tagParseTemp.TagFile.TagHeader.TagFileHeaderInst.TypeHash)).Replace("-", "");
                var tempTaglay = tagParseTemp.getSubTaglayoutFrom(temp_file.FileMemDescriptor.ParentOffResourceRef?.TagGroup, hash);
                if (tempTaglay != null)
                {
                    tagParseTemp.readFile(tempTaglay, tagParseTemp.TagFile);
                    if (tagParseTemp.RootTagInst != null)
                    {
                        bitmapRH = tagParseTemp.RootTagInst;
                        getChunkFrom = temp_file;
                        toUse = tagParseTemp.TagFile;
                    }
                }
                else
                    throw new NotImplementedException("Sin implementar, cuando son externos");

            }
            else {
                Debug.Assert(bitmaps.Count ==1 );
                bitmapRH = (TagInstance)bitmap_i.GetObjByPath("bitmap resource handle.[0]");
            }
                

            bool canGet2k = false;
            bool haveExtra2kFile = bitmapRH["highResMipCountAndFlags"].AccessValue.ToString() == "1";
            if (haveExtra2kFile)
            {
                canGet2k = ((_file as SSpaceFile).Parent as ModuleFile).hasHd1File();
            }



            pict.Width = (Int16)bitmap_i["width"].AccessValue;
            pict.Height = (Int16)bitmap_i["height"].AccessValue;
            pict.Depth = (Int16)bitmap_i["depth"].AccessValue;
            pict.Faces = 1;
            var tv = bitmap_i["type"];
            pict.Type = (string)tv.GetType().GetProperty("Selected").GetValue(tv);
            var bm = bitmap_i["format"];
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
            pict.MipMapCount = (sbyte)bitmap_i["mipmap count"].AccessValue;

            pict.MipMapCount = (sbyte)bitmapRH["mipCountPerArraySlice"].AccessValue;

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
                        /*if ((_file as SSpaceFile).FileMemDescriptor.ResourceFiles.Count > index) {
                            chunkPath = (_file as SSpaceFile).FileMemDescriptor.ResourceFiles[index].Path_string;
                        }*/
                        ISSpaceFile _File = null;
                        _File = getChunkFrom.GetResourceAt(index);
                       
                        //Debug.Assert(canGet2k == false);
                        //ISSpaceFile _File = (_file as SSpaceFile).Resource_list[index]; 
                        // FileDirModel file = HIFileContext.RootDir.GetChildByPath(chunkPath) as FileDirModel;
                        if (_File != null)
                        {
                            HIRTStream stream = null;
                            try
                            {
                                stream = _File.GetStream();

                            }
                            catch (AssertionFailedException ex)
                            {
                                if (ex.Message.Contains("no hd1 file for module"))
                                {
                                    Debug.Assert(canGet2k == false);
                                    _file.MaxResMipMapIndex = 1;
                                    //throw ex;
                                }
                                else
                                    throw ex;
                            }
                            
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
                            else
                            {

                                arrays[i] = new byte[temp_z];
                                full_size_f += (int)temp_z;
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
                if (toUse == null)
                {
                    pict.Data = tagParse.TagFile.TagHeader.getSesion3Bytes(reader.BaseStream);
                }
                else
                {
                    pict.Data = toUse.TagHeader.getSesion3Bytes(temp_file.GetStream());
                }

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

        protected void readUnder2k()
        {

        }
        #endregion

        #region Public Methods

        public static S3DPicture Deserialize(Stream stream, PictureFile file)
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
