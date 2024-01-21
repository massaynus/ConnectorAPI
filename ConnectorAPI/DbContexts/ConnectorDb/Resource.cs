using System;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.DbContexts.ConnectorDb
{
	[Index(nameof(ResourceId))]
	[Index(nameof(ResourceName))]
	public class Resource
	{
		public Guid Id { get; set; }

		public required string ResourceId { get; set; }
		public required string ResourceName { get; set; }
		public required string ResourceTableName { get; set; }

		public required Guid ConnectionId { get; set; }
		public virtual required Connection Connection { get; set; }
		public virtual List<ResourceAttributes> Attributes { get; } = new();
	}
}

