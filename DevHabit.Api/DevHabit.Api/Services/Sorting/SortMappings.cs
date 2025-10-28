namespace DevHabit.Api.Services.Sorting;

public sealed record SortMapping(string SortField, string EntityField, bool Reverse = false);
