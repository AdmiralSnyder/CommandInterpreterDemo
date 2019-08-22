using System.Collections.Generic;

namespace ReflectionTestApp
{
    public class Fetch<TKey, TValue> : IFetch<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> BackingDictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get => BackingDictionary[key];
            set => BackingDictionary[key] = value;
        }

        public bool TryGetValue(TKey key, out TValue value) => BackingDictionary.TryGetValue(key, out value);
    }
}
