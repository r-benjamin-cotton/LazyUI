using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// Dropdownみたいなの。指定オプションから一つを選択できる。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Selector : Selectable, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public enum DirectionType
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }
        [Serializable]
        public struct Item
        {
            public string text;
            public Sprite icon;
            public RectTransform element;
        }

        [Serializable]
        public class SelectorEvent : UnityEvent<int> { }

        [SerializeField]
        private RectTransform template = null;
        [SerializeField]
        private RectTransform viewport = null;
        [SerializeField]
        private List<Item> items = null;
        [SerializeField]
        private float spacing = 0.0f;
        [SerializeField]
        private float duration = 0.5f;
        [SerializeField]
        private int visibleItems = 5;
        [SerializeField]
        private DirectionType direction = DirectionType.TopToBottom;
        [SerializeField]
        private bool reverseScroll = false;
        [SerializeField]
        private bool setAsLastSibling = false;
        [SerializeField]
        private int selection = -1;
        //[SerializeField]
        public SelectorEvent OnValueChanged = new();

        private RectTransform rectTransform = null;

        protected bool started = false;
        private bool refreshItems = false;
        private bool updateVisuals = false;
        private bool immediate = false;
        private Vector2 itemSize = Vector2.one;
        private Vector2 viewOrg = Vector2.zero;
        private Vector2 dragPosition = default;

        private bool opened = false;
        private bool dragging = false;
        private float show = 0.0f;

        private float scrollPos = -1;
        private float scrollDelta = 0;

        private struct ItemInfo
        {
            public bool own;
            public RectTransform element;
            public CanvasGroup group;
            public Image image;
            public TMPro.TextMeshProUGUI text;
            public LazyText lazyText;
        }
        private readonly List<ItemInfo> itemInfos = new();
        private readonly Vector2[] dir = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        public int Selection
        {
            get { return selection; }
            set { Select(value, true); }
        }

        public int Count => items.Count;

        public int AddItem(Item item)
        {
            var index = items.Count;
            items.Add(item);
            refreshItems = true;
            return index;
        }
        public Item GetItem(int index)
        {
            if ((index < 0) || (index >= items.Count))
            {
                return default;
            }
            return items[index];
        }
        public void SetItem(int index, Item item)
        {
            if ((index < 0) || (index >= items.Count))
            {
                return;
            }
            items[index] = item;
            refreshItems = true;
        }
        public void RemoveItem(int index)
        {
            if ((index < 0) || (index >= items.Count))
            {
                return;
            }
            items.RemoveAt(index);
            refreshItems = true;
            if (selection >= index)
            {
                selection = index - 1;
            }
        }
        public void ClearItems()
        {
            items.Clear();
            refreshItems = true;
            selection = -1;
            scrollPos = -1;
        }
        public void RefreshShownValue()
        {
            refreshItems = true;
        }
        public Vector2 GetItemSize()
        {
            return itemSize;
        }

        private T GetChildComponent<T>(Transform transform) where T : Component
        {
            for (int i = 0, end = transform.childCount; i < end; i++)
            {
                var ch = transform.GetChild(i);
                if (ch == null)
                {
                    continue;
                }
                var cm = ch.GetComponent<T>();
                if (cm != null)
                {
                    return cm;
                }
            }
            return null;
        }
        private bool Vertical
        {
            get { return (direction == DirectionType.TopToBottom) || (direction == DirectionType.BottomToTop); }
        }
        private void RefreshItems()
        {
            itemSize = Vector2.one;
            if (template != null)
            {
                itemSize = template.rect.size;
            }
            if ((items == null) || (items.Count == 0))
            {
                ReleaseItems();
                return;
            }
            for (int i = 0, end = items.Count; i < end; i++)
            {
                var item = items[i];
                if (item.element != null)
                {
                    var s = item.element.rect.size;
                    itemSize = Vector2.Max(itemSize, s);
                }
            }
            itemSize += new Vector2(spacing, spacing);
            var dd = dir[(int)direction];
            {
                var s = rectTransform.rect.size;
                viewOrg = (s - itemSize) * 0.5f * dd;
            }
            var mk = dd * 0.5f;
            for (int i = 0, end = items.Count; i < end; i++)
            {
                var item = items[i];
                var ii = default(ItemInfo);
                if (i < itemInfos.Count)
                {
                    ii = itemInfos[i];
                    if (!ReferenceEquals(item.element, null))
                    {
                        if (!ReferenceEquals(item.element, ii.element))
                        {
                            if (ii.own)
                            {
                                ii.own = false;
                                DestroyX(ii.element);
                            }
                            ii.element = item.element;
                            ii.group = null;
                            ii.image = null;
                            ii.text = null;
                            ii.lazyText = null;
                        }
                    }
                    else
                    {
                        if (!ii.own)
                        {
                            ii.element = null;
                            ii.group = null;
                            ii.image = null;
                            ii.text = null;
                            ii.lazyText = null;
                        }
                    }
                }
                else
                {
                    ii.own = false;
                    ii.element = item.element;
                    ii.group = null;
                    ii.image = null;
                    ii.text = null;
                    ii.lazyText = null;
                    itemInfos.Add(ii);
                }
                if (ReferenceEquals(ii.element, null))
                {
                    if (template == null)
                    {
                        continue;
                    }
                    ii.element = Instantiate(template, viewport, false);
                    ii.element.gameObject.name = $"Item ({i})";
                    ii.element.gameObject.hideFlags = HideFlags.DontSave;
                    ii.element.gameObject.SetActive(true);
                    ii.own = true;
                }
                if (ii.element != null)
                {
                    var r = ii.element.rect;
                    ii.element.anchoredPosition = itemSize * dd * i - (r.size - itemSize) * mk;
                    {
                        ii.group = ii.element.GetComponent<CanvasGroup>();
                    }
                    {
                        ii.image = GetChildComponent<Image>(ii.element);
                        if (ii.image != null)
                        {
                            //ii.image.sprite = item.icon;
                            ii.image.overrideSprite = item.icon;
                        }
                    }
                    {
                        ii.text = GetChildComponent<TMPro.TextMeshProUGUI>(ii.element);
                        if (ii.text != null)
                        {
                            ii.text.text = item.text;
                        }
                        ii.lazyText = GetChildComponent<LazyText>(ii.element);
                        if (ii.lazyText != null)
                        {
                            ii.lazyText.Text = item.text;
                        }
                    }
                }
                itemInfos[i] = ii;
            }
            updateVisuals = true;
        }
        private void ReleaseItems()
        {
            foreach (var ii in itemInfos)
            {
                if (ii.own && (ii.element != null))
                {
                    DestroyX(ii.element.gameObject);
                }
            }
            itemInfos.Clear();
        }
        private void DestroyX(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(obj);
                return;
            }
