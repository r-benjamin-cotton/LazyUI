using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LazyUI
{
    public static class LazyVectorUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Div(Vector2 v0, Vector2 v1, float faulted = 0.0f)
        {
            var x = (v1.x != 0) ? (v0.x / v1.x) : faulted;
            var y = (v1.y != 0) ? (v0.y / v1.y) : faulted;
            return new Vector2(x, y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Div(Vector3 v0, Vector3 v1, float faulted = 0.0f)
        {
            var x = (v1.x != 0) ? (v0.x / v1.x) : faulted;
            var y = (v1.y != 0) ? (v0.y / v1.y) : faulted;
            var z = (v1.z != 0) ? (v0.z / v1.z) : faulted;
            return new Vector3(x, y, z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Rcp(Vector2 v0, float zeroDivided = 0.0f)
        {
            var x = (v0.x == 0) ? zeroDivided : (1.0f / v0.x);
            var y = (v0.y == 0) ? zeroDivided : (1.0f / v0.y);
            return new Vector2(x, y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rcp(Vector3 v0, float faulted = 0.0f)
        {
            var x = (v0.x != 0) ? (1.0f / v0.x) : faulted;
            var y = (v0.y != 0) ? (1.0f / v0.y) : faulted;
            var z = (v0.z != 0) ? (1.0f / v0.z) : faulted;
            return new Vector3(x, y, z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Mul(Vector3 v0, Vector3 v1)
        {
            return Vector3.Scale(v0, v1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Round(Vector2 v0)
        {
            return new Vector2(Mathf.Round(v0.x), Mathf.Round(v0.y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(Vector3 v0)
        {
            return new Vector3(Mathf.Round(v0.x), Mathf.Round(v0.y), Mathf.Round(v0.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(Vector2 v)
        {
            return new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(Vector3 v0)
        {
            return new Vector3(Mathf.Floor(v0.x), Mathf.Floor(v0.y), Mathf.Floor(v0.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceil(Vector2 v0)
        {
            return new Vector2(Mathf.Ceil(v0.x), Mathf.Ceil(v0.y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceil(Vector3 v0)
        {
            return new Vector3(Mathf.Ceil(v0.x), Mathf.Ceil(v0.y), Mathf.Ceil(v0.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Trunc(Vector2 v0)
        {
            return new Vector2((int)v0.x, (int)v0.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Trunc(Vector3 v0)
        {
            return new Vector3((int)v0.x, (int)v0.y, (int)v0.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Frac(Vector2 v)
        {
            return new Vector2(v.x - (int)v.x, v.y - (int)v.y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Frac(Vector3 v0)
        {
            return new Vector3(v0.x - (int)v0.x, v0.y - (int)v0.y, v0.z - (int)v0.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Splat2(float v0)
        {
            return new Vector2(v0, v0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Splat3(float v0)
        {
            return new Vector3(v0, v0, v0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(Vector2 v0)
        {
            return new Vector2(Mathf.Abs(v0.x), Mathf.Abs(v0.y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Abs(Vector3 v0)
        {
            return new Vector3(Mathf.Abs(v0.x), Mathf.Abs(v0.y), Mathf.Abs(v0.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sign(Vector3 v0)
        {
            return new Vector3(Mathf.Sign(v0.x), Mathf.Sign(v0.y), Mathf.Sign(v0.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Median(Vector3 v0)
        {
            var vt0 = v0.x;
            var vt1 = v0.y;
            var vt2 = v0.z;
            if (vt0 > vt1)
            {
                (vt0, vt1) = (vt1, vt0);
            }
            if (vt1 > vt2)
            {
                (vt1, vt2) = (vt2, vt1);
            }
            if (vt0 > vt1)
            {
                (vt0, vt1) = (vt1, vt0);
            }
            return vt1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 v0, Vector3 v1)
        {
            return Vector3.Min(v0, v1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 v0, Vector3 v1)
        {
            return Vector3.Max(v0, v1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Greatest(Vector3 v0)
        {
            return Mathf.Max(Mathf.Max(v0.x, v0.y), v0.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 v0, Vector3 v1)
        {
            return Vector3.Dot(v0, v1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 v0, Vector3 v1)
        {
            return Vector3.Cross(v0, v1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(Vector2 v0, Vector2 v1, float t)
        {
            return Vector2.Lerp(v0, v1, t);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(Vector2 v0, Vector2 v1, Vector2 t)
        {
            return new Vector2(Mathf.Lerp(v0.x, v1.x, t.x), Mathf.Lerp(v0.y, v1.y, t.y));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 v0, Vector3 v1, float t)
        {
            return Vector3.Lerp(v0, v1, t);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 v0, Vector3 v1, Vector3 t)
        {
            return new Vector3(Mathf.Lerp(v0.x, v1.x, t.x), Mathf.Lerp(v0.y, v1.y, t.y), Mathf.Lerp(v0.z, v1.z, t.z));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 v0, params Vector2[] vl)
        {
            var vt = v0;
            foreach (var vi in vl)
            {
                vt = Vector2.Min(vt, vi);
            }
            return vt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 v0, params Vector3[] vl)
        {
            var vt = v0;
            foreach (var vi in vl)
            {
                vt = Vector3.Min(vt, vi);
            }
            return vt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 v0, params Vector2[] vl)
        {
            var vt = v0;
            foreach (var vi in vl)
            {
                vt = Vector2.Max(vt, vi);
            }
            return vt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 v0, params Vector3[] vl)
        {
            var vt = v0;
            foreach (var vi in vl)
            {
                vt = Vector3.Max(vt, vi);
            }
            return vt;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsMin(float v0, float v1)
        {
            return (Mathf.Abs(v0) <= Mathf.Abs(v1)) ? v0 : v1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsMin(float v0, float v1, float v2, float v3)
        {
            return AbsMin(AbsMin(v0, v1), AbsMin(v2, v3));
        }
    }
}
