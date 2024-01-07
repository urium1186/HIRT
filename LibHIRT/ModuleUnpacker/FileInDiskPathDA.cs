using LibHIRT.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.ModuleUnpacker
{
    public class FileInDiskPathDA
    {
        static public bool insertToDbInDiskPath(string path_string, int file_id, int module_id, string ref_path = "")
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            var result = true;
            try
            {
                SQLiteDriver.InsertInDiskPath(connectionDb, path_string, file_id, module_id, ref_path);
            }
            catch (Exception ex)
            {
                result = false;
            }

            connectionDb.Close();
            return result;
        }

        static public bool updateToDbInDiskPath(string path_string, int file_id, int module_id, string ref_path = "")
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            var result = true;
            try
            {
                SQLiteDriver.UpdateInDiskPath(connectionDb, path_string, file_id, module_id, ref_path);
            }
            catch (Exception ex)
            {
                result = false;
            }

            connectionDb.Close();
            return result;
        }

        static public bool getFromDbInDiskPath( int file_id, int module_id,out List<Dictionary<string, object>>  salida, string path_string = "")
        {
            var connectionDb = SQLiteDriver.CreateConnection();
            var result = true;
            try
            {
               salida= SQLiteDriver.GetInDiskPath(connectionDb, file_id, module_id, path_string);
            }
            catch (Exception ex)
            {
                result = false;
                salida = null;
            }

            connectionDb.Close();
            return result;
        }

    }
}
