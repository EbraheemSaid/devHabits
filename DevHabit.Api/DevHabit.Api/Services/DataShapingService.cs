using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Services;

public sealed class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];


        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(pi => fieldsSet.Contains(pi.Name))
                .ToArray();
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }

        return (ExpandoObject)shapedObject;

    }

    public List<ExpandoObject> ShapeCollectionData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? LinksFactory = null)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];


        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(pi => fieldsSet.Contains(pi.Name))
                .ToArray();
        }

        List<ExpandoObject> ShapedObjects = [];

        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }
            if (LinksFactory != null)
            {
                shapedObject["Links"] = LinksFactory(entity);
            }

            ShapedObjects.Add((ExpandoObject)shapedObject);
        }

        return ShapedObjects;
    }
    public bool ValidateFields<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(
            typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (string field in fieldsSet)
        {
            if (!propertyInfos.Any(pi => pi.Name.Equals(field, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }
        return true;
    }
}
