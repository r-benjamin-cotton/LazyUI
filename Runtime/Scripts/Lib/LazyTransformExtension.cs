using System;
using UnityEngine;

namespace LazyUI
{
    public static class LazyTransformExtension
    {
        public static string GetFullPath(this Transform transform, string separator = "/")
        {
            if (transform == null)
            {
                return "";
            }
            var path = transform.name;
            for (var t = transform.parent; t != null; t = t.parent)
            {
                path = t.name + separator + path;
            }
            return path;
        }
    }
}
