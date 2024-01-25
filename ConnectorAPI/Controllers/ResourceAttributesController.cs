using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ConnectorAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResourceAttributesController : ControllerBase
    {
        private readonly ConnectorDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public ResourceAttributesController(ConnectorDbContext context, IMapper mapper, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceAttributes>>> GetAttributes()
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            return await _context.Attributes
                .Where(a => a.Owner.Id == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceAttributes>> GetResourceAttributes(Guid id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var resourceAttributes = await _context.Attributes.SingleOrDefaultAsync(a => a.Id == id && a.Owner.Id == userId);

            if (resourceAttributes == null)
            {
                return NotFound();
            }

            return resourceAttributes;
        }

        [HttpPost]
        public async Task<ActionResult<ResourceAttributes>> PostResourceAttributes(CreateResourceAttributes createResourceAttribute)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var resource = _context.Resources.Find(createResourceAttribute.ResourceId);

            if (user is null) return Unauthorized();
            if (resource is null) return BadRequest(new { Message = "Can't create attribute for non existant Resource" });

            var resourceAttributes = _mapper.Map<ResourceAttributes>(createResourceAttribute);
            resourceAttributes.Resource = resource;
            resourceAttributes.Owner = user;

            _context.Attributes.Add(resourceAttributes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResourceAttributes", new { id = resourceAttributes.Id }, resourceAttributes);
        }


        [HttpPost("bulk")]
        public async Task<ActionResult<ResourceAttributes>> PostBulkResourceAttributes(List<CreateResourceAttributes> createResourceAttributes)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null) return Unauthorized();

            var resourceIds = createResourceAttributes.Select(c => c.ResourceId).ToHashSet();
            var resources = _context.Resources.Where(r => resourceIds.Contains(r.Id)).ToDictionary(r => r.Id);

            var resourceAttributes = new List<ResourceAttributes>();
            foreach (var createAttr in createResourceAttributes)
            {
                var resExits = resources.TryGetValue(createAttr.ResourceId, out Resource? res);
                if (!resExits || res is null) return BadRequest(new { Message = "Can't create attribute for non existant Resource" });

                var attr = _mapper.Map<ResourceAttributes>(createAttr);
                attr.Resource = res;
                attr.Owner = user;

                resourceAttributes.Add(attr);
            }


            _context.Attributes.AddRange(resourceAttributes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAttributes", resourceAttributes);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResourceAttributes(Guid id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var resourceAttributes = await _context.Attributes.SingleOrDefaultAsync(a => a.Id == id && a.Owner.Id == userId);
            if (resourceAttributes == null)
                return NotFound();

            _context.Attributes.Remove(resourceAttributes);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
