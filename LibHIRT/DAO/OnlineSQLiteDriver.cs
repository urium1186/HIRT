using Dapper;
using OpenSpartan.Grunt.Models.HaloInfinite;
using System.Data.SQLite;

namespace LibHIRT.DAO
{
    public class ArmorCoreThemeExtende : ArmorCoreTheme
    {
        public int id { get; set; }
        public string ThemeName { get; set; }
        public string CreatedDateUtc { get; set; }
        public string FirstModifiedDateUtc { get; set; }
        public string LastModifiedDateUtc { get; set; }
    }
    public class OnlineSQLiteDriver
    {
        SQLiteConnection sqlite_conn = null;
        public SQLiteConnection Connection
        {
            get
            {
                if (sqlite_conn != null)
                    return sqlite_conn;
                return CreateConnection();
            }
        }

        public OnlineSQLiteDriver()
        {
            sqlite_conn = null;
        }
        private SQLiteConnection CreateConnection()
        {

            // Create a new database connection:
            string filePath = Utils.Utils.GetUserDbPath();
            bool isNew = false;
            if (!System.IO.File.Exists(filePath))
            {
                var remp = System.IO.File.Create(filePath);
                remp.Close();
                isNew = true;
            }

            if (System.IO.File.Exists(filePath))
            {
                sqlite_conn = new SQLiteConnection("Data Source=" + filePath + "; Version = 3; New = True; Compress = True; ");
                // Open the connection:
                try
                {
                    sqlite_conn.OpenAsync().Wait();
                    if (isNew)
                        CreateTables();

                    //_connectionsQueue.Enqueue(sqlite_conn);
                }
                catch (Exception ex)
                {

                }
            }

            return sqlite_conn;
        }

        public void CreateTables()
        {
            //SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}';
            SQLiteCommand sqlite_cmd;

            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "PRAGMA foreign_keys = ON;";
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS ThemesImageTable;";
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS EmblemsOnThemes;";
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS CustomArmorCoresOwnThemes;";
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = "DROP TABLE IF EXISTS CoresTable;";
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            string Createsql = "CREATE TABLE IF NOT EXISTS CoresTable ( CoreId VARCHAR(40) NOT NULL PRIMARY KEY, CorePath VARCHAR(255), IsEquipped INTEGER, CoreType VARCHAR(30), FirstAcquiredDate VARCHAR(40));";

            string Createsql1 = "CREATE TABLE IF NOT EXISTS CustomArmorCoresOwnThemes (id INTEGER PRIMARY KEY, ThemeName VARCHAR(50),CoatingPath VARCHAR(255), GlovePath VARCHAR(255), HelmetPath VARCHAR(255), HelmetAttachmentPath VARCHAR(255), ChestAttachmentPath VARCHAR(255)" +
                ", KneePadPath VARCHAR(255), LeftShoulderPadPath VARCHAR(255), RightShoulderPadPath VARCHAR(255), ArmorFxPath VARCHAR(255), MythicFxPath VARCHAR(255), VisorPath VARCHAR(255)" +
                ", HipAttachmentPath VARCHAR(255), WristAttachmentPath VARCHAR(255), FirstModifiedDateUtc VARCHAR(40), LastModifiedDateUtc VARCHAR(40), CreatedDateUtc VARCHAR(40),ThemePath VARCHAR(255),IsEquipped INTEGER," +
                "IsDefault INTEGER," +
                "CoreId VARCHAR(40) NOT NULL,  UNIQUE (ThemeName, CoreId),FOREIGN KEY (CoreId) REFERENCES CoresTable (CoreId)" +
                ");";

            string Createsql2 = "CREATE TABLE IF NOT EXISTS EmblemsOnThemes (id INTEGER PRIMARY KEY AUTOINCREMENT, EmblemPath VARCHAR(255), ConfigurationId INTEGER, IdTheme INTEGER," +
                "FOREIGN KEY (IdTheme) REFERENCES CustomArmorCoresOwnThemes (id)" +
                ");";
            string Createsql3 = @"
            CREATE TABLE IF NOT EXISTS ThemesImageTable
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ImageData BLOB,
                RelatedId INTEGER,
                FOREIGN KEY (RelatedId) REFERENCES CustomArmorCoresOwnThemes(id)
            )
            ";
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = Createsql1;
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = Createsql2;
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_cmd.CommandText = Createsql3;
            sqlite_cmd.ExecuteNonQueryAsync().Wait();

        }

