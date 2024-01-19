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
            if (connection is null) return NotFound();

            var resource = connection.sharedResources.SingleOrDefault(r => r.ResourceName == accessRequest.ResourceName);
            if (resource is null) return NotFound();

            var attributes = resource.Attributes.ToList();
            if (attributes.Count == 0) return NotFound();

            var query = $"SELECT {string.Join(',', attributes.Select(a => a.AttributeColumnName))} FROM {resource.ResourceTableName}";
            var connStr = connection.DBConnectionString;

            using SqlConnection sqlConnection = new(connStr);
            using SqlCommand sqlCommand = new(query, sqlConnection);

            await sqlConnection.OpenAsync();
            var reader = await sqlCommand.ExecuteReaderAsync();
            if (reader is null) return NotFound();

            List<Dictionary<string, string>> resultSet = new();
            do {
                var entry = new Dictionary<string, string>();
                var schema = reader.GetColumnSchema();
                for(int i = 0; i < schema.Count; i++)
                {
                    var column = schema[i];
                    entry.Add(column.ColumnName, reader.GetValue(i)?.ToString() ?? "null");
		        }

                resultSet.Add(entry);

		     } while (reader.NextResult());

            return Ok(resultSet);
	    }
    }
}

