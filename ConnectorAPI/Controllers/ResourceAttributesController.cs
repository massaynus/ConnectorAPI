using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using AutoMapper;

namespace ConnectorAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResourceAttributesController : ControllerBase
    {
        private readonly ConnectorDbContext _context;
        private readonly IMapper _mapper;

        public ResourceAttributesController(ConnectorDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceAttributes>>> GetAttributes()
        {
            return await _context.Attributes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResourceAttributes>> GetResourceAttributes(Guid id)
        {
            var resourceAttributes = await _context.Attributes.FindAsync(id);

            if (resourceAttributes == null)
            {
                return NotFound();
            }

            return resourceAttributes;
        }

        [HttpPost]
        public async Task<ActionResult<ResourceAttributes>> PostResourceAttributes(CreateResourceAttributes createResourceAttribute)
        {
            var resource = _context.Resources.Find(createResourceAttribute.ResourceId);
            if (resource is null) return BadRequest(new { Message = "Can't create attribute for non existant Resource" });

            var resourceAttributes = _mapper.Map<ResourceAttributes>(createResourceAttribute);
            resourceAttributes.Resource = resource;

            _context.Attributes.Add(resourceAttributes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResourceAttributes", new { id = resourceAttributes.Id }, resourceAttributes);
        }


        [HttpPost("bulk")]
        public async Task<ActionResult<ResourceAttributes>> PostBulkResourceAttributes(List<CreateResourceAttributes> createResourceAttributes)
        {
            var resourceAttributes = new List<ResourceAttributes>();
            foreach (var createAttr in createResourceAttributes)
            {
                var res = _context.Resources.Find(createAttr.ResourceId);
                if (res is null) return BadRequest(new { Message = "Can't create attribute for non existant Resource" });

                var attr = _mapper.Map<ResourceAttributes>(createAttr);
                attr.Resource = res;

                resourceAttributes.Add(attr);
	        }


            _context.Attributes.AddRange(resourceAttributes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAttributes", resourceAttributes);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResourceAttributes(Guid id)
        {
            var resourceAttributes = await _context.Attributes.FindAsync(id);
            if (resourceAttributes == null)
            {
                return NotFound();
            }

            _context.Attributes.Remove(resourceAttributes);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
