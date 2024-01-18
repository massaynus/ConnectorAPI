using System;
namespace ConnectorAPI.DbContexts.ConnectorDb
{
	public class Resource
	{
		public Guid Id { get; set; }

		public string ResourceName { get; set; }
		public string ResourceTableName { get; set; }

		public Connection connection { get; set; }
		public List<ResourceAttributes> Attributes { get; } = new();
	}
}

