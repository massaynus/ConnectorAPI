namespace ConnectorAPI.DTOs;

public record CreateConnectionRequest
{
    public required string DBConnectionString { get; init; }
    public required string GuestUserId { get; init; }
}