﻿using ConnectorAPI.DbContexts;
using ConnectorAPI.DTOs;
using ConnectorAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessorController : ControllerBase
    {
        private readonly ConnectorDbContext _db;
        private readonly ConnectionManagerService _connectionManager;
        private readonly ILogger<AccessorController> _logger;

        public AccessorController(ConnectorDbContext db, ConnectionManagerService connectionManager, ILogger<AccessorController> logger)
        {
            _db = db;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<List<Dictionary<string, string>>>> GetResource([FromBody] AccessRequest accessRequest)
        {
            var connection = _db.Connections
                .Include(c => c.Resources.Where(r => r.ResourceId == accessRequest.ResourceId && r.ResourceName == accessRequest.ResourceName))
                    .ThenInclude(r => r.Attributes.Where(at => accessRequest.AccessLevel >= at.MinimumAccessLevel))
                .FirstOrDefault(cn => cn.OwnerNode == accessRequest.OwnerNode && cn.AccessorNode == accessRequest.GuestNode);
            if (connection is null) return NotFound("Connection not found");

            var resource = connection.Resources.FirstOrDefault();
            if (resource is null) return NotFound("Resource not found");

            var attributes = resource.Attributes.Select(at => at.AttributeColumnName).ToArray();
            if (attributes.Length == 0) return NotFound("No Attributes were permitted");

            var query = $"SELECT {string.Join(',', attributes)} FROM {resource.ResourceTableName} WHERE Id=@id";
            var connStr = connection.DBConnectionString;
            var idParameter = new SqlParameter("id", resource.ResourceId);

            var reader = await _connectionManager.ExecuteReader(connStr, query, idParameter);
            if (reader is null) return NotFound();

            List<Dictionary<string, string>> resultSet = new();
            while (reader.Read())
            {
                var entry = new Dictionary<string, string>();
                var schema = reader.GetColumnSchema();
                for (int i = 0; i < schema.Count; i++)
                {
                    var column = schema[i];
                    entry.Add(column.ColumnName, reader.GetValue(i)?.ToString() ?? "null");
                }

                resultSet.Add(entry);

            }
            reader.Close();

            return Ok(resultSet);
        }
    }
}

