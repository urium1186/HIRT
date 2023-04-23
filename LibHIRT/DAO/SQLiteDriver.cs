using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace LibHIRT.DAO
{
    public class SQLiteDriver
    {
        public static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn = null;
            // Create a new database connection:
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\db\\database.db";
            if (File.Exists(filePath)) {
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
        
        public static void InsertMmh3LTU(SQLiteConnection conn, int int_hash,string str_value, bool in_use, bool generate)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO StringMmh3LTU (mmh3_id, str_value, in_use , generated) VALUES(" + int_hash.ToString()+",'"+str_value+"','"+(in_use?"1":"0")+ "','"+ (generate ? "1" : "0") +"'); ";
            sqlite_cmd.ExecuteNonQuery();

        }
        
        public static void InsertDataCollaide(SQLiteConnection conn, int int_hash, string str_value)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO StringMmh3LTU (mmh3_id, str_value) VALUES("+int_hash.ToString()+",'"+str_value+"'); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public static void UpdateMmh3LTU(SQLiteConnection conn, int int_hash, string str_value, bool in_use, bool generate)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "UPDATE StringMmh3LTU SET in_use = '" + (in_use ? "1" : "0") + "', generated = '" + (generate ? "1" : "0")+ "', str_value = '"+ str_value + "' WHERE mmh3_id = '" + int_hash + "';"; 
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
    }
}
