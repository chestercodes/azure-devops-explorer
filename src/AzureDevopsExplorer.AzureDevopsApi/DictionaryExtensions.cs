using System.Text.Json;

namespace AzureDevopsExplorer.AzureDevopsApi;

public static class DictionaryExtensions
{
    public static Dictionary<string, string?> FlattenJsonObject(this Dictionary<string, object> @object, string prefix = "")
    {
        var dictionary = new Dictionary<string, string>();

        foreach (var (k, v) in @object)
        {
            if (v is JsonElement)
            {
                var element = (JsonElement)v;
                Flatten(dictionary, element, k);
            }
        }

        return dictionary;
    }

    private static void Flatten(IDictionary<string, string?> dictionary, JsonElement element, string name)
    {
        var key = name;
        if (element.ValueKind == JsonValueKind.Array)
        {
            var i = 0;
            foreach (var arrayItem in element.EnumerateArray())
            {
                var itemKey = $"{key}[{i}]";
                Flatten(dictionary, arrayItem, itemKey);
                i++;
            }
            return;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var objectProperty in element.EnumerateObject())
            {
                var itemKey = $"{key}.{objectProperty.Name}";
                Flatten(dictionary, objectProperty.Value, itemKey);
            }
            return;
        }

        if (element.ValueKind == JsonValueKind.Null)
        {
            dictionary[key] = null;
            return;
        }

        dictionary[key] = element.ToString();
    }
}
