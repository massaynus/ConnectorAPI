using AutoMapper;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ConnectorAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionConroller : ControllerBase
{
    private readonly ILogger<ConnectionConroller> _logger;
    private readonly ConnectorDbContext _db;
    private readonly IMapper _mapper;

    public ConnectionConroller(
    ILogger<ConnectionConroller> logger,
    ConnectorDbContext db
,
    IMapper mapper)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
    }

    [HttpGet(Name = "GetAllConnections")]
    public IEnumerable<Connection> Get()
    {
        return _db.Connections.ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Resource>> GetConnection(Guid id)
    {
        var connection = await _db.Connections.FindAsync(id);

        if (connection == null)
        {
            return NotFound();
        }

        return Ok(connection);
    }

    [HttpPost(Name = "CreateConnection")]
    public ActionResult<Connection> Create([FromBody] CreateConnectionRequest connectionRequest)
    {
        var connection = _mapper.Map<Connection>(connectionRequest);
        _db.Connections.Add(connection);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetConnection), new { id = connection.Id }, connection);
    }

    [HttpDelete("{id}", Name = "DeleteConnection")]
    public IActionResult Delete(Guid id)
    {
        var connection = _db.Connections.SingleOrDefault(cn => cn.Id == id);
        if (connection is null) return NotFound();
        else
        {
            _db.Connections.Remove(connection);
            _db.SaveChanges();
            return Ok();
        }
    }
}

