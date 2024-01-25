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

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Connection>()
				.HasOne(c => c.Owner)
				.WithMany(u => u.Connections);

			builder.Entity<Connection>()
				.HasOne(c => c.Guest)
				.WithMany(u => u.GuestConnections)
				.OnDelete(DeleteBehavior.NoAction);
		}
	}
}

