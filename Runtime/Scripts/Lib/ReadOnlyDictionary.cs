using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyUI
{
    public struct ReadOnlyDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IDictionary
    {
        private readonly Dictionary<TKey, TValue> dict;
        public ReadOnlyDictionary(Dictionary<TKey, TValue> dict)
        {
            this.dict = dict;
        }
        public static implicit operator ReadOnlyDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }

        public TValue this[TKey key]
        {
            readonly get => dict[key];
            set => throw new InvalidOperationException();
        }

        public readonly Dictionary<TKey, TValue>.KeyCollection Keys => dict.Keys;
        public readonly Dictionary<TKey, TValue>.ValueCollection Values => dict.Values;
        public readonly IEqualityComparer<TKey> Comparer => dict.Comparer;
        public readonly int Count => dict.Count;


        public readonly bool IsReadOnly => true;

        readonly ICollection<TKey> IDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)dict).Keys;

        readonly ICollection<TValue> IDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)dict).Values;

        readonly IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IReadOnlyDictionary<TKey, TValue>)dict).Keys;

        readonly IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IReadOnlyDictionary<TKey, TValue>)dict).Values;

        readonly bool ICollection.IsSynchronized => ((ICollection)dict).IsSynchronized;

        readonly object ICollection.SyncRoot => ((ICollection)dict).SyncRoot;

        readonly bool IDictionary.IsFixedSize => ((IDictionary)dict).IsFixedSize;

        readonly ICollection IDictionary.Keys => ((IDictionary)dict).Keys;

        readonly ICollection IDictionary.Values => ((IDictionary)dict).Values;

        object IDictionary.this[object key]
        {
            readonly get => ((IDictionary)dict)[key];
            set => throw new InvalidOperationException();
        }

        public void Add(TKey key, TValue value)
        {
            throw new InvalidOperationException();
        }
        public void Clear()
        {
            throw new InvalidOperationException();
        }
        public readonly bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }
        public readonly bool ContainsValue(TValue value)
        {
            return dict.ContainsValue(value);
        }
        public int EnsureCapacity(int capacity)
        {
            throw new InvalidOperationException();
        }
        public readonly Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return dict.GetEnumerator();
        }
        public bool Remove(TKey key, out TValue value)
        {
            throw new InvalidOperationException();
        }
        public bool Remove(TKey key)
        {
            throw new InvalidOperationException();
        }
        public void TrimExcess()
        {
            throw new InvalidOperationException();
        }
        public void TrimExcess(int capacity)
        {
            throw new InvalidOperationException();
        }
        public bool TryAdd(TKey key, TValue value)
        {
            throw new InvalidOperationException();
        }
        public readonly bool TryGetValue(TKey key, out TValue value)
        {
            return dict.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new InvalidOperationException();
        }
        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new InvalidOperationException();
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new InvalidOperationException();
        }
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        readonly bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dict).Contains(item);
        }
        readonly void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dict).CopyTo(array, arrayIndex);
        }
        readonly void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)dict).CopyTo(array, index);
        }
        void IDictionary.Add(object key, object value)
        {
            throw new InvalidOperationException();
        }
        readonly bool IDictionary.Contains(object key)
        {
            return ((IDictionary)dict).Contains(key);
        }
        readonly IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)dict).GetEnumerator();
        }
        void IDictionary.Remove(object key)
        {
            throw new InvalidOperationException();
        }
    }
}
