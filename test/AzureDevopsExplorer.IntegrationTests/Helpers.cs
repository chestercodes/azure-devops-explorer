using System.Reflection;

namespace AzureDevopsExplorer.IntegrationTests;

public class Helpers
{
    /// <summary>
    /// This is far from perfect, but people who live in glass houses...
    /// </summary>
    private static void DuplicatePublicClassesWithPropertiesAndEnums(Type[] baseTypes, string newNamespace, List<string> namespacesToReplace)
    {
        // Create a new file to write the duplicated classes and enums
        string fileName = $"{newNamespace}.txt";
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            writer.WriteLine($"using System.Collections.Generic;");
            writer.WriteLine($"");

            // Write the new namespace
            writer.WriteLine($"namespace {newNamespace}");
            writer.WriteLine("{");

            foreach (var type in baseTypes)
            {
                // Get all public types and enums in the DLL
                Type[] types = type.Assembly.GetExportedTypes();

                // Loop through each type and write it to the new file
                foreach (Type t in types)
                {
                    // Only write public classes with properties and enums
                    if (t.IsClass && t.IsPublic /* && !t.IsAbstract */ && t.GetProperties().Length > 0)
                    {
                        writer.WriteLine($"\t// {t.FullName}");
                        var className = t.Name
                            .Replace("`1", "")
                            .Replace("`2", "")
                            ;
                        if (t.IsAbstract)
                        {
                            className = "I" + className;
                        }
                        writer.WriteLine($"\tpublic class {className}");
                        writer.WriteLine("\t{");

                        // Loop through each property and write it to the new file
                        foreach (PropertyInfo prop in t.GetProperties())
                        {
                            string typeName = GetPropertyName(prop.PropertyType, namespacesToReplace);

                            writer.WriteLine($"\t\tpublic {typeName} {prop.Name} {{ get; set; }}");
                        }

                        writer.WriteLine("\t}");
                    }
                    else if (t.IsEnum && t.IsPublic)
                    {
                        writer.WriteLine($"\tpublic enum {t.Name}");
                        writer.WriteLine("\t{");

                        // Loop through each enum value and write it to the new file
                        foreach (string name in Enum.GetNames(t))
                        {
                            writer.WriteLine($"\t\t{name},");
                        }

                        writer.WriteLine("\t}");
                    }
                }

            }

            writer.WriteLine("}");
        }
    }

    public static string GetPropertyName(Type t, List<string> namespacesToReplace)
    {
        if (t.FullName == null)
        {
            return "TODO";
        }

        if (t.IsGenericType)
        {
            string typeName = t.FullName
                .Substring(0, t.FullName.IndexOf('`'))
                .Replace("System.Collections.Generic.", "");
            string genericArguments = string.Join(", ", t.GetGenericArguments().Select(x => GetPropertyName(x, namespacesToReplace)));
            return $"{typeName}<{genericArguments}>";
        }

        if (t.IsArray)
        {
            return $"{GetPropertyName(t.GetElementType(), namespacesToReplace)}[]";
        }

        string newName = t.FullName
            .Replace("System.String", "string")
            .Replace("System.Int32", "int")
            .Replace("System.Int64", "long")
            .Replace("System.Boolean", "bool")
            .Replace("System.Collections.Generic.", "")
            .Replace("System.Guid", "Guid")
            .Replace("System.DateTime", "DateTime")
            .Replace("System.Nullable", "Nullable")
            ;

        foreach (var ns in namespacesToReplace)
        {
            newName = newName.Replace(ns + ".", "");
        }

        //newName = newName
        //    .Replace("`1[", "<")
        //    .Replace("`2[", "<")
        //    .Replace("`3[", "<")
        //    .Replace("`4[", "<")
        //    .Replace("]", ">");

        return newName;
    }


    [Fact]
    public async Task GenerateTypesForTfsDll()
    {
        var types = new[] {
            typeof(Microsoft.TeamFoundation.Build.WebApi.Build),
            typeof(Microsoft.VisualStudio.Services.WebApi.AnswersDetails),
            typeof(Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient),
            typeof(Microsoft.TeamFoundation.Core.WebApi.WebApiTagDefinition),

        };
        List<string> namespaces = [
            "Microsoft.TeamFoundation.Build.WebApi",
            "Microsoft.VisualStudio.Services.WebApi",
            "Microsoft.TeamFoundation.SourceControl.WebApi",
            "Microsoft.TeamFoundation.Core.WebApi"
            ];
        DuplicatePublicClassesWithPropertiesAndEnums(types, "AzureDevopsExplorer.Database.Model.Generated", namespaces);
    }


    [Theory]
    [InlineData(typeof(System.String), "string")]
    [InlineData(typeof(System.Int32), "int")]
    [InlineData(typeof(System.Boolean), "bool")]
    [InlineData(typeof(Microsoft.TeamFoundation.Build.WebApi.AgentPoolQueue), "AgentPoolQueue")]
    [InlineData(typeof(System.Collections.Generic.List<Microsoft.TeamFoundation.Build.WebApi.Demand>), "System.Collections.Generic.List<Demand>")]
    [InlineData(typeof(System.Nullable<System.Boolean>), "System.Nullable<bool>")]
    [InlineData(typeof(System.Collections.Generic.IDictionary<System.String, Microsoft.TeamFoundation.Build.WebApi.BuildDefinitionVariable>), "System.Collections.Generic.IDictionary<string, BuildDefinitionVariable>")]
    public async Task TransformTypeCorrectly(Type fullName, string expected)
    {
        var baseNamespace = "Microsoft.TeamFoundation.Build.WebApi";
        var result = GetPropertyName(fullName, [baseNamespace]);
        Assert.Equal(expected, result);
    }
}