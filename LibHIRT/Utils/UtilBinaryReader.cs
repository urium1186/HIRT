using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LibHIRT.TagReader.Headers.TagHeader;

namespace LibHIRT.Utils
{
    public static class UtilBinaryReader
    {
        public static T marshallBinData<T>(byte[] buffer) {
            GCHandle tempHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T result = (T)Marshal.PtrToStructure(tempHandle.AddrOfPinnedObject(), typeof(T));
            tempHandle.Free();
            return result;
        }

        public static string readStringFromOffset(BinaryReader stream, long offset, bool inplace= false)
        {
            var init_pos = stream.BaseStream.Position;
            stream.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte w = 0x1;
            List<byte> temp = new List<byte>();
            while (w != 0x00) {
                w = stream.ReadByte();
                if (w != 0x00)
                    temp.Add(w);
            }
                
            var strPath = Encoding.ASCII.GetString(temp.ToArray<byte>());
            if (inplace)
                stream.BaseStream.Seek(init_pos, SeekOrigin.Begin);
            return strPath;
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            var result = Convert.ToString(b, 2).PadLeft(8, '0');
            
            
            var bit = (b & (1 << bitNumber)) != 0;
            return bit;
        }

        public static byte[] GetBytesFormStringBit(string binaryStr) {
            var byteArray = Enumerable.Range(0, int.MaxValue / 8)
                                      .Select(i => i * 8)    // get the starting index of which char segment
                                      .TakeWhile(i => i < binaryStr.Length)
                                      .Select(i => binaryStr.Substring(i, 8)) // get the binary string segments
                                      .Select(s => Convert.ToByte(new string(s.ToCharArray().Reverse().ToArray()), 2)) // convert to byte
                                      .ToArray();
            return byteArray;
        }
    }
}
