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
        public static string GetPreferencesPath()
        {
            return Path.Combine(GetUserAppPath(), "HIRT.prefs");
        }
    }
}
