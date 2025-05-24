using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    public static class RectTransformExtension
    {
        public static Vector2 Transform(this RectTransform from, Vector2 pos, RectTransform to)
        {
            var l2l = to.worldToLocalMatrix * from.localToWorldMatrix;
            var vc = l2l.MultiplyPoint3x4(pos);
            return vc;
        }
        public static Rect Transform(this RectTransform from, Rect rect, RectTransform to)
        {
            var l2l = to.worldToLocalMatrix * from.localToWorldMatrix;
#if true
            // CalculateRelativeRectTransformBoundsだとworldcorner取得してからローカルへ変換してるけれど
            var v00 = rect.min;
            var v11 = rect.max;
            var v01 = new Vector2(v00.x, v11.y);
            var v10 = new Vector2(v11.x, v00.y);
            var t00 = l2l.MultiplyPoint3x4(v00);
            var t01 = l2l.MultiplyPoint3x4(v01);
            var t10 = l2l.MultiplyPoint3x4(v10);
            var t11 = l2l.MultiplyPoint3x4(v11);
            var min = Vector2.Min(Vector2.Min(t00, t01), Vector2.Min(t10, t11));
            var max = Vector2.Max(Vector2.Max(t00, t01), Vector2.Max(t10, t11));
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
#else
            //簡易
            var vc = l2l.MultiplyPoint3x4(rect.center);
            var vx = l2l.MultiplyVector(new Vector3(rect.width * 0.5f, 0, 0));
            var vy = l2l.MultiplyVector(new Vector3(0, rect.height * 0.5f, 0));
            var ex = Abs(vx) + Abs(vy);
            return new Rect(vc - ex, ex * 2.0f);
#endif
        }
        public static Rect GetRectInParent(this RectTransform rectTransform)
        {
            var parentRectTransform = rectTransform.parent as RectTransform;
#if false
            // test
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parentRectTransform, rectTransform);
            return new Rect(bounds.min, bounds.size);
#endif
            var rect = rectTransform.Transform(rectTransform.rect, parentRectTransform);
            return rect;
        }
        public static Rect GetRectInParentZ(this RectTransform rectTransform)
        {
            var parentRectTransform = rectTransform.parent as RectTransform;
            var rect = rectTransform.Transform(rectTransform.rect, parentRectTransform);
            rect.position -= parentRectTransform.rect.position;
            return rect;
        }
        public static Rect GetParentRect(this RectTransform rectTransform)
        {
            var parentRectTransform = rectTransform.parent as RectTransform;
            return parentRectTransform.rect;
        }
#if false
        public static Rect GetRectInParentSpace(this RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            var offset = rectTransform.anchoredPosition - Vector2.Scale(rectTransform.sizeDelta, rectTransform.pivot);
            var vector = offset + Vector2.Scale(rect.size, rectTransform.pivot);
            var parentRectTransform = rectTransform.parent as RectTransform;
            if (parentRectTransform != null)
            {
                vector += Vector2.Scale(rectTransform.anchorMin, parentRectTransform.rect.size);
            }
            rect.x += vector.x;
            rect.y += vector.y;
            return rect;
        }
#endif
    }
}
