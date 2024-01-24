using ConnectorAPI.DbContexts.ConnectorDb;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.DbContexts
{
	public class ConnectorDbContext : IdentityDbContext<User>
	{
		public DbSet<Connection> Connections { get; set; }
		public DbSet<Resource> Resources { get; set; }
		public DbSet<ResourceAttributes> Attributes { get; set; }

		public ConnectorDbContext(DbContextOptions<ConnectorDbContext> options)
			: base(options)
		{

		}
	}
}