        public long insertOnOwnThemes(string name, string CoreId, ArmorCoreTheme t)
        {
            string CreatedDateUtc = "";
            SQLiteCommand sqlite_cmd = Connection.CreateCommand();
            sqlite_cmd.CommandText = String.Format("INSERT INTO CustomArmorCoresOwnThemes ( ThemeName, CoatingPath,   GlovePath,   HelmetPath,   HelmetAttachmentPath,   ChestAttachmentPath,   KneePadPath,   LeftShoulderPadPath,   RightShoulderPadPath,   ArmorFxPath,   MythicFxPath,   VisorPath,   HipAttachmentPath,   WristAttachmentPath,   FirstModifiedDateUtc,   LastModifiedDateUtc,   CreatedDateUtc,   ThemePath,   IsEquipped,   IsDefault,   CoreId) " +
                                                        "VALUES(   '{0}',       '{1}',          '{2}',         '{3}',         '{4}',                   '{5}',                  '{6}',             '{7}',             '{8}',                  '{9}',        '{10}',        '{11}',           '{12}',               '{13}',             '{14}',               '{15}',              '{16}',        '{17}',       {18},         {19},     '{20}');",
                                                                    name, t.CoatingPath, t.GlovePath, t.HelmetPath, t.HelmetAttachmentPath, t.ChestAttachmentPath, t.KneePadPath, t.LeftShoulderPadPath, t.RightShoulderPadPath, t.ArmorFxPath, t.MythicFxPath, t.VisorPath, t.HipAttachmentPath, t.WristAttachmentPath, t.FirstModifiedDateUtc, t.LastModifiedDateUtc, CreatedDateUtc, t.ThemePath, t.IsEquipped, t.IsDefault, CoreId);
            sqlite_cmd.ExecuteNonQueryAsync().Wait();
            return Connection.LastInsertRowId;
        }

        public List<ArmorCoreThemeExtende> getOnOwnThemes(string CoreId)
        {
            var consulta = String.Format("SELECT * FROM CustomArmorCoresOwnThemes WHERE CoreId = '{0}'", CoreId);

            var resultados = Connection.Query<ArmorCoreThemeExtende>(consulta).ToList();
            return resultados;
        }
        public ArmorCoreThemeExtende getOnOwnTheme(string name, string CoreId)
        {
            var consulta = String.Format("SELECT * FROM CustomArmorCoresOwnThemes WHERE ThemeName = '{0}' AND CoreId = '{1}' ", name, CoreId);

            var resultados = Connection.Query<ArmorCoreThemeExtende>(consulta).ToList();
            if (resultados.Count > 0)
                return resultados[0];
            return null;

        }
        public void insertOnArmorCore(ArmorCore t, string name = "", byte[] imageBytes = null)
        {
            if (t == null)
                return;
            string FirstAcquiredDate = t?.FirstAcquiredDate?.ISO8601Date?.ToString();
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd = Connection.CreateCommand();
            sqlite_cmd.CommandText = String.Format("SELECT CoreId FROM CoresTable WHERE CoreId = '{0}';", t.CoreId);
            //sqlite_cmd.ExecuteNonQueryAsync().Wait();
            sqlite_datareader = sqlite_cmd.ExecuteReader();
            string valueId = "";
            while (sqlite_datareader.Read())
            {
                valueId = sqlite_datareader.GetString(0);
            }
            sqlite_datareader.Close();
            if (string.IsNullOrEmpty(valueId))
            {
                sqlite_cmd.CommandText = String.Format("INSERT INTO CoresTable (CoreId, CorePath, IsEquipped, CoreType, FirstAcquiredDate) VALUES ('{0}', '{1}', {2}, '{3}', '{4}');", t.CoreId, t.CorePath, (t.IsEquipped != null || t.IsEquipped == true ? 1 : 0), t.CoreType, FirstAcquiredDate);
                sqlite_cmd.ExecuteNonQueryAsync().Wait();
            }

            if (!string.IsNullOrEmpty(name) && t.Themes != null && t.Themes.Count > 0)
            {
                long theme_id = insertOnOwnThemes(name, t.CoreId, t.Themes[0]);
                SaveImage(imageBytes, theme_id);
            }
        }

        public bool DeleteCustomArmorCoresOwnThemes(int id)
        {
            try
            {
                string query = "DELETE FROM ThemesImageTable WHERE RelatedId = @id";
                SQLiteCommand command1 = new SQLiteCommand(query, Connection);
                command1.Parameters.AddWithValue("@id", id);
                command1.ExecuteNonQuery();

                query = "DELETE FROM CustomArmorCoresOwnThemes WHERE id = @id";
                SQLiteCommand command = new SQLiteCommand(query, Connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }

        }

        public void SaveImage(byte[] imageBytes, long relatedId)
        {
            using (var command = new SQLiteCommand(Connection))
            {
                command.CommandText = "INSERT INTO ThemesImageTable (ImageData, RelatedId) VALUES (@Image, @RelatedId)";
                command.Parameters.AddWithValue("@Image", imageBytes);
                command.Parameters.AddWithValue("@RelatedId", relatedId);
                command.ExecuteNonQueryAsync().Wait();
            }
        }

        public List<byte[]> GetImage(int relatedId)
        {
            var images = new List<byte[]>();
            using (var command = new SQLiteCommand(Connection))
            {
                command.CommandText = "SELECT ImageData FROM ThemesImageTable WHERE RelatedId = @RelatedId";
                command.Parameters.AddWithValue("@RelatedId", relatedId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var bytes = (byte[])reader["ImageData"];
                        images.Add(bytes);
                    }
                }
            }

            return images;
        }


    }
}