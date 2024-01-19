using AutoMapper;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;

namespace ConnectorAPI
{
	public class Mappers : Profile
	{
		public Mappers()
		{
			CreateMap<CreateConnectionRequest, Connection>();
			CreateMap<CreateResourceRequest, Resource>();
			CreateMap<CreateResourceAttributes, ResourceAttributes>();
		}
	}
}

