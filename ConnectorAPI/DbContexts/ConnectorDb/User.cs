using Microsoft.AspNetCore.Identity;

namespace ConnectorAPI.DbContexts.ConnectorDb;

public class User : IdentityUser {

    public List<Connection> Connections {get; set;} = new();
}