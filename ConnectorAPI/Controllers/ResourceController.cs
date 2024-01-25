using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ConnectorAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ConnectorDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public ResourceController(ConnectorDbContext context, IMapper mapper, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            return await _context.Resources
                .Where(r => r.Owner.Id == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("{id}", Name = nameof(GetResource))]
        public async Task<ActionResult<Resource>> GetResource(Guid id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var resource = await _context.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id && r.Owner.Id == userId);

            if (resource == null)
                return NotFound();

            return resource;
        }

        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(CreateResourceRequest createResource)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = _userManager.GetUserId(HttpContext.User);

            if (user is null) return Unauthorized();

            var connection = _context.Connections.Where(c => c.OwnerId == userId && c.Id == createResource.ConnectionId).SingleOrDefault();
            if (connection is null) return BadRequest(new { Message = "Can't create resource for non existing connection" });

            var resource = _mapper.Map<Resource>(createResource);
            resource.Owner = user;
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return Ok(resource);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var resource = await _context.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id && r.Owner.Id == userId);

            if (resource == null)
                return NotFound();

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
