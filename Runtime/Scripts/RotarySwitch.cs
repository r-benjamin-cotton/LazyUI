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
    /// ロータリー(順送り多ステート)スイッチ
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RotarySwitch : Selectable, IScrollHandler, ISubmitHandler
    {
        [Serializable]
        public class RotarySwitchEvent : UnityEvent<int> { }

        [SerializeField]
        private bool reverseScroll = false;

        [SerializeField]
        private int minValue = 0;

        [SerializeField]
        private int maxValue = 1;

        [SerializeField]
        private Image coverGraphic = null;

        [SerializeField]
        private List<Sprite> sprites = new();

        [SerializeField]
        private int selection = 0;

        //[SerializeField]
        public RotarySwitchEvent OnValueChanged = new();

        private float scrollDelta = 0;

        public LazyRange<int> Range
        {
            get
            {
                return new LazyRange<int>(minValue, Mathf.Max(minValue, maxValue));
            }
            set
            {
                var vmin = value.MinValue;
                var vmax = Mathf.Max(value.MinValue, value.MaxValue);
                if ((vmin == minValue) && (vmax == maxValue))
                {
                    return;
                }
                minValue = vmin;
                maxValue = vmax;
                Select(selection, true, true);
            }
        }
        public int MinValue
        {
            get { return minValue; }
            set
            {
                if (value == minValue)
                {
                    return;
                }
                minValue = value;
                Select(selection, true, true);
            }
        }
        public int MaxValue
        {
            get { return maxValue; }
            set
            {
                value = Mathf.Max(minValue, value);
                if (value == maxValue)
                {
                    return;
                }
                maxValue = value;
                Select(selection, true, true);
            }
        }
        public int Value
        {
            get
            {
                return selection + minValue;
            }
            set
            {
                Selection = value - minValue;
            }
        }
        public int Count
        {
            get
            {
                return Mathf.Max(0, maxValue - minValue) + 1;
            }
        }
        public int Selection
        {
            get { return selection; }
            set { Select(value, true, false); }
        }
        public void SetSprite(int index, Sprite sprite)
        {
            if ((index < 0) || (index >= sprites.Count))
            {
                return;
            }
            sprites[index] = sprite;
            if (index == selection)
            {
                UpdateVisuals();
            }
        }
        public Sprite GetSprite(int index)
        {
            if ((index < 0) || (index >= sprites.Count))
            {
                return null;
            }
            return sprites[index];
        }
        private void UpdateVisuals()
        {
            var target = (coverGraphic != null) ? coverGraphic : image;
            if (target != null)
            {
                Sprite sprite = null;
                if ((sprites != null) && (selection < sprites.Count))
                {
                    sprite = sprites[selection];
                }
                target.overrideSprite = sprite;
            }
        }
        private int Repeat(int value, int length)
        {
            value %= length;
            if (value < 0)
            {
                value += length;
            }
            return value;
        }
        protected void Select(int select, bool notify, bool force)
        {
            Select(select, minValue, maxValue, notify, force);
        }
        protected void Select(int select, int minValue, int maxValue, bool notify, bool force)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            while (sprites.Count < Count)
            {
                sprites.Add(null);
            }
            select = Repeat(select, Count);
            var dirty = selection != select;
            if (force || dirty)
            {
                selection = select;
                UpdateVisuals();
            }
            if (dirty && notify)
            {
                OnValueChanged?.Invoke(selection + minValue);
            }
        }
        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            Submit();
        }
        public virtual void Submit()
        {
            if (!IsInteractable())
            {
                return;
            }
            Select(Selection + 1, true, false);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    Select(selection + 1, true, false);
                    break;
                case PointerEventData.InputButton.Right:
                    Select(selection - 1, true, false);
                    break;
                default:
                case PointerEventData.InputButton.Middle:
                    break;
            }
        }
        public virtual void OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            var dt = eventData.scrollDelta.y;
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
                scrollDelta -= di;
                Select(selection + di, true, false);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Select(selection, false, true);
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            Select(selection, false, true);
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += DelayedUpdate;
        }
#endif
    }
}
