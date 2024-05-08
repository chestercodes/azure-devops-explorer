using KellermanSoftware.CompareNetObjects;

namespace AzureDevopsExplorer.Application;

public static class CompareLogicExtensions
{
    // force the types to match
    public static ComparisonResult CompareSameType<T>(this CompareLogic that, T expectedObject, T actualObject)
    {
        return that.Compare(expectedObject, actualObject);
    }
}
