using System.Collections.Concurrent;
using System.Data;
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

    public async Task<SqlDataReader?> ExecuteReader(string connectionString, string commandText, params SqlParameter[] sqlParameters)
    {
        while (IsMaxPoolSizeReached(connectionString))
        {
            _logger.LogWarning("MAX_POOL_SIZE reached");
            Thread.Sleep(1);
        }

        using var sqlConnection = CreateConnection(connectionString);
        using SqlCommand sqlCommand = new(commandText, sqlConnection);
        sqlCommand.Parameters.AddRange(sqlParameters);


        var reader = await sqlCommand.ExecuteReaderAsync();

        _logger.LogDebug(
            "Got reder for command: {0} and params: {1}\nconnection: {2}",
            commandText, string.Join('\t', sqlParameters.Select(p => $"{p.ParameterName}: {p.Value}")), connectionString);

        return reader;
    }

    private SqlConnection CreateConnection(string connectionString)
    {
        var connection = new SqlConnection(connectionString);
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