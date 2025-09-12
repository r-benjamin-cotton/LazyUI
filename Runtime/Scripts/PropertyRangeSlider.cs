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
    /// Range型プロパティを操作するスライダー
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyRangeSlider : Selectable, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        [Serializable]
        public class PropertyRangeSliderEvent : UnityEvent<Range<float>> { }

        public enum DirectionType
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        [SerializeField]
        private RectTransform fillRect = null;

        [SerializeField]
        private DirectionType direction = DirectionType.LeftToRight;

        [SerializeField]
        private float minValue = 0;

        [SerializeField]
        private float maxValue = 0;

        [SerializeField]
        private float minWidth = 0;

        [SerializeField]
        private float stepValue = 0.1f;

        [SerializeField]
        private float stepRatio = 0.1f;

        [SerializeField]
        private bool wholeNumbers = false;

        [SerializeField]
        private Range<float> value = new(0, 1);

        [SerializeField]
        private TMPro.TextMeshProUGUI textMin = null;

        [SerializeField]
        private TMPro.TextMeshProUGUI textMax = null;

        [SerializeField]
        private LazyText lazyTextMin = null;

        [SerializeField]
        private LazyText lazyTextMax = null;

        [SerializeField]
        private string textFormat = "";

        [SerializeField]
        [LazyProperty(PropertyValueType.IntRange | PropertyValueType.FloatRange, false)]
        private LazyProperty targetProperty = new();

        [SerializeField]
        private PropertyRangeSliderEvent onValueChanged = new();

        private Range<float> range = default;

        private bool started = false;
        private bool active = false;

        private RectTransform fillParentRect = null;
        private DirectionType currentDirection = DirectionType.LeftToRight;

        private bool dragging = false;
        private Vector2 dragPoint = default;
        private float dragValue = 0.0f;

        public Range<float> Value
        {
            get
            {
                return value;
            }
            set
            {
                SetValue(value);
                UpdateState(true, false);
            }
        }
        public Range<float> NormalizedValue
        {
            get
            {
                var d = range.MaxValue - range.MinValue;
                if (d <= 0)
                {
                    return new Range<float>(0, 1);
                }
                var id = 1.0f / d;
                var min = Mathf.Clamp01((value.MinValue - range.MinValue) * id);
                var max = Mathf.Clamp01((value.MaxValue - range.MinValue) * id);
                return new Range<float>(min, max);
            }
            set
            {
                var d = Mathf.Max(0, range.MaxValue - range.MinValue);
                var min = range.MinValue + d * Mathf.Clamp01(value.MinValue);
                var max = range.MinValue + d * Mathf.Clamp01(value.MaxValue);
                SetValue(new Range<float>(min, max));
                UpdateState(true, false);
            }
        }
        public float StepValue
        {
            get
            {
                var step = Mathf.Max(Mathf.Max(0, range.MaxValue - range.MinValue) * stepRatio, stepValue);
                if (wholeNumbers)
                {
                    step = Mathf.Max(1, Mathf.Floor(step));
                }
                return step;
            }
        }
        public DirectionType Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                UpdateVisuals();
            }
        }
        public override bool IsInteractable()
        {
            return active && base.IsInteractable();
        }
        public PropertyRangeSliderEvent OnValueChanged => onValueChanged;

        private Range<float> ValidateValue(Range<float> value)
        {
            var rd = range.MaxValue - range.MinValue;
            if (rd < 0)
            {
                return new Range<float>(0, 0);
            }
            var nrw = Mathf.Max(Mathf.Min(rd, minWidth), 0);
            var valueMin = value.MinValue;
            var valueMax = value.MaxValue;
            if (wholeNumbers)
            {
                nrw = Mathf.Floor(nrw);
                valueMin = Mathf.Round(valueMin);
                valueMax = Mathf.Round(valueMax);
            }
            valueMin = range.Clamp(valueMin);
            valueMax = range.Clamp(valueMax);
            if (valueMax - valueMin < nrw)
            {
                if (valueMin != this.value.MinValue)
                {
                    valueMax = valueMin + nrw;
                    if (valueMax > range.MaxValue)
                    {
                        var d = valueMax - range.MaxValue;
                        valueMin -= d;
                        valueMax -= d;
                    }
                }
                else
                {
                    valueMin = valueMax - nrw;
                    if (valueMin < range.MinValue)
                    {
                        var d = range.MinValue - valueMin;
                        valueMin += d;
                        valueMax += d;
                    }
                }
            }
            return new Range<float>(valueMin, valueMax);
        }
        private void SetValue(Range<float> value)
        {
            value = ValidateValue(value);
            if (this.value == value)
            {
                return;
            }
            if (!active)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                this.value = value;
                return;
            }
