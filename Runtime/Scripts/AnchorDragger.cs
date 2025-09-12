using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    using static VectorUtil;

    /// <summary>
    /// AnchorCoordinatroにDrag機能を追加
    /// ・bounding:親のRect境界に収める
    /// ・snap:同じ親の仲でスナップ
    /// ・save:PlayerPrefsへアンカーを保存
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AnchorDragger : AnchorCoordinator, IInitializePotentialDragHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private bool bounding = false;

        [SerializeField]
        private bool snap = false;

        [SerializeField]
        private bool save = false;

        [SerializeField]
        private string playerPrefsKeyPrefix = "";

        private int snapDistance = 4;

        private bool dragging = false;
        private int button = -1;
        private Vector2 origin = default;
        private Vector2 dragAnchorPos = default;

        private static readonly HashSet<AnchorDragger> draggers = new();

        private void Drag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 target);
            var dt = target - origin;
#if true
            dt = Floor(dt);
#endif
            rectTransform.anchoredPosition = dragAnchorPos + dt;
        }
        private void Snap()
        {
            var r0 = rectTransform.Transform(rectTransform.rect, parentRectTransform);
            var dx = float.MaxValue;
            var dy = float.MaxValue;
            foreach (var ax in draggers)
            {
                if ((ax == null) || (ax == this) || !ax.snap || !ReferenceEquals(ax.transform.parent, parentRectTransform))
                {
                    continue;
                }
                var r1 = ax.rectTransform.Transform(ax.rectTransform.rect, parentRectTransform);
                var dx0 = r0.min.x - r1.min.x;
                var dx1 = r0.max.x - r1.min.x;
                var dx2 = r0.min.x - r1.max.x;
                var dx3 = r0.max.x - r1.max.x;
                dx = AbsMin(dx, AbsMin(dx0, dx1, dx2, dx3));
                var dy0 = r0.min.y - r1.min.y;
                var dy1 = r0.max.y - r1.min.y;
                var dy2 = r0.min.y - r1.max.y;
                var dy3 = r0.max.y - r1.max.y;
                dy = AbsMin(dy, AbsMin(dy0, dy1, dy2, dy3));
            }
            var dt = Vector2.zero;
            if (Mathf.Abs(dx) <= snapDistance)
            {
                dt.x -= dx;
            }
            if (Mathf.Abs(dy) <= snapDistance)
            {
                dt.y -= dy;
            }
            rectTransform.anchoredPosition += dt;
        }
        private bool Bounding()
        {
            var pr = parentRectTransform.rect;
            if ((pr.width == 0) || (pr.height == 0))
            {
                return false;
            }
            var cr = rectTransform.GetRectInParent();
            var cs = cr.size;
            var ps = pr.size;
            var dt = Vector2.zero;
            if (cs.x > ps.x)
            {
                if (cr.xMin > pr.xMin)
                {
                    dt.x -= cr.xMin - pr.xMin;
                }
                if (cr.xMax < pr.xMax)
                {
                    dt.x += pr.xMax - cr.xMax;
                }
            }
            else
            {
                if (cr.xMin < pr.xMin)
                {
                    dt.x += pr.xMin - cr.xMin;
                }
                if (cr.xMax > pr.xMax)
                {
                    dt.x -= cr.xMax - pr.xMax;
                }
            }
            if (cs.y > ps.y)
            {
                if (cr.yMin > pr.yMin)
                {
                    dt.y -= cr.yMin - pr.yMin;
                }
                if (cr.yMax < pr.yMax)
                {
                    dt.y += pr.yMax - cr.yMax;
                }
            }
            else
            {
                if (cr.yMin < pr.yMin)
                {
                    dt.y += pr.yMin - cr.yMin;
                }
                if (cr.yMax > pr.yMax)
                {
                    dt.y -= cr.yMax - pr.yMax;
                }
            }
            if (dt == Vector2.zero)
            {
                return false;
            }
            rectTransform.anchoredPosition += dt;
            return true;
        }
        protected override void OnCoordinateAnchor()
        {
            if (dragging)
            {
                return;
            }
            if (bounding)
            {
                Bounding();
            }
            base.OnCoordinateAnchor();
        }
        protected void BeginDrag(PointerEventData eventData)
        {
            var p0 = eventData.position;
            var parentRectTransform = rectTransform.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, p0, eventData.pressEventCamera, out origin);
            dragAnchorPos = rectTransform.anchoredPosition;
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (button < 0)
            {
                button = (int)eventData.button;
                dragging = true;
                BeginDrag(eventData);
            }
            else
            {
                dragging = false;
            }
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (button == (int)eventData.button)
            {
                button = -1;
            }
            dragging = false;
        }
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = dragging;
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            parentRectTransform = rectTransform.parent as RectTransform;
            if (parentRectTransform != null)
            {
                {
                    Drag(eventData);
                }
                if (snap)
                {
                    Snap();
                }
                if (bounding)
                {
                    Bounding();
                }
                parentRectTransform = null;
            }
        }
        private Vector2 GetAnchor()
        {
            var pr = rectTransform.GetParentRect();
            var cr = rectTransform.GetRectInParent();
            var pv = rectTransform.pivot;
            if (AutoPivotX)
            {
                pv.x = 0.5f;
            }
            if (AutoPivotY)
            {
                pv.y = 0.5f;
            }
            return (Lerp(cr.min, cr.max, pv) - pr.min) * Rcp(pr.size);
        }
        private void Save()
        {
            if (!save)
            {
                return;
            }
            //LazyDebug.Log("AnchorDragger.Save()");
            {
                var anchor = GetAnchor();
                LazyPlayerPrefs.SetValue(playerPrefsKeyPrefix + ".Anchor", anchor);
            }
        }
        private void Load()
        {
            snapDistance = LazyPlayerPrefs.GetValue("SnapDistance", 4);
            if (!save)
            {
                return;
            }
            //LazyDebug.Log("AnchorDragger.Load()");
            {
                var anchor = GetAnchor();
                var target = LazyPlayerPrefs.GetValue(playerPrefsKeyPrefix + ".Anchor", anchor);
                rectTransform.anchoredPosition += (target - anchor) * rectTransform.GetParentRect().size;
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();
            Load();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            {
                draggers.Add(this);
            }
            if (refCount++ == 0)
            {
                LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.Quitting, 0, SaveAll);
            }
        }
        protected override void OnDisable()
        {
            if (--refCount == 0)
            {
                LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.Quitting, 0, SaveAll);
            }
            {
                draggers.Remove(this);
            }
            Save();
            button = -1;
            base.OnDisable();
        }
        private static int refCount = 0;

        public override bool Equals(object other)
        {
            return ReferenceEquals(this, other);
        }
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        }
        public static void SaveAll()
        {
            var acs = FindObjectsByType<AnchorDragger>(FindObjectsSortMode.None);
            foreach (var dg in acs)
            {
                if (dg.isActiveAndEnabled)
                {
                    dg.Save();
                }
            }
            //LazyPlayerPrefs.Save();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (save && string.IsNullOrEmpty(playerPrefsKeyPrefix))
            {
                playerPrefsKeyPrefix = transform.GetFullPath(".");
            }
        }
        [UnityEditor.MenuItem("LazyUI/AnchorDragger.ForceSave()")]
        private static void ForceSave()
        {
            var acs = FindObjectsByType<AnchorDragger>(FindObjectsSortMode.None);
            foreach (var dg in acs)
            {
                dg.Save();
            }
            LazyPlayerPrefs.Save();
        }
        [UnityEditor.MenuItem("LazyUI/AnchorDragger.ForceLoad()")]
        private static void ForceLoad()
        {
            LazyPlayerPrefs.Load();
            var acs = FindObjectsByType<AnchorDragger>(FindObjectsSortMode.None);
            foreach (var dg in acs)
            {
                dg.Load();
            }
        }
#endif
    }
}
