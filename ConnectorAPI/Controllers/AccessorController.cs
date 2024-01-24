﻿using ConnectorAPI.DbContexts;
using ConnectorAPI.DTOs;
using ConnectorAPI.Repositories;
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
        private readonly AccessRepository _accessRepository;

        public AccessorController(ConnectorDbContext db, ConnectionManagerService connectionManager, AccessRepository accessRepository)
        {
            _db = db;
            _connectionManager = connectionManager;
            _accessRepository = accessRepository;
        }

        [HttpPost]
        public async Task<ActionResult<List<Dictionary<string, string>>>> GetResource([FromBody] AccessRequest accessRequest)
        {
            var connection = _accessRepository.GetConnection(accessRequest);
            if (connection is null) return NotFound("Connection not found");

            var resource = connection.Resources.FirstOrDefault();
            if (resource is null) return NotFound("Resource not found");

            var attributes = resource.Attributes.Select(at => at.AttributeColumnName).ToArray();
            if (attributes.Length == 0) return NotFound("No Attributes were permitted");

            var atParams = attributes.Select(a => $"[{a}]");
            var query = $"SELECT {string.Join(',', atParams)} FROM [{resource.ResourceTableName}] WHERE Id=@rowId";
            var connStr = connection.DBConnectionString;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("rowId", resource.ResourceId),
            };

            var data = await _connectionManager.ExecuteReader(connStr, query, parameters.ToArray());
            return Ok(data);
        }
    }
}

