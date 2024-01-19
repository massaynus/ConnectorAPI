namespace ConnectorAPI.DTOs
{
	public record AccessRequest
	{
		public required string GuestNode { get; init; }
		public required string OwnerNode { get; init; }

		public Guid? ResourceId { get; init; }
		public required string ResourceName { get; init; }

	}
}

