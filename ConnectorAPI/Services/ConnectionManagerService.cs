using System.Data;
using Microsoft.Data.SqlClient;

namespace ConnectorAPI.Services;

public class ConnectionManagerService
{
    private readonly ILogger<ConnectionManagerService> _logger;
    private readonly Dictionary<string, HashSet<ConnectionContainer>> connections;
    private readonly int MAX_POOL_SIZE;


    public ConnectionManagerService(ILogger<ConnectionManagerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        connections = new();
        MAX_POOL_SIZE = int.Parse(configuration["ConnectionPoolSize"] ?? "20");

        _logger.LogInformation("Created with a MAX_POOL_SIZE of {0}", MAX_POOL_SIZE);
    }

    public async Task<ConnectionContainer> Add(string connectionString)
    {
        SqlConnection conn = new(connectionString);
        await conn.OpenAsync();
        _logger.LogDebug("Opened a new connection to {0}", connectionString);

        var container = new ConnectionContainer
        {
            ConnectionString = connectionString,
            Connection = conn,
        };

        if (connections.ContainsKey(connectionString))
            connections[connectionString].Add(container);
        else connections.Add(connectionString, new HashSet<ConnectionContainer>() { container });

        conn.InfoMessage += (object _, SqlInfoMessageEventArgs e) =>
        {
            _logger.LogError("Client side error ganarated:\nMessage: {0}", e.Message);
        };

        conn.StateChange += async (_, e) =>
        {
            if (e.CurrentState == ConnectionState.Closed || e.CurrentState == ConnectionState.Broken)
            {
                lock (container)
                {
                    connections[connectionString].Remove(container);
                }

                await Add(connectionString);
                conn.Dispose();
            }
        };

        return container;

    }

    public async Task<ConnectionContainer> Get(string connectionString)
    {
        var container = connections[connectionString];
        var connection = container.FirstOrDefault(c => !c.IsOccupied);

        if (connection is not null) return connection;

        if (container.Count < MAX_POOL_SIZE)
        {
            _logger.LogDebug(
                "No available connections, creating a new one: \t MAX_POOL_SIZE: {0}, CONNECTIONS: {1}, ConnSTR: {2}",
                MAX_POOL_SIZE, container.Count, connectionString);
            return await Add(connectionString);
        }
        else
        {
            _logger.LogDebug(
                "No available connections, waiting for one to be freed: \t MAX_POOL_SIZE: {0}, CONNECTIONS: {1}, ConnSTR: {2}",
                MAX_POOL_SIZE, container.Count, connectionString);
            Thread.Sleep(5);
            return await Get(connectionString);
        }

    }

    public Task<ConnectionContainer> GetOrAdd(string connectionString)
    {
        if (connections.ContainsKey(connectionString))
            return Get(connectionString);
        else
            return Add(connectionString);

    }

    public async Task<SqlDataReader?> ExecuteReader(string connStr, string commandText, params SqlParameter[] sqlParameters)
    {
        var container = await GetOrAdd(connStr);
        container.Occupie();

        var sqlConnection = container.Connection;
        using SqlCommand sqlCommand = new(commandText, sqlConnection);
        sqlCommand.Parameters.AddRange(sqlParameters);


        var reader = await sqlCommand.ExecuteReaderAsync();

        _logger.LogDebug(
            "Got reder for command: {0} and params: {1}\nconnection: {2}",
            commandText, string.Join('\t', sqlParameters.Select(p => $"{p.ParameterName}: {p.Value}")), connStr);

        container.Free();
        return reader;
    }


    public sealed class ConnectionContainer : IDisposable
    {
        public required string ConnectionString { get; init; }
        public required SqlConnection Connection { get; init; }
        public bool IsOccupied { get; private set; } = false;
        public DateTime CreatedAt { get; } = DateTime.Now;

        public void Occupie()
        {
            IsOccupied = true;
        }

        public void Free()
        {
            IsOccupied = false;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}