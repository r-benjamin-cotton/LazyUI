using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    public static class LazyRangeExtensions
    {
        public static bool Valid(this LazyRange<int> range)
        {
            return range.MinValue <= range.MaxValue;
        }
        public static bool Valid(this LazyRange<float> range)
        {
            return range.MinValue <= range.MaxValue;
        }
        public static LazyRange<float> Validate(this LazyRange<float> range)
        {
            var min = Mathf.Min(range.MinValue, range.MaxValue);
            var max = Mathf.Max(range.MinValue, range.MaxValue);
            return new LazyRange<float>(min, max);
        }
        public static LazyRange<int> Validate(this LazyRange<int> range)
        {
            var min = Mathf.Min(range.MinValue, range.MaxValue);
            var max = Mathf.Max(range.MinValue, range.MaxValue);
            return new LazyRange<int>(min, max);
        }
        public static int Clamp(this LazyRange<int> range, int value)
        {
            if (range.MinValue > range.MaxValue)
            {
                return value;
            }
            return Math.Clamp(value, range.MinValue, range.MaxValue);
        }
        public static float Clamp(this LazyRange<float> range, float value)
        {
            if (range.MinValue > range.MaxValue)
            {
                return value;
            }
            return Math.Clamp(value, range.MinValue, range.MaxValue);
        }
        public static LazyRange<int> Clamp(this LazyRange<int> range, LazyRange<int> r0)
        {
            var min = range.Clamp(r0.MinValue);
            var max = range.Clamp(r0.MaxValue);
            return new LazyRange<int>(min, max);
        }
        public static LazyRange<float> Clamp(this LazyRange<float> range, LazyRange<float> r0)
        {
            var min = range.Clamp(r0.MinValue);
            var max = range.Clamp(r0.MaxValue);
            return new LazyRange<float>(min, max);
        }

        public static int Repat(this LazyRange<int> range, int value)
        {
            var d = range.MaxValue - range.MinValue;
            if (d < 0)
            {
                return value;
            }
            if (d == 0)
            {
                return range.MinValue;
            }
            var f = value % d;
            if (f < 0)
            {
                f += d;
            }
            return f;
        }
        public static float Repeat(this LazyRange<float> range, float value)
        {
            var d = range.MaxValue - range.MinValue;
            if (d < 0)
            {
                return value;
            }
            if (d == 0)
            {
                return range.MinValue;
            }
            var f = value % d;
            if (f < 0)
            {
                f += d;
            }
            return f;
        }
        public static LazyRange<int> Repeat(this LazyRange<int> range, LazyRange<int> r0)
        {
            var min = range.Repat(r0.MinValue);
            var max = range.Repat(r0.MaxValue);
            return new LazyRange<int>(min, max);
        }
        public static LazyRange<float> Repeat(this LazyRange<float> range, LazyRange<float> r0)
        {
            var min = range.Repeat(r0.MinValue);
            var max = range.Repeat(r0.MaxValue);
            return new LazyRange<float>(min, max);
        }
    }
    /// <summary>
    /// 範囲数値型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public readonly struct LazyRange<T> : IEquatable<LazyRange<T>> where T : struct, IEquatable<T>
    {
        private readonly T min;
        private readonly T max;
        public readonly T MinValue => min;
        public readonly T MaxValue => max;
        public LazyRange(T min, T max)
        {
            this.min = min;
            this.max = max;
        }
#if false
        public readonly bool ZeroWidth
        {
            get { return max.CompareTo(min) <= 0; }
        }
        public readonly bool Valid
        {
            get { return max.CompareTo(min) >= 0; }
        }
        private static T Min(T v0, T v1)
        {
            return (v0.CompareTo(v1) <= 0) ? v0 : v1;
        }
        private static T Max(T v0, T v1)
        {
            return (v0.CompareTo(v1) >= 0) ? v0 : v1;
        }
        public readonly Range<T> Clamp(Range<T> r0)
        {
            if (!Valid)
            {
                return r0;
            }
            var mn = Max(min, r0.min);
            var mx = Min(max, r0.max);
            return new Range<T>(mn, mx);
        }
        public readonly T Clamp(T value)
        {
            if (!Valid)
            {
                return value;
            }
            if (value.CompareTo(min) < 0)
            {
                return min;
            }
            if (value.CompareTo(max) > 0)
            {
                return max;
            }
            return value;
        }
#endif
        public static bool operator ==(LazyRange<T> v0, LazyRange<T> v1)
        {
            return v0.Equals(v1);
        }
        public static bool operator !=(LazyRange<T> v0, LazyRange<T> v1)
        {
            return !v0.Equals(v1);
        }

        public readonly bool Equals(LazyRange<T> other)
        {
            return min.Equals(other.min) && max.Equals(other.max);
        }
        public readonly override bool Equals(object obj)
        {
            if (obj is LazyRange<T> range)
            {
                return Equals(range);
            }
            return false;
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(min, max);
        }
        public readonly override string ToString()
        {
            return $"{{{min},{max}}}";
        }
        private static string[] Split(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (!value.StartsWith('{') || !value.EndsWith('}'))
            {
                return null;
            }
            var tt = value[1..^1].Split(',');
            if (tt.Length != 2)
            {
                return null;
            }
            return tt;
        }
        public static bool TryParse(string value, out LazyRange<float> result)
        {
            result = default;
            var tt = Split(value);
            if (tt == null)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out float v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out float v1))
            {
                return false;
            }
            result = new LazyRange<float>(v0, v1);
            return true;
        }
        public static bool TryParse(string value, out LazyRange<int> result)
        {
            result = default;
            var tt = Split(value);
            if (tt == null)
            {
                return false;
            }
            if (!int.TryParse(tt[0], out int v0))
            {
                return false;
            }
            if (!int.TryParse(tt[1], out int v1))
            {
                return false;
            }
            result = new LazyRange<int>(v0, v1);
            return true;
        }
    }
}
