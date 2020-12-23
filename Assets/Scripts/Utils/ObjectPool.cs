using System.Collections.Generic;

namespace SardineFish.Utils
{
    public static class ObjectPool<T> where T : new()
    {
        private static Stack<T> pool = new Stack<T>();

        public static T Get()
        {
            if (pool.Count > 0)
                return pool.Pop();
            return new T();
        }

        public static void Release(T obj)
            => pool.Push(obj);
    }
}