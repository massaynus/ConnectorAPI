using ConnectorAPI.DbContexts;
using ConnectorAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ConnectorAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessorController : ControllerBase
    {
        private readonly ConnectorDbContext _db;

        public AccessorController(ConnectorDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<List<Dictionary<string, string>>>> GetResource([FromBody] AccessRequest accessRequest)
        {
            var connection = _db.Connections.SingleOrDefault(cn => cn.OwnerNode == accessRequest.OwnerNode && cn.AccessorNode == accessRequest.GuestNode);
            if (connection is null) return NotFound("Connection not found");

            var resource = _db.Resources
                .SingleOrDefault(r => r.ResourceId == accessRequest.ResourceId && r.Connection == connection && r.ResourceName == accessRequest.ResourceName);
            if (resource is null) return NotFound("Resource not found");

            var attributes = _db.Attributes
                .Where(attr => attr.Resource == resource && accessRequest.AccessLevel >= attr.MinimumAccessLevel)
                .ToList();
            if (attributes.Count == 0) return NotFound("No Attributes were permitted");

            var query = $"SELECT {string.Join(',', attributes.Select(a => a.AttributeColumnName))} FROM {resource.ResourceTableName} WHERE Id=@id";
            var connStr = connection.DBConnectionString;

            using SqlConnection sqlConnection = new(connStr);
            using SqlCommand sqlCommand = new(query, sqlConnection);
            sqlCommand.Parameters.AddWithValue("id", resource.ResourceId);

            await sqlConnection.OpenAsync();
            var reader = await sqlCommand.ExecuteReaderAsync();
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

