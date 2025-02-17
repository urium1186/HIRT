using System.IO.Compression;

namespace LibHIRT.Utils
{
    public static class Utils
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static bool HasNonASCIIChars(string str)
        {
            return (System.Text.Encoding.UTF8.GetByteCount(str) != str.Length);
        }

        public static string CreatePathFromString(string p_path, string root_path = "", string parent_folder = "")
        {

            if (string.IsNullOrEmpty(root_path) || !Directory.Exists(root_path))
            {
                //root_path = Directory.GetCurrentDirectory();
                root_path = GetUserAppPath();
            }
            if (!root_path.EndsWith('\\'))
            {
                root_path = root_path + "\\";
            }
            if (!string.IsNullOrEmpty(root_path))
            {
                root_path = root_path + parent_folder + "\\";
            }
            string full_path = Path.GetFullPath(root_path + p_path);
            Directory.CreateDirectory(Path.GetDirectoryName(full_path));
            if (Directory.Exists(Path.GetDirectoryName(full_path)))
                return full_path;
            return "";
        }

        public static string GetUserAppPath()
        {
            var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string directory_path = Path.Combine(userPath, "HIRT");
            if (!Directory.Exists(directory_path))
                Directory.CreateDirectory(directory_path);
            return directory_path;
        }
        public static string GetResourcesAppPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        }
        public static string GetPreferencesPath()
        {
            return Path.Combine(GetUserAppPath(), "HIRT.prefs");
        }

        public static string GetUserDbPath()
        {
            return Path.Combine(GetUserAppPath(), "user.db");
        }

        public static string GetUserDefaultXmlFolderPath()
        {
            string directory_path = Path.Combine(GetUserAppPath(), "xml_tags");
            if (!Directory.Exists(directory_path))
                Directory.CreateDirectory(directory_path);
            return directory_path;
        }
        public static bool DirectorioEstaVacio(string directorio)
        {
            return !Directory.EnumerateFileSystemEntries(directorio).Any();
        }
        public static string GetUserXboxTokenPath()
        {
            string p_path = Path.Combine(GetUserAppPath(), "xboxservice", "tokens.json");
            string full_path = Path.GetFullPath(p_path);
            Directory.CreateDirectory(Path.GetDirectoryName(full_path));
            if (Directory.Exists(Path.GetDirectoryName(full_path)))
                return p_path;
            return "";
        }

        public static void DescomprimirArchivoZip(string archivoZip, string carpetaDestino)
        {
            // Asegurarse de que la carpeta de destino exista
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }
            if (!File.Exists(archivoZip))
                return;
            // Descomprimir el archivo ZIP
            ZipFile.ExtractToDirectory(archivoZip, carpetaDestino);

            Console.WriteLine("Descompresión completada.");
        }
    }
}
