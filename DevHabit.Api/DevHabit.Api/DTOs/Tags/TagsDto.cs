namespace DevHabit.Api.DTOs.Tags;

public sealed record TagsCollectionDto
{
    public List<TagDto> Data { get; init; }
}
public sealed class TagDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

}
