using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConnectorAPI.DbContexts.ConnectorDb
{
	[Index(nameof(OwnerId))]
	[Index(nameof(GuestId))]
	public class Connection
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public required string OwnerId { get; set; }
		public required string DBConnectionString { get; set; }
		public required string GuestId { get; set; }

		public required User Owner { get; set; }
		public required User Guest { get; set; }

		public List<Resource> Resources { get; set; } = new();
	}
}

