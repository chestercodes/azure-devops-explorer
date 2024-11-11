namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;

public class CombinationsPicker
{
    public static Dictionary<int, IEnumerable<int[]>> Values = new Dictionary<int, IEnumerable<int[]>>();

    public static IEnumerable<int[]> GetCombinations(int length)
    {
        if (Values.ContainsKey(length))
        {
            return Values[length];
        }
        var result = Combinations(Enumerable.Range(0, length));
        Values[length] = result;
        return result;
    }
    private static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source)
    {
        // https://stackoverflow.com/a/57058345/4854368
        if (null == source)
            throw new ArgumentNullException(nameof(source));

        T[] data = source.ToArray();

        return Enumerable
          .Range(1, (1 << data.Length) - 1)
          .Select(index => data
             .Where((v, i) => (index & 1 << i) != 0)
             .ToArray());
    }

}