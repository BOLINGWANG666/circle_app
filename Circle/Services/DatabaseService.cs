using SQLite;
using Circle.Models;
using System.IO;

namespace Circle.Services;

internal static class DatabaseService
{
    private static string? _databaseFile;

    private static string DatabaseFile
    {
        get
        {
            if (_databaseFile == null)
            {
                string databaseDir = Path.Combine(FileSystem.Current.AppDataDirectory, "data");
                Directory.CreateDirectory(databaseDir);
                _databaseFile = Path.Combine(databaseDir, "character_data.sqlite");
            }
            return _databaseFile;
        }
    }

    private static SQLiteConnection? _connection;

    public static SQLiteConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(DatabaseFile);
                _connection.CreateTable<PlayerSaveData>();
            }
            return _connection;
        }
    }
}