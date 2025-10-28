using System.Linq.Dynamic.Core;
namespace DevHabit.Api.Services.Sorting;

public sealed class SortMappingsProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetSortMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? mappingDefinition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (mappingDefinition == null)
        {
            throw new InvalidOperationException(
                $"No sort mapping definition found for source type '{typeof(TSource)}' and destination type '{typeof(TDestination)}'.");
        }
        return mappingDefinition.Mappings;
    }



    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }
        var sortFields = sort
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(sortPart =>
            {
                var parts = sortPart.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return parts[0];
            })
            .ToList();

        SortMapping[] mappings = GetSortMappings<TSource, TDestination>();

        return sortFields.All(field =>
            mappings.Any(m => m.SortField.Equals(field, StringComparison.OrdinalIgnoreCase)));
    }
}