#endif
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.IntRange:
                    {
                        targetProperty.SetValue(new Range<int>((int)value.MinValue, (int)value.MaxValue));
                    }
                    break;
                case PropertyValueType.FloatRange:
                    {
                        targetProperty.SetValue(value);
                    }
                    break;
                default:
                    break;
            }
        }
        private void UpdateValue()
        {
            active = false;
            range = new Range<float>(minValue, maxValue);
            if (wholeNumbers)
            {
                range = new Range<float>(Mathf.Ceil(range.MinValue), Mathf.Floor(range.MaxValue));
            }
            if (!IsActive())
            {
                return;
            }
            if (!targetProperty.IsValid())
            {
                return;
            }
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.IntRange:
                    {
                        if (!targetProperty.TryGetValue(out Range<int> vv))
                        {
                            return;
                        }
                        value = new Range<float>(vv.MinValue, vv.MaxValue);
                        if (targetProperty.TryGetRange(out Range<int> r0) && r0.Valid())
                        {
                            range = new Range<float>(r0.MinValue, r0.MaxValue);
                        }
                    }
                    break;
                case PropertyValueType.FloatRange:
                    {
                        if (!targetProperty.TryGetValue(out Range<float> vv))
                        {
                            return;
                        }
                        value = vv;
                        if (targetProperty.TryGetRange(out Range<float> r0) && r0.Valid())
                        {
                            range = r0;
                        }
                        if (wholeNumbers)
                        {
                            range = new Range<float>(Mathf.Ceil(range.MinValue), Mathf.Floor(range.MaxValue));
                        }
                    }
                    break;
                default:
                    return;
            }
            active = true;
            value = ValidateValue(value);
        }
        private void UpdateState(bool notify, bool force)
        {
            var ac = active;
            var vl = value;
            var rg = range;
            UpdateValue();
            if (force || (vl != value) || (rg != range) || (ac != active))
            {
                UpdateVisuals();
            }
            if (notify && (vl != value))
            {
                OnValueChanged?.Invoke(value);
            }
        }
        private bool Reverse
        {
            get
            {
                return (currentDirection == DirectionType.RightToLeft) || (currentDirection == DirectionType.TopToBottom);
            }
        }
        private int Axis
        {
            get
            {
                return ((currentDirection == DirectionType.BottomToTop) || (currentDirection == DirectionType.TopToBottom)) ? 1 : 0;
            }
        }

        public void StepUp()
        {
            var step = StepValue;
            Value = new Range<float>(Value.MinValue, value.MaxValue + step);
        }
        public void StepDown()
        {
            var step = StepValue;
            Value = new Range<float>(Value.MinValue - step, value.MaxValue);
        }
        private void AddValue(float step)
        {
            var v = Value;
            var min = v.MinValue;
            var max = v.MaxValue;
            min += step;
            max += step;
            Value = new Range<float>(min, max);
        }
        private void UpdateInput()
        {
            if (!IsActive() || !IsInteractable() || (currentSelectionState != SelectionState.Selected))
            {
                return;
            }
            var step = StepValue;
            if (InputActions.Up.WasPressedThisFrame())
            {
                AddValue(Reverse ? -step : +step);
            }
            if (InputActions.Down.WasPressedThisFrame())
            {
                AddValue(Reverse ? +step : -step);
            }
        }
        private void UpdateVisuals()
        {
            try
            {
                var vt = targetProperty.GetValueType();
                switch (vt)
                {
                    case PropertyValueType.IntRange:
                        {
                            var intValue = new Range<int>((int)value.MinValue, (int)value.MaxValue);
                            {
                                if (textMin != null)
                                {
                                    textMin.text = intValue.MinValue.ToString(textFormat);
                                }
                                if (textMax != null)
                                {
                                    textMax.text = intValue.MaxValue.ToString(textFormat);
                                }
                                if (lazyTextMin != null)
                                {
                                    lazyTextMin.Text = intValue.MinValue.ToString(textFormat);
                                }
                                if (lazyTextMax != null)
                                {
                                    lazyTextMax.Text = intValue.MaxValue.ToString(textFormat);
                                }
                            }
                        }
                        break;
                    case PropertyValueType.FloatRange:
                        {
                            if (textMin != null)
                            {
                                textMin.text = value.MinValue.ToString(textFormat);
                            }
                            if (textMax != null)
                            {
                                textMax.text = value.MaxValue.ToString(textFormat);
                            }
                            if (lazyTextMin != null)
                            {
                                lazyTextMin.Text = value.MinValue.ToString(textFormat);
                            }
                            if (lazyTextMax != null)
                            {
                                lazyTextMax.Text = value.MaxValue.ToString(textFormat);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ec)
            {
                LazyDebug.LogWarning($"{name}.TextFormat={textFormat}: {ec.Message}");
            }
            if (currentDirection != direction)
            {
                var oldAxis = Axis;
                var oldReverse = Reverse;
                currentDirection = direction;
                var newAxis = Axis;
                var newReverse = Reverse;
                if (oldAxis != newAxis)
                {
                    RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);
                }
                if (oldReverse != newReverse)
                {
                    RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, newAxis, true, true);
                }
            }
            if (fillRect != null)
            {
                var anchorMin = Vector2.zero;
                var anchorMax = Vector2.one;
                var value = NormalizedValue;
                var axis = Axis;
                if (Reverse)
                {
                    anchorMin[axis] = 1.0f - value.MaxValue;
                    anchorMax[axis] = 1.0f - value.MinValue;
                }
                else
                {
                    anchorMin[axis] = value.MinValue;
                    anchorMax[axis] = value.MaxValue;
                }
                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }
#if true
            if (this != null)
            {
                OnDidApplyAnimationProperties();
            }
#endif
        }
        private bool MayDrag(PointerEventData eventData)
        {
            return (eventData.button == PointerEventData.InputButton.Left) && IsActive() && IsInteractable();
        }
        private void UpdateDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(fillParentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                var axis = Axis;
                var rect = fillParentRect.rect;
                var size = rect.size[axis];
                if (size > 0)
                {
                    var dx = (localPoint - dragPoint) / size;
                    var d0 = dx[axis];
                    if (Reverse)
                    {
                        d0 = 1 - d0;
                    }
                    var d1 = dx[axis ^ 1];
                    var vv = PropertySlider.CalcDragValue(d0, d1, dragValue);
                    if (dragValue != vv)
                    {
                        dragValue = vv;
                        dragPoint[axis] = localPoint[axis];
                    }
                    var nv = NormalizedValue;
                    var v0 = vv;
                    var v1 = vv + (nv.MaxValue - nv.MinValue);
                    if (v0 < 0)
                    {
                        v1 -= v0;
                        v0 = 0;
                    }
                    if (v1 > 1)
                    {
                        v0 -= v1 - 1;
                        v1 = 1;
                    }
                    NormalizedValue = new Range<float>(v0, v1);
                }
            }
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            dragging = false;
            if (!MayDrag(eventData))
            {
                return;
            }
            if (fillParentRect != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(fillRect, eventData.position, eventData.pressEventCamera))
                {
                    dragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(fillParentRect, eventData.position, eventData.pressEventCamera, out dragPoint);
                    return;
                }
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            UpdateDrag(eventData);
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragValue = NormalizedValue.MinValue;
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
        private void SetupCache()
        {
            fillParentRect = (fillRect == null) ? null : (fillRect.parent as RectTransform);
        }
        private void SetupProperty(bool warn)
        {
            if (targetProperty == null)
            {
                targetProperty = new();
            }
            targetProperty.Invalidate();
            if (targetProperty.IsEmpty())
            {
                return;
            }
            if (!targetProperty.IsValid())
            {
                if (warn)
                {
                    LazyDebug.LogWarning($"{transform.GetFullPath()}: targetProperty is not valid. : {targetProperty}");
                }
                return;
            }
        }
        private void UpdateState()
        {
            UpdateInput();
            UpdateState(true, false);
        }
        protected override void Awake()
        {
            base.Awake();
            currentDirection = direction;
            SetupProperty(true);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            InputActions.Activate();
            SetupCache();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                UpdateState(true, true);
            }
        }
        protected override void Start()
        {
            base.Start();
            started = true;
            {
                UpdateState(true, true);
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            InputActions.Deactivate();
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            SetupCache();
            UpdateState(false, true);
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
