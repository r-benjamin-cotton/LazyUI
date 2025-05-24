using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// 子をそのままのサイズで指定方向へ整列する
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class LazyLayout : UIBehaviour
    {
        private enum Direction
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop,
        }
        [SerializeField]
        private RectOffset padding = new();

        [SerializeField]
        private Vector2 spacing = Vector2.zero;

        [SerializeField]
        private Direction direction = Direction.LeftToRight;

        [SerializeField]
        private bool autoSizing = false;

        private RectTransform rectTransform = null;

#if false
        protected override void OnBeforeTransformParentChanged()
        {
            base.OnBeforeTransformParentChanged();
            Debug.Log("OnBeforeTransformParentChanged");
        }
        protected override void OnCanvasGroupChanged()
        {
            base.OnCanvasGroupChanged();
            Debug.Log("OnCanvasGroupChanged");
        }
        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            Debug.Log("OnCanvasHierarchyChanged");
        }
        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            Debug.Log("OnDidApplyAnimationProperties");
        }
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            Debug.Log("OnRectTransformDimensionsChange");
        }
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            Debug.Log("OnTransformParentChanged");
        }
#endif
        private static readonly List<(RectTransform, Rect)> work = new();
        private void RecalcLayout()
        {
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var child = rectTransform.GetChild(i) as RectTransform;
                if ((child == null) || !child.gameObject.activeInHierarchy)
                {
                    continue;
                }
                var rect = child.GetRectInParent();
                work.Add((child, rect));
            }
            if (autoSizing)
            {
                var w = 0.0f;
                var h = 0.0f;
                if (work.Count > 0)
                {
                    switch (direction)
                    {
                        case Direction.LeftToRight:
                        case Direction.RightToLeft:
                            foreach ((var child, var rect) in work)
                            {
                                w += rect.size.x + spacing.x;
                                h = Mathf.Max(h, rect.size.y);
                            }
                            w -= spacing.x;
                            break;
                        case Direction.TopToBottom:
                        case Direction.BottomToTop:
                            foreach ((var child, var rect) in work)
                            {
                                w = Mathf.Max(w, rect.size.x);
                                h += rect.size.y + spacing.y;
                            }
                            h -= spacing.y;
                            break;
                    }
                }
                var rr = new Vector2(w + padding.horizontal, h + padding.vertical);
                var size = rectTransform.rect.size;
                var dx = rr - size;
                rectTransform.sizeDelta += dx;
            }
            {
                switch (direction)
                {
                    case Direction.LeftToRight:
                    case Direction.RightToLeft:
                        {
                            var r = rectTransform.rect;
                            var px = (direction == Direction.LeftToRight) ? (r.xMin + padding.left) : (r.xMax - padding.right);
                            var sx = (direction == Direction.LeftToRight) ? +1 : -1;
                            foreach ((var child, var rect) in work)
                            {
                                var dx = px + rect.width * 0.5f * sx - rect.center.x;
                                var dy = r.center.y - rect.center.y;
#if true
                                if (MathF.Abs(dy) < 0.001f)
                                {
                                    dy = 0;
                                }
#endif
                                var ap = child.anchoredPosition;
                                ap.x += dx;
                                ap.y += dy;
                                child.anchoredPosition = ap;
                                px += (rect.width + spacing.x) * sx;
                            }
                        }
                        break;
                    case Direction.TopToBottom:
                    case Direction.BottomToTop:
                        {
                            var r = rectTransform.rect;
                            var py = (direction == Direction.TopToBottom) ? (r.yMax - padding.top) : (r.yMin + padding.bottom);
                            var sy = (direction == Direction.TopToBottom) ? -1 : +1;
                            foreach ((var child, var rect) in work)
                            {
                                var dx = r.center.x - rect.center.x;
                                var dy = py + rect.height * 0.5f * sy - rect.center.y;
#if true
                                if (MathF.Abs(dx) < 0.001f)
                                {
                                    dx = 0;
                                }
#endif
                                var ap = child.anchoredPosition;
                                ap.x += dx;
                                ap.y += dy;
                                child.anchoredPosition = ap;
                                py += (rect.height + spacing.y) * sy;
                            }
                        }
                        break;
                }
            }
            work.Clear();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.LateUpdate, 0, RecalcLayout);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.LateUpdate, 0, RecalcLayout);
        }
        protected override void Awake()
        {
            base.Awake();
            rectTransform = GetComponent<RectTransform>();
        }
#if false
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}
