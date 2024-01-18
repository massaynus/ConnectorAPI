using System;
namespace ConnectorAPI.DbContexts.ConnectorDb
{
	public class Connection
	{
		public Guid Id { get; set; }

		public string OwnerNode { get; set; }
        public string DBConnectionString { get; set; }
		public string AccessorNode { get; set; }

		public List<ResourceModel> sharedResources { get; } = new();
	}
}

