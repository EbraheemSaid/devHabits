using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagQueries
{
    public static System.Linq.Expressions.Expression<Func<Tag, TagDto>> TagToDtoProjection()
    {
        return t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            CreatedAtUtc = t.CreatedAtUtc,
            UpdatedAtUtc = t.UpdatedAtUtc
        };
    }
}
