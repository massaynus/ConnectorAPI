using System.Text.RegularExpressions;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using ConnectorAPI.Repositories;
using ConnectorAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ConnectorAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessorController : ControllerBase
    {
        private readonly ConnectionManagerService _connectionManager;
        private readonly AccessRepository _accessRepository;
        private readonly UserManager<User> _userManager;

        public AccessorController(ConnectionManagerService connectionManager,
                                  AccessRepository accessRepository,
                                  UserManager<User> userManager)
        {
            _connectionManager = connectionManager;
            _accessRepository = accessRepository;
            _userManager = userManager;
        }

        [HttpPost(nameof(GetResource))]
        public async Task<ActionResult<List<Dictionary<string, string>>>> GetResource([FromBody] AccessRequest request)
        {
            var guestId = _userManager.GetUserId(HttpContext.User);
            if (guestId is null) return Unauthorized();

            var connection = _accessRepository.GetConnection(guestId, request);
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


        [HttpPost(nameof(UpdateResource))]
        public async Task<ActionResult<List<Dictionary<string, string>>>> UpdateResource([FromBody] UpdateRequest request)
        {
            var guestId = _userManager.GetUserId(HttpContext.User);
            if (guestId is null) return Unauthorized();

            var connection = _accessRepository.GetConnection(guestId, request);
            if (connection is null) return NotFound("Connection not found");

            var resource = connection.Resources.FirstOrDefault();
            if (resource is null) return NotFound("Resource not found");

            var updateAttributesValues = request.Updates.ToDictionary(a => a.AttributeName);
            var attributes = resource.Attributes
                .Where(a => updateAttributesValues.ContainsKey(a.AttributeName))
                .ToArray();

            if (attributes.Length == 0) return NotFound("No Attributes of the requested ones were permitted");

            var updateSequence = string.Join(", ", attributes.Select(a => $"[{a.AttributeColumnName}] = @{a.AttributeParameterName}"));
            var query = $"UPDATE [{resource.ResourceTableName}] SET {updateSequence} WHERE Id=@rowId";
            var connStr = connection.DBConnectionString;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("rowId", resource.ResourceId),
            };

            parameters.AddRange(
                attributes.Select(a => new SqlParameter($"{a.AttributeParameterName}", updateAttributesValues[a.AttributeName].NewValue))
            );

            var data = await _connectionManager.ExecuteReader(connStr, query, parameters.ToArray());
            return Ok(data);
        }
    }
}

