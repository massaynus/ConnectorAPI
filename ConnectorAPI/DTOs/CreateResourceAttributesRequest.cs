using System;
namespace ConnectorAPI.DTOs
{
	public record CreateResourceAttributes
	{
		public required string AttributeName { get; set; }
		public required string AttributeColumnName { get; set; }
		public short MinimumAccessLevel { get; set; }

		public required Guid ResourceId { get; set; }
	}
}

