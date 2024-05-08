using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class SecurityNamespace
{
    [JsonProperty("namespaceId")]
    public string NamespaceId { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("separatorValue")]
    public string SeparatorValue { get; set; }
    [JsonProperty("elementLength")]
    public int ElementLength { get; set; }
    [JsonProperty("writePermission")]
    public int WritePermission { get; set; }
    [JsonProperty("readPermission")]
    public int ReadPermission { get; set; }
    [JsonProperty("dataspaceCategory")]
    public string DataspaceCategory { get; set; }
    [JsonProperty("actions")]
    public Action[] Actions { get; set; }
    [JsonProperty("structureValue")]
    public int StructureValue { get; set; }
    [JsonProperty("extensionType")]
    public string ExtensionType { get; set; }
    [JsonProperty("isRemotable")]
    public bool IsRemotable { get; set; }
    [JsonProperty("useTokenTranslator")]
    public bool UseTokenTranslator { get; set; }
    [JsonProperty("systemBitMask")]
    public int SystemBitMask { get; set; }
}

public class Action
{
    [JsonProperty("bit")]
    public int Bit { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
    [JsonProperty("namespaceId")]
    public string NamespaceId { get; set; }
}
