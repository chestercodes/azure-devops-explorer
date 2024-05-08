namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

/// <summary>
/// TODO make Pascal case???
/// </summary>
public class SearchResponse
{
    public int count { get; set; }
    public SearchResponseResult[] results { get; set; }
    public int infoCode { get; set; }
    public SearchResponseFacets facets { get; set; }
}

public class SearchResponseFacets
{
    public SearchResponseProject[] Project { get; set; }
    public SearchResponseRepository[] Repository { get; set; }
}

public class SearchResponseProject
{
    public string name { get; set; }
    public string id { get; set; }
    public int resultCount { get; set; }
}

public class SearchResponseRepository
{
    public string name { get; set; }
    public string id { get; set; }
    public int resultCount { get; set; }
}

public class SearchResponseResult
{
    public string fileName { get; set; }
    public string path { get; set; }
    public SearchResponseMatches matches { get; set; }
    public SearchResponseCollection collection { get; set; }
    public SearchResponseProject1 project { get; set; }
    public SearchResponseRepository1 repository { get; set; }
    public SearchResponseVersion[] versions { get; set; }
    public string contentId { get; set; }
}

public class SearchResponseMatches
{
    public SearchResponseContent[] content { get; set; }
    //public object[] fileName { get; set; }
}

public class SearchResponseContent
{
    public int charOffset { get; set; }
    public int length { get; set; }
    public int line { get; set; }
    public int column { get; set; }
    public object codeSnippet { get; set; }
    public string type { get; set; }
}

public class SearchResponseCollection
{
    public string name { get; set; }
}

public class SearchResponseProject1
{
    public string name { get; set; }
    public string id { get; set; }
}

public class SearchResponseRepository1
{
    public string name { get; set; }
    public string id { get; set; }
    public string type { get; set; }
}

public class SearchResponseVersion
{
    public string branchName { get; set; }
    public string changeId { get; set; }
}
