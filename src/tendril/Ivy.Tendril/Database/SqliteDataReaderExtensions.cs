using Microsoft.Data.Sqlite;

namespace Ivy.Tendril.Database;

public static class SqliteDataReaderExtensions
{
    public static string? GetStringOrNull(this SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
