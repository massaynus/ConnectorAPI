using System.Collections.Concurrent;
using System.Data;
using ConnectorAPI.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace ConnectorAPI.Services;

public class ConnectionManagerService
{
    private readonly ILogger<ConnectionManagerService> _logger;
    private readonly int MAX_POOL_SIZE;
    private readonly ConcurrentDictionary<string, HashSet<SqlConnection>> _connectionsPool;
    private readonly IMemoryCache _memoryCache;


    public ConnectionManagerService(ILogger<ConnectionManagerService> logger, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _logger = logger;
        _connectionsPool = new();
        MAX_POOL_SIZE = int.Parse(configuration["ConnectionPoolSize"] ?? "20");

        _logger.LogInformation("Created with a MAX_POOL_SIZE of {0}", MAX_POOL_SIZE);
        _memoryCache = memoryCache;
    }

    public async Task<List<Dictionary<string, string>>> ExecuteReader(string connectionString, string commandText, params SqlParameter[] sqlParameters)
    {
        var key = GenerateCacheKey(connectionString, commandText, sqlParameters);
        if (_memoryCache.TryGetValue(key, out List<Dictionary<string, string>>? result) && result is not null) return result;

        while (IsMaxPoolSizeReached(connectionString))
        {
            _logger.LogWarning("MAX_POOL_SIZE reached");
            Thread.Sleep(1);
        }

        using SqlConnection sqlConnection = CreateConnection(connectionString);
        using SqlCommand sqlCommand = new(commandText, sqlConnection);
        sqlCommand.Parameters.AddRange(sqlParameters);

        var reader = await sqlCommand.ExecuteReaderAsync();

        _logger.LogDebug(
            "Got reder for command: {0} and params: {1}\nconnection: {2}",
            commandText, string.Join('\t', sqlParameters.Select(p => $"{p.ParameterName}: {p.Value}")), connectionString);

        var data = reader.ToRecords();

        _memoryCache.Set(key, data, TimeSpan.FromMinutes(1));

        return data;
    }

    private string GenerateCacheKey(string connectionString, string commandText, params SqlParameter[] sqlParameters)
    {
        string prs = string.Join(',', sqlParameters.Select(p => $"{p.ParameterName}:{p.Value}"));

        return $"{connectionString}#{commandText}#{prs}";
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