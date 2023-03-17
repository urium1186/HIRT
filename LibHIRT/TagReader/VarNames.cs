using LibHIRT.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.TagReader
{
    public static class VarNames
    {
        public static int getMmr3HashIntFrom(string str_in) {
            Encoding encoding = new UTF8Encoding();
            byte[] input = encoding.GetBytes(str_in);
            using (MemoryStream stream = new MemoryStream(input))
            {
                return MurMurHash3.Hash(stream);
            }
        }
            
        public static string getMmr3HashFromInt(int hash) {
            var byt = BitConverter.GetBytes(hash);
            string hex = BitConverter.ToString(byt).Replace("-", "");
            return hex;
        }
        
        public static string getMmr3HashFrom(string str_in) {

            Encoding encoding = new UTF8Encoding();
            byte[] input = encoding.GetBytes(str_in);
            using (MemoryStream stream = new MemoryStream(input))
            {
                int hash = MurMurHash3.Hash(stream);
                //hash = 1651078253;
                uint h2 = MurMurHash3.murmur_hash_inverse((uint)hash, 0);
                uint salida = MurMurHash3.murmur_hash_inverse((uint)hash, 0);
                var t = BitConverter.GetBytes((int)salida);
                //new { Hash = hash, Bytes = BitConverter.GetBytes(hash) }.Dump("Result");
                //Console.WriteLine("Hash (" + hash+")" );
                //Console.WriteLine("Bytes (" + BitConverter.ToString(BitConverter.GetBytes(hash)).Replace("-", "") + ")" );
                var byt = BitConverter.GetBytes(hash);
                var byt_s = BitConverter.GetBytes(h2);
                string hex = BitConverter.ToString(byt).Replace("-", "");
                var byt1 = stringToByteArray(hex);
                int n_1 = fromStrHash(hex);
                var byt2 = Convert.FromBase64String(hex);
                int number = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                return hex;
            }
            return "";
        }

        public static int fromStrHash(string strHash)
        {
            var byt1 = stringToByteArray(strHash);
            return BitConverter.ToInt32(byt1);
        }
        public static byte[] stringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }
    }
}
