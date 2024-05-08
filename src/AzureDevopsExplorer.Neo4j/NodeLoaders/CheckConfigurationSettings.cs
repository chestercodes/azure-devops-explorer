namespace AzureDevopsExplorer.Neo4j.NodeLoaders
{
    public class CheckConfigurationSettings
    {
        public Approver[]? approvers { get; set; }
        public string? executionOrder { get; set; }
        public int? minRequiredApprovers { get; set; }
        public string? instructions { get; set; }
        //public object[] blockedApprovers { get; set; }
        public string? displayName { get; set; }
        public Definitionref? definitionRef { get; set; }
        public Inputs? inputs { get; set; }
        public int? retryInterval { get; set; }
        public string? linkedVariableGroup { get; set; }
        public Extendscheck[]? extendsChecks { get; set; }
    }
}

public class Definitionref
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? version { get; set; }
}

public class Inputs
{
    public string? allowedBranches { get; set; }
    public string? ensureProtectionOfBranch { get; set; }
    public string? displayName { get; set; }
    public string? retryInterval { get; set; }
    public string? timeout { get; set; }
    public string? linkedVariableGroup { get; set; }
    public string? method { get; set; }
    public string? waitForCompletion { get; set; }
    public string? function { get; set; }
    public string? key { get; set; }
    public string? body { get; set; }
    public string? headers { get; set; }
    public string? businessDays { get; set; }
    public string? timeZone { get; set; }
    public string? startTime { get; set; }
    public string? endTime { get; set; }
    public string? filterType { get; set; }
    public string? connectedServiceNameARM { get; set; }
    public string? ResourceGroupName { get; set; }
}

public class Approver
{
    public string? displayName { get; set; }
    public string? id { get; set; }
    public string? uniqueName { get; set; }
    public string? imageUrl { get; set; }
    public string? descriptor { get; set; }
}

public class Extendscheck
{
    public string? repositoryType { get; set; }
    public string? repositoryName { get; set; }
    public string? repositoryRef { get; set; }
    public string? templatePath { get; set; }
}
