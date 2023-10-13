using System.Reflection;
using System.Text.Json;

namespace database.Helpers;

public static class Diff
{
    private static bool HasProperty(object objectToCheck, string propName)
    {
        var type = objectToCheck.GetType();
        return type.GetProperty(propName) != null;
    }

    private static object? GetPropValue(object src, string propName)
    {
        return src.GetType().GetProperty(propName)?.GetValue(src, null);
    }

    public static IEnumerable<KeyValuePair<string, object>> GetAllowedDiff(object previousObjectState, 
        IEnumerable<KeyValuePair<string, string>> newObjectState, string[] allowedProps)
    {
        var previousStateObjectType = previousObjectState.GetType();
        var result = new List<KeyValuePair<string, object>>();
        foreach (var (keyAnyCase, value) in newObjectState)
        {
            var key = keyAnyCase.FirstCharToUpper();
            if (!allowedProps.Contains(key))
                continue;
            var previousObjectField = 
                previousStateObjectType.GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
            if (previousObjectField == null)
                throw new InvalidOperationException("previousStateObject does not contain " + key);
            var valueAsTargetType = Convert.ChangeType(value, previousObjectField.PropertyType);
            var newObjectPropValueJson = JsonSerializer.Serialize(valueAsTargetType);
            var previousObjectPropValue = previousObjectField.GetValue(previousObjectState);
            var previousObjectPropValueJson = JsonSerializer.Serialize(previousObjectPropValue);

            if (previousObjectPropValueJson == newObjectPropValueJson) 
                continue;
            var keyValue = new KeyValuePair<string, object>(key, valueAsTargetType);
            result.Add(keyValue);
        }

        return result;
    }

    public static object ApplyAllowedDiff(object previousObjectState, 
        IEnumerable<KeyValuePair<string, string>> newObjectState, string[] allowedProps)
    {
        var previousStateObjectType = previousObjectState.GetType();
        foreach (var (keyAnyCase, value) in newObjectState)
        {
            var key = keyAnyCase.FirstCharToUpper();
            if (!allowedProps.Contains(key))
                continue;
            var newObjectPropValueJson = JsonSerializer.Serialize(value);
            var previousObjectField = 
                previousStateObjectType.GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
            if (previousObjectField == null)
                throw new InvalidOperationException("previousStateObject does not contain " + key);
            var previousObjectPropValue = previousObjectField.GetValue(previousObjectState);
            var previousObjectPropValueJson = JsonSerializer.Serialize(previousObjectPropValue);

            if (previousObjectPropValueJson == newObjectPropValueJson) 
                continue;

            // TODO: исправить на полноценный разбор типов с возможностью нуля
            if (previousObjectField.PropertyType == typeof(DateTime?))
                if (string.IsNullOrEmpty(value))
                    previousObjectField.SetValue(previousObjectState, null);
                else
                    previousObjectField.SetValue(previousObjectState,DateTime.UtcNow);
                    // TODO: исправить на полноценный разбор даты
                    // возможно подойдет DateTime.Parse("2010-08-20T15:00:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind)
            else if (previousObjectField.PropertyType == typeof(ulong?)) {
                if (string.IsNullOrEmpty(value))
                    previousObjectField.SetValue(previousObjectState, null);                
                else
                    previousObjectField.SetValue(previousObjectState, Convert.ChangeType(value, typeof(ulong)));
            } else
                previousObjectField.SetValue(previousObjectState,
                    Convert.ChangeType(value, previousObjectField.PropertyType));

        }

        return previousObjectState;
    }
    
    public static List<KeyValuePair<string, string>> GetDifferentProperties<T>(T original, T updated) where T : class
    {
        var differentProps = new List<KeyValuePair<string, string>>();
        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in props)
        {
            object originalProp = prop.GetValue(original);
            object updatedProp = prop.GetValue(updated);
            if ((originalProp != null && !originalProp.Equals(updatedProp)) 
                || (originalProp == null && originalProp != updatedProp))
                differentProps.Add(new KeyValuePair<string, string>(prop.Name, JsonSerializer.Serialize(updatedProp)));
        }

        return differentProps;
    }
    
    public static string PatchString(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        return string.Join(", ", keyValuePairs.Select(pair => $"{pair.Key}={pair.Value}"));
    }
    
}