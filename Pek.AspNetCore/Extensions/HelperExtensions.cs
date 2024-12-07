using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Pek;

public static class HelperExtensions
{
    public static String ToEnvVariable(this IEnumerable<String> stringList, String seperator = ",") => String.Join(seperator, stringList.Select(d => d.Replace(@"\", @"\\").Replace(seperator, $@"\{seperator}")));

    public static void CopyTo(this StreamReader reader, StreamWriter writer, Int32 bufferSize = 4096)
    {
        var buffer = new Char[bufferSize];
        Int32 read;
        while ((read = reader.Read(buffer, 0, bufferSize)) > 0)
        {
            writer.Write(buffer, 0, read);
        }
    }

    public static void AddDictionary<TValue>(this AttributeDictionary attrDictionary, IDictionary<String, TValue> dictionary)
    {
        foreach (var kvp in dictionary)
            attrDictionary.Add(kvp.Key, kvp.Value?.ToString());
    }

    public static Boolean DictionaryEqual<TKey, TValue>(
        this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second) => first.DictionaryEqual(second, null);

    public static Boolean DictionaryEqual<TKey, TValue>(
        this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
        IEqualityComparer<TValue>? valueComparer)
    {
        if (first == second) return true;
        if ((first == null) || (second == null)) return false;
        if (first.Count != second.Count) return false;

        valueComparer ??= EqualityComparer<TValue>.Default;

        foreach (var kvp in first)
        {
            if (!second.TryGetValue(kvp.Key, out var secondValue)) return false;
            if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
        }
        return true;
    }
}