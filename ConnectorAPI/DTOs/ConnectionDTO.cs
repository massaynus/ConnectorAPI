namespace ConnectorAPI.DTOs;

public record class ConnectionDTO
{
    public Guid Id { get; set; }

    public required string OwnerId { get; set; }
    public required string GuestId { get; set; }
}
