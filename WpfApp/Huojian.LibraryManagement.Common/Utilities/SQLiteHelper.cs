using System.Data.SQLite;

namespace ShadowBot.Common.Utilities
{
    public class SQLiteHelper
    {
        public static SQLiteConnection CreateConnectionFromFile(string filename)
        {
            return new SQLiteConnection($"Data Source={filename};");
        }
    }
}
