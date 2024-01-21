namespace ConnectorAPI.DbContexts.ConnectorDb
{
	public class ResourceAttributes
	{
		public Guid Id { get; set; }

		public required string AttributeName { get; set; }
		public required string AttributeColumnName { get; set; }
		public short MinimumAccessLevel { get; set; }

		public required Guid ResourceId { get; set; }
		public virtual required Resource Resource { get; set; }
	}
}