#endif
            UnityEngine.Object.Destroy(obj);
        }
        private void UpdateVisuals()
        {
            var scrolling = false;
            var scpos = scrollPos;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (scpos < 0)
                {
                    scpos = 0;
                }
            }
#endif
            if (viewport != null)
            {
                var pos = viewport.anchoredPosition;
                var trg = pos;
                float l;
                if (Vertical)
                {
                    trg.y = -scpos * itemSize.y * ((direction == DirectionType.BottomToTop) ? +1 : -1) + viewOrg.y;
                    l = Mathf.Abs((trg.y - pos.y) / itemSize.y);
                }
                else
                {
                    trg.x = -scpos * itemSize.x * ((direction == DirectionType.LeftToRight) ? +1 : -1) + viewOrg.x;
                    l = Mathf.Abs((trg.x - pos.x) / itemSize.x);
                }
                if (immediate || (l < 0.01f))
                {
                    pos = trg;
                }
                else
                {
                    scrolling = l > 0.1f;
                    pos = Vector2.Lerp(trg, pos, Mathf.Pow(0.001f, Time.deltaTime));
                    updateVisuals = true;
                }
                viewport.anchoredPosition = pos;
            }
            var sh = (opened || scrolling) ? 1.0f : 0.0f;
            if (immediate || (duration <= 0) || (Mathf.Abs(show - sh) < 0.01f))
            {
                show = sh;
            }
            else
            {
                show = Mathf.Lerp(sh, show, Mathf.Pow(0.001f, Time.deltaTime / duration));
                updateVisuals = true;
            }
            var hvl = visibleItems * 0.5f;
            for (int i = 0, end = itemInfos.Count; i < end; i++)
            {
                var group = itemInfos[i].group;
                if (group != null)
                {
                    float a;
                    var d = Mathf.Abs(i - scpos) - 0.5f;
                    if (d <= 0)
                    {
                        a = 1.0f;
                    }
                    else if (d >= hvl)
                    {
                        a = 0.0f;
                    }
                    else
                    {
                        var t = 1.0f - d / hvl;
                        //a = Mathf.Pow(t, 0.5f) * show;
                        a = (0.5f - 0.5f * Mathf.Cos(t * Mathf.PI)) * show;
                    }
                    group.alpha = a;
                }
                var active = (i == selection) || (show != 0);
                var element = itemInfos[i].element;
                if ((element != null) && (element.gameObject.activeSelf != active))
                {
                    element.gameObject.SetActive(active);
                }
            }
            immediate = false;
        }
        protected void Select(int select, bool notify, bool force = false)
        {
            if ((select < 0) || (select >= items.Count))
            {
                select = -1;
            }
            var dirty = selection != select;
            if (dirty)
            {
                selection = select;
                if (selection >= 0)
                {
                    if (notify)
                    {
                        OnValueChanged?.Invoke(select);
                    }
                }
            }
            if (dirty || force)
            {
                scrollPos = selection;
                updateVisuals = true;
                immediate = true;
            }
        }

        public void Open()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            opened = true;
            updateVisuals = true;
            if (setAsLastSibling)
            {
                rectTransform.SetAsLastSibling();
            }
        }
        public void Close()
        {
            opened = false;
            Select(Mathf.RoundToInt(scrollPos), true, true);
            immediate = false;
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            //LazyDebug.Log("OnPointerDown");
            base.OnPointerDown(eventData);
#if true
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
#endif
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out dragPosition);
            Open();
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            //LazyDebug.Log("OnPointerUp");
            base.OnPointerUp(eventData);
