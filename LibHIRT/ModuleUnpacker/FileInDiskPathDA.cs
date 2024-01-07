using LibHIRT.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.ModuleUnpacker
{
    public class FileInDiskPathDA
    {
        static private SQLiteConnection connectionDb = null;

        public static SQLiteConnection ConnectionDb { get { 
                if (connectionDb==null)
                    connectionDb = SQLiteDriver.CreateConnection();
                ; return connectionDb; } }

        private static object locker = new object();

        static public bool insertToDbInDiskPath(string path_string, int file_id, int module_id, string ref_path = "")
        {
            var result = true;

            lock (locker) {
                try
                {
                    if (ConnectionDb.State != ConnectionState.Open)
                    {
                        ConnectionDb.Open();
                    }
                    SQLiteDriver.InsertInDiskPath(ConnectionDb, path_string, file_id, module_id, ref_path);
                }
                catch (Exception ex)
                {
                    SQLiteDriver.RemoveConnection(ConnectionDb);
                    result = false;
                }
            }

            
            return result;
        }

        static public bool updateToDbInDiskPath(string path_string, int file_id, int module_id, string ref_path = "")
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            var result = true;
            try
            {
                SQLiteDriver.UpdateInDiskPath(connectionDb, path_string, file_id, module_id, ref_path);
                SQLiteDriver.RemoveConnection(connectionDb);
            }
            catch (Exception ex)
            {
                SQLiteDriver.RemoveConnection(connectionDb);
                result = false;
            }

            
            return result;
        }
        static public bool getFromDbInDiskPath(int module_id, out List<Dictionary<string, object>> salida)
        {
            var result = true;
            lock (locker)
            {
                try
                {
                    if (ConnectionDb.State != ConnectionState.Open)
                    {
                        ConnectionDb.Open();
                    }
                    salida = SQLiteDriver.GetInDiskPath(ConnectionDb, module_id);
                    SQLiteDriver.RemoveConnection(ConnectionDb);
                }
                catch (Exception ex)
                {
                    SQLiteDriver.RemoveConnection(ConnectionDb);
                    result = false;
                    salida = null;
                }
            }

            return result;
        }
        
        static public bool getFromDbInDiskPath( int file_id, int module_id,out List<Dictionary<string, object>>  salida, string path_string = "")
        {
            var result = true;
            lock (locker)
            {
                try
                {
                    if (ConnectionDb.State != ConnectionState.Open)
                    {
                        ConnectionDb.Open();
                    }
                    salida = SQLiteDriver.GetInDiskPath(ConnectionDb, file_id, module_id, path_string);
                    SQLiteDriver.RemoveConnection(ConnectionDb);
                }
                catch (Exception ex)
                {
                    SQLiteDriver.RemoveConnection(ConnectionDb);
                    result = false;
                    salida = null;
                }
            }
            
            return result;
        }

    }
}
