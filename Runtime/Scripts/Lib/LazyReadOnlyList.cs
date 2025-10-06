using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LazyUI
{
#if true
    public readonly struct LazyReadOnlyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        private readonly List<T> list;
        public LazyReadOnlyList(List<T> list)
        {
            this.list = list;
        }
        public static implicit operator LazyReadOnlyList<T>(List<T> list)
        {
            return new LazyReadOnlyList<T>(list);
        }
        public int Capacity => list.Capacity;
        public int Count => list.Count;

        bool IList.IsFixedSize => ((IList)list).IsFixedSize;
        bool ICollection<T>.IsReadOnly => true;
        bool IList.IsReadOnly => true;
        bool ICollection.IsSynchronized => ((ICollection)list).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)list).SyncRoot;
        public T this[int index]
        {
            get => list[index];
            set => throw new InvalidOperationException();
        }
        object IList.this[int index]
        {
            get => list[index];
            set => throw new InvalidOperationException();
        }
        int IList.Add(object item)
        {
            throw new InvalidOperationException();
        }
        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return list.BinarySearch(index, count, item, comparer);
        }
        public int BinarySearch(T item)
        {
            return BinarySearch(0, Count, item, null);
        }
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, Count, item, comparer);
        }
        public void Add(T item)
        {
            throw new InvalidOperationException();
        }
        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        bool IList.Contains(object item)
        {
            return ((IList)list).Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return list.ConvertAll(converter);
        }
        public void CopyTo(T[] array)
        {
            list.CopyTo(array, 0);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            ((ICollection)list).CopyTo(array, 0);
        }
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            list.CopyTo(index, array, arrayIndex, count);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        public bool Exists(Predicate<T> match)
        {
            return list.Exists(match);
        }
        public T Find(Predicate<T> match)
        {
            return list.Find(match);
        }
        public List<T> FindAll(Predicate<T> match)
        {
            return list.FindAll(match);
        }
        public int FindIndex(Predicate<T> match)
        {
            return list.FindIndex(match);
        }
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return list.FindIndex(startIndex, match);
        }
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindIndex(startIndex, count, match);
        }
        public T FindLast(Predicate<T> match)
        {
            return list.FindLast(match);
        }
        public int FindLastIndex(Predicate<T> match)
        {
            return list.FindLastIndex(match);
        }
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, match);
        }
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, count, match);
        }
        public void ForEach(Action<T> action)
        {
            list.ForEach(action);
        }
        public List<T>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        public List<T> GetRange(int index, int count)
        {
            return list.GetRange(index, count);
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        int IList.IndexOf(object item)
        {
            return ((IList)list).IndexOf(item);
        }
        public int IndexOf(T item, int index)
        {
            return list.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return list.IndexOf(item, index, count);
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList.Insert(int index, object item)
        {
            throw new InvalidOperationException();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            throw new InvalidOperationException();
        }

        public int LastIndexOf(T item)
        {
            return list.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            return list.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            return list.LastIndexOf(item, index, count);
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException();
        }

        void IList.Remove(object item)
        {
            throw new InvalidOperationException();
        }

        public int RemoveAll(Predicate<T> match)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        public void RemoveRange(int index, int count)
        {
            throw new InvalidOperationException();
        }

        public void Reverse()
        {
            throw new NotSupportedException();
        }

        public void Reverse(int index, int count)
        {
            throw new NotSupportedException();
        }

        public void Sort()
        {
            throw new NotSupportedException();
        }

        public void Sort(IComparer<T> comparer)
        {
            throw new NotSupportedException();
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            throw new NotSupportedException();
        }

        public void Sort(Comparison<T> comparison)
        {
            throw new NotSupportedException();
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void TrimExcess()
        {
            throw new NotSupportedException();
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return list.TrueForAll(match);
        }
    }
#else
    public readonly struct ReadOnlyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
    {
        private readonly List<T> list;
        private ReadOnlyList(List<T> list)
        {
            this.list = list;
        }
        public static implicit operator ReadOnlyList<T>(List<T> list)
        {
            return new ReadOnlyList<T>(list);
        }
        public readonly int Count => list.Count;
        public readonly bool IsReadOnly => true;

        bool IList.IsFixedSize => ((IList)list).IsFixedSize;
        bool ICollection.IsSynchronized => ((ICollection)list).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)list).SyncRoot;
        object IList.this[int index]
        {
            get { return list[index]; }
            set
            {
                throw new InvalidOperationException();
            }
        }
        public T this[int index]
        {
            get { return list[index]; }
            set
            {
                throw new InvalidOperationException();
            }
        }
        public void Add(T item)
        {
            throw new InvalidOperationException();
        }
        int IList.Add(object value)
        {
            throw new InvalidOperationException();
        }
        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }
        void IList.Insert(int index, object value)
        {
            throw new InvalidOperationException();
        }
        public void Clear()
        {
            throw new InvalidOperationException();
        }
        public readonly bool Contains(T item)
        {
            return list.Contains(item);
        }
        bool IList.Contains(object value)
        {
            return ((IList)list).Contains(value);
        }
        public readonly void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }
        public readonly bool Remove(T item)
        {
            throw new InvalidOperationException();
        }
        void IList.Remove(object value)
        {
            throw new InvalidOperationException();
        }
        public readonly void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)list).GetEnumerator();
        }
        public readonly int IndexOf(T item)
        {
            return list.IndexOf(item);
        }
        int IList.IndexOf(object value)
        {
            return ((IList)list).IndexOf(value);
        }

        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly List<T> list;

            private int index;

            private T current;

            internal Enumerator(ReadOnlyList<T> list)
            {
                this.list = list.list;
                index = -1;
                current = default;
            }
            public readonly void Dispose()
            {
            }
            public readonly T Current
            {
                get
                {
                    if ((index < 0) || (index > list.Count))
                    {
                        throw new InvalidOperationException();
                    }
                    return current;
                }
            }
            readonly object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (index >= list.Count)
                {
                    index = list.Count + 1;
                    return false;
                }
                index++;
                current = list[index];
                return true;
            }
            public void Reset()
            {
                index = -1;
                current = default;
            }
        }
    }
#endif
}
