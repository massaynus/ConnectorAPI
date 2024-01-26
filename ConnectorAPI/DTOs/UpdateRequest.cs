namespace ConnectorAPI;

public record class UpdateRequest
{
    public required string OwnerId { get; init; }

    public required string ResourceId { get; init; }
    public required string ResourceName { get; init; }

    public required short AccessLevel { get; init; }


    public required IList<AttributeUpdate> Updates { get; init; }

}

public record class AttributeUpdate
{
    public required string AttributeName { get; init; }
    public required string NewValue { get; init; }
}
