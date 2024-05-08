using System.Collections;

namespace AzureDevopsExplorer.Neo4j;

internal static class Extensions
{
    internal static bool IsValueTypeOrString(this Type type)
    {
        return type.IsValueType || type == typeof(string);
    }

    internal static string ToStringValueType(this object value)
    {
        return value switch
        {
            DateTime dateTime => dateTime.ToString("o"),
            bool boolean => boolean.ToStringLowerCase(),
            _ => value.ToString()
        };
    }

    internal static bool IsIEnumerable(this Type type)
    {
        return type.IsAssignableTo(typeof(IEnumerable));
    }

    internal static string ToStringLowerCase(this bool boolean)
    {
        return boolean ? "true" : "false";
    }
}

/// <summary>
/// Credit goes to https://josef.codes/transform-csharp-objects-to-a-flat-string-dictionary/
/// </summary>
public static class ObjectExtensions
{
    public static Dictionary<string, string> FlattenToDictionary(this object @object, string prefix = "")
    {
        var dictionary = new Dictionary<string, string>();
        Flatten(dictionary, @object, prefix);
        return dictionary;
    }

    private static void Flatten(
        IDictionary<string, string> dictionary,
        object source,
        string name)
    {
        var properties = source.GetType().GetProperties().Where(x => x.CanRead);
        foreach (var property in properties)
        {
            var key = string.IsNullOrWhiteSpace(name) ? property.Name : $"{name}.{property.Name}";
            var value = property.GetValue(source, null);

            if (value == null)
            {
                // don't include if null
                //dictionary[key] = null;
                continue;
            }

            if (property.PropertyType.IsValueTypeOrString())
            {
                dictionary[key] = value.ToStringValueType();
            }
            else
            {
                if (value is IEnumerable enumerable)
                {
                    var counter = 0;
                    foreach (var item in enumerable)
                    {
                        var itemKey = $"{key}[{counter++}]";
                        if (item.GetType().IsValueTypeOrString())
                        {
                            dictionary.Add(itemKey, item.ToStringValueType());
                        }
                        else
                        {
                            Flatten(dictionary, item, itemKey);
                        }
                    }
                }
                else
                {
                    Flatten(dictionary, value, key);
                }
            }
        }
    }
}