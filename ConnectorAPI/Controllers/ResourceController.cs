using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using AutoMapper;

namespace ConnectorAPI.Controllers
{
    [ApiController]
	[Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ConnectorDbContext _context;
        private readonly IMapper _mapper;

        public ResourceController(ConnectorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
            return await _context.Resources.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Resource>> GetResource(Guid id)
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
            {
                return NotFound();
            }

            return resource;
        }

        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(CreateResourceRequest createResource)
        {
            var connection = _context.Connections.Find(createResource.ConnectionId);
            if (connection is null) return BadRequest(new { Message = "Can't create resource for non existing connection" });

            var resource = _mapper.Map<Resource>(createResource);
            resource.connection = connection;
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResource", new { id = resource.Id }, resource);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
