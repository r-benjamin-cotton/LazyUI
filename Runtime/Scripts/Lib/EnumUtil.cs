using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// Enum型を楽に使うためのユーティリティ
    /// </summary>
    public static class EnumUtil
    {
        /// <summary>
        /// Enumからintへ変換
        /// </summary>
        /// <param name="v0"></param>
        /// <returns></returns>
        public static int ToInteger(Enum v0)
        {
            //LazyDebug.Assert(GetValueType(v0.GetType()) == typeof(int));
            return (int)(object)v0;
        }
        public static int ToInteger(object v0)
        {
            return (int)v0;
        }
        /// <summary>
        /// v0のうちv1のいずれかのビットが立っていればtrue
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static bool Any(Enum v0, Enum v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            return (i0 & i1) != 0;
        }
        public static bool Any(object v0, object v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            return (i0 & i1) != 0;
        }
        /// <summary>
        /// v0のうちv1のすべてのビットが立っていればtrue
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static bool All(Enum v0, Enum v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            return (i0 & i1) == i1;
        }
        public static bool All(object v0, object v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            return (i0 & i1) == i1;
        }
        /// <summary>
        /// v0のv1ビットを立てて返す
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Enum Set(Enum v0, Enum v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            var t0 = v0.GetType();
            LazyDebug.Assert(t0 == v1.GetType());
            return (Enum)Enum.ToObject(t0, i0 | i1);
        }
        public static object Set(object v0, object v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            var t0 = v0.GetType();
            LazyDebug.Assert(t0.IsEnum);
            LazyDebug.Assert(t0 == v1.GetType());
            return Enum.ToObject(t0, i0 | i1);
        }
        /// <summary>
        /// v0のv1ビットを下ろして返す
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Enum Clear(Enum v0, Enum v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            var t0 = v0.GetType();
            LazyDebug.Assert(t0 == v1.GetType());
            return (Enum)Enum.ToObject(t0, i0 & ~i1);
        }
        public static object Clear(object v0, object v1)
        {
            var i0 = ToInteger(v0);
            var i1 = ToInteger(v1);
            var t0 = v0.GetType();
            LazyDebug.Assert(t0.IsEnum);
            LazyDebug.Assert(t0 == v1.GetType());
            return Enum.ToObject(t0, i0 & ~i1);
        }
        /// <summary>
        /// typeで指定されたenum型のデフォルト値=0をEnum型に変換して返す
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Enum DefaultValue(Type type)
        {
            if (!type.IsEnum)
            {
                return null;
            }
            return (Enum)Enum.ToObject(type, 0);
        }
        /// <summary>
        /// 先頭のField(_value)で型を判別できる？怪しい
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetValueType(Type type)
        {
            if (!type.IsEnum)
            {
                return null;
            }
            return type.GetFields()[0].FieldType;
        }
    }
}
