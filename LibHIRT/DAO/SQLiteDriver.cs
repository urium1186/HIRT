using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Data.SQLite;

namespace LibHIRT.DAO
{
    public class SQLiteDriver
    {
        public static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn = null;
            // Create a new database connection:
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\db\\database.db";
            if (File.Exists(filePath))
            {
                sqlite_conn = new SQLiteConnection("Data Source=" + filePath + "; Version = 3; New = True; Compress = True; ");
                // Open the connection:
                try
                {
                    sqlite_conn.Open();
                }
                catch (Exception ex)
                {

                }
            }

            return sqlite_conn;
        }

        public static void CreateTable(SQLiteConnection conn)
        {
            //SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}';
            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE SampleTable (Col1 VARCHAR(20), Col2 INT)";
            string Createsql1 = "CREATE TABLE SampleTable1 (Col1 VARCHAR(20), Col2 INT)";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = Createsql1;
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void InsertData(SQLiteConnection conn)
        {
            return;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO StringMmh3LTU (mmh3_id, str_value) VALUES('Test Text ', 1); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable (Col1, Col2) VALUES('Test1 Text1 ', 2); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable (Col1, Col2) VALUES('Test2 Text2 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();


            sqlite_cmd.CommandText = "INSERT INTO SampleTable1 (Col1, Col2) VALUES('Test3 Text3 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void InsertMmh3LTU(SQLiteConnection conn, int int_hash, string str_value, bool in_use, bool generate)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO StringMmh3LTU (mmh3_id, str_value, in_use , generated) VALUES(" + int_hash.ToString() + ",'" + str_value + "','" + (in_use ? "1" : "0") + "','" + (generate ? "1" : "0") + "'); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void InsertDataCollaide(SQLiteConnection conn, int int_hash, string str_value)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO StringMmh3LTU (mmh3_id, str_value) VALUES(" + int_hash.ToString() + ",'" + str_value + "'); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void UpdateMmh3LTU(SQLiteConnection conn, int int_hash, string str_value, bool in_use, bool generate)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "UPDATE StringMmh3LTU SET in_use = '" + (in_use ? "1" : "0") + "', generated = '" + (generate ? "1" : "0") + "', str_value = '" + str_value + "' WHERE mmh3_id = '" + int_hash + "';";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void ReadData(SQLiteConnection conn, ConcurrentDictionary<int, string> _Mmr3lTU)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM StringMmh3LTU";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string value = sqlite_datareader.GetString(1);
                int key = sqlite_datareader.GetInt32(0);
                _Mmr3lTU.TryAdd(key, value);
            }
            conn.Close();
        }

        public static void InsertInDiskPath(SQLiteConnection conn, string path_string, int file_id, int module_id, string ref_path)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            
            sqlite_cmd.CommandText = "INSERT INTO InDiskPath (path_string, file_id, module_id, ref_path, mod_date) VALUES(" + path_string + ",'" + file_id.ToString() + ",'" + module_id.ToString() + "','" + ref_path + "','" + DateAndTime.Now.ToString() + "'); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void UpdateInDiskPath(SQLiteConnection conn, string path_string, int file_id, int module_id, string ref_path)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "UPDATE InDiskPath SET ref_path = '" + ref_path + "' WHERE file_id = " + file_id.ToString() + " and module_id = "+ module_id.ToString() + ";";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static List<Dictionary<string, object>> GetInDiskPath(SQLiteConnection conn, int file_id, int module_id, string path_string="")
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            string pathQ = path_string == "" ? "" : " and path_string = '" + module_id + "' ";
            sqlite_cmd.CommandText = "SELECT * FROM InDiskPath  WHERE file_id = " + file_id.ToString() + " and module_id = " + module_id.ToString() + path_string + ";";
            List<Dictionary<string,object>> result = new List<Dictionary<string, object>>();
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                Dictionary<string, object> temp = new Dictionary<string, object>();
                temp["path_string"] = sqlite_datareader.GetString(0);
                temp["file_id"] = sqlite_datareader.GetInt32(1);
                temp["module_id"] = sqlite_datareader.GetInt32(2);
                temp["ref_path"] = sqlite_datareader.GetString(3);
                temp["mod_date"] = sqlite_datareader.GetString(4);
                result.Add(temp);
            }
            conn.Close();
            return result;
        }
    }
}
