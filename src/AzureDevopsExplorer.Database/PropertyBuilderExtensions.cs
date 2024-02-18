using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureDevopsExplorer.Database;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<T> AddEnumMaxLengthAndConversion<T>(this PropertyBuilder<T> builder)
    {
        var t = typeof(T);
        if (typeof(T).IsGenericType)
        {
            t = t.GenericTypeArguments[0];
        }
        var vals = Enum.GetValues(t);
        var longest = 0;
        foreach (var item in vals)
        {
            if (item != null)
            {
                var l = item.ToString().Length;
                if (l > longest)
                {
                    longest = l;
                }
            }
        }
        builder.HasMaxLength(longest);
        builder.HasConversion<string>();
        return builder;
    }
}
