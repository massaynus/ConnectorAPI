using System;
using ConnectorAPI.DbContexts.ConnectorDb;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.DbContexts
{
	public class ConnectorDbContext : DbContext
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

