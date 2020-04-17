using System;

namespace RootTools.Inspects
{
    public class Singleton<T> where T : class, new()
    {
        public static readonly Lazy<T> lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return lazy.Value; } }
        public Singleton()
        {
        }
    }
}
