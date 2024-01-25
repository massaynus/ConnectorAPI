namespace ConnectorAPI.DTOs
{
	public record AccessRequest
	{
		public required string OwnerId { get; init; }

		public required string ResourceId { get; init; }
		public required string ResourceName { get; init; }

		public required short AccessLevel { get; init; }
	}
}

