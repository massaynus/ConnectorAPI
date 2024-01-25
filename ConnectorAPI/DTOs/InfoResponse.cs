using System.Security.Claims;

namespace ConnectorAPI.DTOs;


public record ClaimDTO(string Type, string Value);

public record InfoResponse
{
    public required IList<ClaimDTO> Claims { get; init; }
}
