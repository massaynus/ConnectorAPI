using System;
namespace ConnectorAPI.DbContexts.ConnectorDb
{
	public class ResourceAttributesAccessModel
	{
		public Guid Id { get; set; }

		public string AttributeName { get; set; }
		public string AttributeColumnName { get; set; }
		public short MinimumAccessLevel { get; set; }

		public ResourceModel Resource { get; set; }
	}
}

