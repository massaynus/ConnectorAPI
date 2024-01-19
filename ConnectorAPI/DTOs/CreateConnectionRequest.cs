namespace ConnectorAPI.DTOs;

public record CreateConnectionRequest
{
    public required string OwnerNode { get; init; }
    public required string DBConnectionString { get; init; }
    public required string AccessorNode { get; init; }
}