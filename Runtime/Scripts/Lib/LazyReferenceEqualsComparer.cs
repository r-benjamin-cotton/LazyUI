using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyUI
{
    /// <summary>
    /// 明示的に参照比較を行うComparer
    /// (通常は参照型の.Equalsは参照比較なので使わなくても大丈夫だけれど)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyReferenceEqualsComparer<T> : IEqualityComparer<T>, IEqualityComparer where T : class
    {
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }
        public int GetHashCode(T obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
        bool IEqualityComparer.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        int IEqualityComparer.GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
        public static LazyReferenceEqualsComparer<T> Default { get; private set; } = new LazyReferenceEqualsComparer<T>();
    }
}
