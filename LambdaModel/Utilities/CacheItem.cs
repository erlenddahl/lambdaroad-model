namespace LambdaModel.Utilities
{
    public class CacheItem<T>
    {
        public T Item;
        public int AddedAt;

        public CacheItem(T value)
        {
            Item = value;
        }
    }
}