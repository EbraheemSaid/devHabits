using System.Linq.Dynamic.Core;
namespace DevHabit.Api.Services.Sorting;

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        SortMapping[] mappings,
        string defaultSortField = "Id"
        )
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderBy($"{defaultSortField} ASC");
        }
        var sortInstructions = sortBy
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(sortPart =>
            {
                var parts = sortPart.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var field = parts[0];
                var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
                return (field, descending);
            })
            .ToList();
        var orderByClauses = new List<string>();
        foreach (var (field, descending) in sortInstructions)
        {
            var mapping = mappings.FirstOrDefault(m => m.SortField.Equals(field, StringComparison.OrdinalIgnoreCase));
            if (mapping != null)
            {
                var finalDescending = descending ^ mapping.Reverse;
                orderByClauses.Add($"{mapping.EntityField} {(finalDescending ? "DESC" : "ASC")}");
            }
        }
        if (orderByClauses.Count == 0)
        {
            orderByClauses.Add($"{defaultSortField} ASC");
        }
        var orderByString = string.Join(", ", orderByClauses);
        return query.OrderBy(orderByString);
    }
}
