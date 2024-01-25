using AutoMapper;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectionConroller : ControllerBase
{
    private readonly ILogger<ConnectionConroller> _logger;
    private readonly ConnectorDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public ConnectionConroller(
    ILogger<ConnectionConroller> logger,
    ConnectorDbContext db,
    IMapper mapper,
    UserManager<User> userManager)
    {
        _logger = logger;
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpGet(Name = "GetAllConnections")]
    public ActionResult<IEnumerable<Connection>> Get()
    {
        var userId = _userManager.GetUserId(HttpContext.User);

        return _db.Connections.Where(c => c.User.Id == userId).AsNoTracking().ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Connection>> GetConnection(Guid id)
    {
        var userId = _userManager.GetUserId(HttpContext.User);

        var connection = await _db.Connections
            .Where(c => c.User.Id == userId && c.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (connection == null)
        {
            return NotFound();
        }

        return Ok(connection);
    }

    [HttpPost(Name = "CreateConnection")]
    public async Task<ActionResult<Connection>> Create([FromBody] CreateConnectionRequest connectionRequest)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);

        var connection = _mapper.Map<Connection>(connectionRequest);
        connection.User = user!;

        _db.Connections.Add(connection);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetConnection), new { id = connection.Id }, connection);
    }

    [HttpDelete("{id}", Name = "DeleteConnection")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userManager.GetUserId(HttpContext.User);

        var connection = await _db.Connections
            .Include(c => c.User)
            .AsSingleQuery()
            .SingleOrDefaultAsync(cn => cn.Id == id);

        if (connection is null) return NotFound();
        else if (connection.User.Id != userId) return Forbid();
        else
        {
            _db.Connections.Remove(connection);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}

