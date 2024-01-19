using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.DbContexts.ConnectorDb
{
	[Index(nameof(OwnerNode), nameof(AccessorNode))]
	public class Connection
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public required string OwnerNode { get; set; }
		public required string DBConnectionString { get; set; }
		public required string AccessorNode { get; set; }

		public List<Resource> sharedResources { get; } = new();
	}
}

