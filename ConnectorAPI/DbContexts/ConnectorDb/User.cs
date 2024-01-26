using Microsoft.AspNetCore.Identity;

namespace ConnectorAPI.DbContexts.ConnectorDb;

public class User : IdentityUser
{
    public string? RSAPublicKey { get; set; }

    public List<Connection> GuestConnections { get; set; } = new();
    public List<Connection> Connections { get; set; } = new();
    public List<Resource> Resources { get; set; } = new();
    public List<ResourceAttributes> Attributes { get; set; } = new();
}