using Microsoft.Data.SqlClient;

namespace ConnectorAPI.Extensions;

public static class SqlDataReaderExtensions
{
    public static List<Dictionary<string, string>> ToRecords(this SqlDataReader reader)
    {
        List<Dictionary<string, string>> resultSet = new();
        while (reader.Read())
        {
            var entry = new Dictionary<string, string>();
            var schema = reader.GetColumnSchema();
            foreach (var column in schema)
                entry.Add(column.ColumnName, reader[column.ColumnName]?.ToString() ?? "NULL");

            resultSet.Add(entry);

        }
        reader.Close();

        return resultSet;
    }
}