#if true
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
#endif
            Close();
        }
        public float ScrollPos
        {
            get { return scrollPos; }
            set
            {
                if (scrollPos == value)
                {
                    return;
                }
                scrollPos = value;
                updateVisuals = true;
            }
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            //LazyDebug.Log("OnPointerExit");
            base.OnPointerExit(eventData);
        }
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            //LazyDebug.Log("OnBeginDrag");
            dragging = opened;
        }
        public virtual void OnDrag(PointerEventData eventData)
        {
            //LazyDebug.Log("OnDrag");
            if (!dragging)
            {
                return;
            }
            Vector2 p1;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out p1);
            var dd = p1 - dragPosition;
            dragPosition = p1;
            var dt = Vector2.Dot(dd / itemSize, dir[(int)direction]);
            if (reverseScroll)
            {
                dt = -dt;
            }
            var sp = scrollPos + dt;
            sp = Mathf.Clamp(sp, 0, items.Count - 1);
            scrollPos = sp;
            updateVisuals = true;
        }
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            //LazyDebug.Log("OnEndDrag");
            if (!dragging)
            {
                return;
            }
            Select(Mathf.RoundToInt(scrollPos), true, true);
            immediate = false;
        }
        public virtual void OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            var dt = Vector2.Dot(eventData.scrollDelta, dir[(int)direction]);
            if (Mathf.Abs(dt) >= 120)
            {
                dt /= 120.0f;
            }
            if (reverseScroll)
            {
                dt = -dt;
            }
            scrollDelta -= dt / 3;
            var di = (int)scrollDelta;
            if (di != 0)
            {
                show = 1.0f;
                scrollDelta -= di;
                var sel = selection + di;
                sel = Mathf.Clamp(sel, 0, items.Count - 1);
                Select(sel, true);
#if false
                immediate = false;
#endif
            }
        }
        public bool Reverse => reverseScroll;
        public void Up()
        {
            var sel = Mathf.Clamp(selection + (reverseScroll ? -1 : +1), 0, items.Count - 1);
            Select(sel, true);
        }
        public void Down()
        {
            var sel = Mathf.Clamp(selection + (reverseScroll ? +1 : -1), 0, items.Count - 1);
            Select(sel, true);
        }
        private void UpdateInput()
        {
            if (!IsActive() || !IsInteractable() || (currentSelectionState != SelectionState.Selected))
            {
                return;
            }
            if (LazyInputActions.PageUp?.WasPressedThisFrame() == true)
            {
                Up();
            }
            if (LazyInputActions.PageDown?.WasPressedThisFrame() == true)
            {
                Down();
            }
        }
        protected virtual void OnUpdate()
        {

        }
        private void UpdateState()
        {
            OnUpdate();
            UpdateInput();
            if (refreshItems)
            {
                refreshItems = false;
                RefreshItems();
            }
            if (updateVisuals)
            {
                updateVisuals = false;
                UpdateVisuals();
            }
        }
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if ((template != null) && Application.isPlaying)
#else
            if (template != null)
#endif
            {
                template.gameObject.SetActive(false);
            }
            rectTransform = transform as RectTransform;
        }
        private void StartUp()
        {
            Select(selection, false, true);
            refreshItems = true;
            updateVisuals = true;
            immediate = true;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                StartUp();
            }
        }
        protected override void Start()
        {
            base.Start();
            started = true;
            {
                StartUp();
            }
        }
        protected override void OnDisable()
        {
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            ReleaseItems();
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            Select(selection, false, true);
            RefreshItems();
            UpdateVisuals();
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += DelayedUpdate;
        }
#endif
    }
}
