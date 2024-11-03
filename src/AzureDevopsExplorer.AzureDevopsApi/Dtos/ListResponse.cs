namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;


public class ListResponse<T>
{
    public int count { get; set; }
    public T[]? value { get; set; }
}
