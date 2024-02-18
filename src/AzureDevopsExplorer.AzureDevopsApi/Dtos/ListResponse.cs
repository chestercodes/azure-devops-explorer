using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;


public class ListResponse<T>
{
    [JsonProperty("count")]
    public int Count { get; set; }
    [JsonProperty("value")]
    public T[] Value { get; set; }
}
