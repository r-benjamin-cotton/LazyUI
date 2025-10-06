using System;
using System.Collections;
using System.Collections.Generic;

namespace LazyUI
{
    public readonly struct LazyReadOnlyArray<T> : IEnumerable<T>, IEnumerable
    {
        private readonly T[] array;
        public LazyReadOnlyArray(T[] array)
        {
            this.array = array;
        }
        public LazyReadOnlyArray(LazyReadOnlyArray<T> array)
        {
            this.array = array.array;
        }
        public static implicit operator LazyReadOnlyArray<T>(T[] array)
        {
            return new LazyReadOnlyArray<T>(array);
        }
        public static implicit operator ReadOnlySpan<T>(LazyReadOnlyArray<T> array)
        {
            return array.array;
        }
        public readonly T this[int i]
        {
            get { return array[i]; }
        }
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            return array;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] array;
            private int index;
            public Enumerator(T[] array)
            {
                this.array = array;
                index = -1;
            }
            public readonly T Current => index < 0 ? default : array[index];

            readonly object IEnumerator.Current => Current;
            public void Dispose()
            {
            }
            public bool MoveNext()
            {
                if (index >= array.Length)
                {
                    return false;
                }
                index++;
                return true;
            }
            public void Reset()
            {
                index = -1;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(array);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }
}
