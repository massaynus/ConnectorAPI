using System.Collections.Concurrent;
using System.Data;
using ConnectorAPI.Extensions;
using Microsoft.Data.SqlClient;

namespace ConnectorAPI.Services;

public class ConnectionManagerService
{
    private readonly ILogger<ConnectionManagerService> _logger;
    private readonly int MAX_POOL_SIZE;
    private readonly ConcurrentDictionary<string, HashSet<SqlConnection>> _connectionsPool;


    public ConnectionManagerService(ILogger<ConnectionManagerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionsPool = new();
        MAX_POOL_SIZE = int.Parse(configuration["ConnectionPoolSize"] ?? "20");

        _logger.LogInformation("Created with a MAX_POOL_SIZE of {0}", MAX_POOL_SIZE);
    }

    public async Task<List<Dictionary<string, string>>> ExecuteReader(string connectionString, string commandText, params SqlParameter[] sqlParameters)
    {
        while (IsMaxPoolSizeReached(connectionString))
        {
            _logger.LogWarning("MAX_POOL_SIZE reached");
            Thread.Sleep(1);
        }

        using SqlConnection sqlConnection = CreateConnection(connectionString);
        using SqlCommand sqlCommand = new(commandText, sqlConnection);
        sqlCommand.Parameters.AddRange(sqlParameters);

        _logger.LogInformation(
            "Executing Query: {0}",
            commandText
        );

        var reader = await sqlCommand.ExecuteReaderAsync();

        _logger.LogInformation(
            "\nGot reader for command: {0} and params: {1}",
            commandText, string.Join("\n\t", sqlParameters.Select(p => $"{p.ParameterName}: {p.Value}")));

        return reader.ToRecords();
    }

    private SqlConnection CreateConnection(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();

        connection.Disposed += (_, __) => { RemoveConnection(connectionString, connection); };
        AddConnection(connectionString, connection);

        return connection;
    }

    private void AddConnection(string connectionString, SqlConnection connection)
    {
        _connectionsPool.AddOrUpdate(
            connectionString,
            (key) => new() { connection },
            (key, set) =>
            {
                set.Add(connection);
                return set;
            }
        );
    }

    private void RemoveConnection(string connectionString, SqlConnection connection)
    {
        _connectionsPool[connectionString].Remove(connection);
    }

    private bool IsMaxPoolSizeReached(string connectionString)
    {
        return _connectionsPool.ContainsKey(connectionString) && _connectionsPool[connectionString].Count >= MAX_POOL_SIZE;
    }
}