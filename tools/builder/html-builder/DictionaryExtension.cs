namespace ereadian.builder.html;

public static class DictionaryExtension
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        public void Add(IDictionary<TKey, TValue> source)
        {
            foreach(var pair in source)
            {
                if (!dictionary.ContainsKey(pair.Key))
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
            }
        }

        public void Add(IReadOnlyDictionary<TKey, TValue> source)
        {
            foreach(var pair in source)
            {
                if (!dictionary.ContainsKey(pair.Key))
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
