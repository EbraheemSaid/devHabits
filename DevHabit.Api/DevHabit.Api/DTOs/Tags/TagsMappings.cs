using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagsMappings
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc

        };
    }
    public static Tag ToEntity(this CreateTagDto createTagDto)
    {
        var tag = new Tag
        {
            Id = $"t_{Guid.CreateVersion7()}",
            Name = createTagDto.Name,
            Description = createTagDto.Description,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        return tag;
    }
    public static TagDto UpdateFromDto(this Tag tag, UpdateTagDto updateTagDto)
    {
        tag.Name = updateTagDto.Name;
        tag.Description = updateTagDto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
        return tag.ToDto();
    }
}
