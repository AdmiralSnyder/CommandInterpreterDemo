namespace ReflectionTestApp
{
    #endregion

    #region Helper Classes

    public interface IFetch<TKey, TValue>
    {
        public TValue this[TKey key] { get; }
        public bool TryGetValue(TKey key, out TValue value);
    }
}
