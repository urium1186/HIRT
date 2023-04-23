using LibHIRT.DAO;
using LibHIRT.Utils;
using System.Collections.Concurrent;
using System.Text;

namespace LibHIRT.TagReader
{
    public static class Mmr3HashLTU
    {

        static private ConcurrentDictionary<int, string> _Mmr3lTU;
        static private ConcurrentDictionary<int, string> _Mmr3Collaide1 = new ConcurrentDictionary<int, string>();
        static private ConcurrentDictionary<int, string> _Mmr3Collaide2 = new ConcurrentDictionary<int, string>();
        static private ConcurrentDictionary<int, string> _Mmr3Collaide3 = new ConcurrentDictionary<int, string>();
        static private ConcurrentDictionary<int, string> _Mmr3Collaide4 = new ConcurrentDictionary<int, string>();

        public static readonly bool ForceFillData = false;

        public static ConcurrentDictionary<int, string> Mmr3lTU { get {
                if (_Mmr3lTU == null)
                    _Mmr3lTU = new ConcurrentDictionary<int, string>();
                return _Mmr3lTU;
            } }

        public static int getMmr3HashIntFrom(string str_in) {
            Encoding encoding = new UTF8Encoding();
            byte[] input = encoding.GetBytes(str_in);
            using (MemoryStream stream = new MemoryStream(input))
            {
                return MurMurHash3.Hash(stream);
            }
        }


        static public void saveLtu(bool changed) {
            /*
            var connectionDb = SQLiteDriver.CreateConnection();
            foreach (var item in Mmr3HashLTU.Mmr3lTU)
            {
                try
                {
                    SQLiteDriver.InsertMmh3LTU(connectionDb, item.Key, item.Value);
                }
                catch (Exception ex)
                {

                }
            }
            connectionDb.Close();
            */
        }

        static public void saveToDbLtu(int hash, string str_value, bool in_use, bool generate)
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            
            try
            {
                SQLiteDriver.InsertMmh3LTU(connectionDb, hash, str_value, in_use, generate);
            }
            catch (Exception ex)
            {

            }
            
            connectionDb.Close();
        } 
        
        static public void loadFromDbLtu()
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            
            try
            {
                SQLiteDriver.ReadData(connectionDb, Mmr3HashLTU.Mmr3lTU);
            }
            catch (Exception ex)
            {

            }
            
            connectionDb.Close();
        }
        static public void insertToDbLtuCollaide(int hash, string str_value)
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            
            try
            {
                SQLiteDriver.InsertDataCollaide(connectionDb, hash, str_value);
            }
            catch (Exception ex)
            {

            }
            
            connectionDb.Close();
        }
         static public void updateToDbLtu(int hash, string str_value, bool in_use, bool generate)
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            
            try
            {
                SQLiteDriver.UpdateMmh3LTU(connectionDb, hash, str_value, in_use, generate);
            }
            catch (Exception ex)
            {

            }
            
            connectionDb.Close();
        }


        public static void AddUniqueStrValue(string value) {
            if (!string.IsNullOrEmpty(value))
            {
                int key = Mmr3HashLTU.getMmr3HashIntFrom(value);
                if (!Mmr3HashLTU.Mmr3lTU.TryAdd(key, value))
                {
                    if (Mmr3HashLTU.Mmr3lTU[key] != value)
                    {
                        if (Mmr3HashLTU.Mmr3lTU[key] == Mmr3HashLTU.getMmr3HashFromInt(key))
                        {
                            Mmr3HashLTU.Mmr3lTU[key] = value;
                            updateToDbLtu(key, value, true, true);
                        }
                        else
                        {
                            if (!_Mmr3Collaide1.TryAdd(key, value))
                                if (!_Mmr3Collaide2.TryAdd(key, value))
                                    if (!_Mmr3Collaide3.TryAdd(key, value))
                                        if (!_Mmr3Collaide4.TryAdd(key, value))
                                        {
                                        }
                            insertToDbLtuCollaide(key, value);
                        }

                    }
                }
                else {
                    saveToDbLtu(key, value, false, true);
                }
            }
        }

        public static void AddUniqueIntHash(int key)
        {
            if (Mmr3HashLTU.Mmr3lTU.TryAdd(key, Mmr3HashLTU.getMmr3HashFromInt(key)))
            {
                saveToDbLtu(key, Mmr3HashLTU.getMmr3HashFromInt(key), true, false);
            }
            else {
                if (Mmr3HashLTU.Mmr3lTU[key] != Mmr3HashLTU.getMmr3HashFromInt(key))
                    updateToDbLtu(key, Mmr3HashLTU.Mmr3lTU[key], true, true);
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
