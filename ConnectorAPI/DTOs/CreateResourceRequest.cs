using System;
namespace ConnectorAPI.DTOs
{
	public record CreateResourceRequest
	{
		public required string ResourceId { get; init; }
		public required string ResourceName { get; init; }
		public required string ResourceTableName { get; init; }

		public required Guid ConnectionId { get; init; }
	}
}

