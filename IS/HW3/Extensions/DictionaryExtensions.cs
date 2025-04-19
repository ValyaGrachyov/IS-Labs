namespace HW3.Extensions;

public static class DictionaryExtensions
{
    public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, IComparer<TKey>? comparer) 
        where TKey : notnull
    {
        return new SortedDictionary<TKey, TValue>(dictionary, comparer);
    }
    
    public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, IComparer<TKey>? comparer) 
        where TKey : notnull
    {
        var dictionary = enumerable.ToDictionary();
        return new SortedDictionary<TKey, TValue>(dictionary, comparer);
    }
}