namespace AzureDevopsExplorer.AzureDevopsApi.Security;

public class SecurityNamespace
{
    public string namespaceId { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string separatorValue { get; set; }
    public int elementLength { get; set; }
    public int writePermission { get; set; }
    public int readPermission { get; set; }
    public string dataspaceCategory { get; set; }
    public Action[] actions { get; set; }
    public int structureValue { get; set; }
    public string extensionType { get; set; }
    public bool isRemotable { get; set; }
    public bool useTokenTranslator { get; set; }
    public int systemBitMask { get; set; }
}

public class Action
{
    public int bit { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string namespaceId { get; set; }
}
