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
    /// floatもしくはintのプロパティを操作するスライダー
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertySlider : Selectable, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        [Serializable]
        public class PropertySliderEvent : UnityEvent<float> { }

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
        private RectTransform handleRect = null;

        [SerializeField]
        private DirectionType direction = DirectionType.LeftToRight;

        [SerializeField]
        private float minValue = 0;

        [SerializeField]
        private float maxValue = 0;

        [SerializeField]
        private float stepValue = 0.1f;

        [SerializeField]
        private float stepRatio = 0.1f;

        [SerializeField]
        private bool wholeNumbers = false;

        [SerializeField]
        private float value = 0;

        [SerializeField]
        private TMPro.TextMeshProUGUI view = null;

        [SerializeField]
        private LazyText lazyView = null;

        [SerializeField]
        [LazyProperty(PropertyValueType.Int32 | PropertyValueType.Single | PropertyValueType.Boolean | PropertyValueType.Enum, false)]
        private LazyProperty targetProperty = new();

        [SerializeField]
        private PropertySliderEvent onValueChanged = new();

        private DirectionType currentDirection = DirectionType.LeftToRight;
        private Range<float> range = default;

        private bool started = false;
        private bool active = false;
        private RectTransform handleParentRect = null;

        private bool dragging = false;
        private Vector2 dragPoint = default;
        private float dragValue = 0.0f;

        public float Value
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
        public float NormalizedValue
        {
            get
            {
                var d = range.MaxValue - range.MinValue;
                if (d <= 0)
                {
                    return 0;
                }
                return Mathf.Clamp01((value - range.MinValue) / d);
            }
            set
            {
                var d = Mathf.Max(0, range.MaxValue - range.MinValue);
                SetValue(range.MinValue + d * value);
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
                UpdateDirection();
                UpdateVisuals();
            }
        }
        public override bool IsInteractable()
        {
            return active && base.IsInteractable();
        }
        public PropertySliderEvent OnValueChanged => onValueChanged;

        private float ValidateValue(float value)
        {
            if (wholeNumbers)
            {
                value = Mathf.Round(value);
            }
            return range.Clamp(value);
        }
        private void SetValue(float value)
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
                case PropertyValueType.Int32:
                    {
                        targetProperty.SetValue((int)value);
                    }
                    break;
                case PropertyValueType.Single:
                    {
                        targetProperty.SetValue(value);
                    }
                    break;
                case PropertyValueType.Boolean:
                    {
                        targetProperty.SetValue(value >= 0.5f);
                    }
                    break;
                case PropertyValueType.Enum:
                    {
                        targetProperty.SetEnumValueIndex((int)value);
                    }
                    break;
                default:
                    break;
            }
        }
        private void UpdateValue()
        {
            range = new Range<float>(minValue, maxValue);
            if (wholeNumbers)
            {
                range = new Range<float>(Mathf.Ceil(range.MinValue), Mathf.Floor(range.MaxValue));
            }
            active = false;
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
                case PropertyValueType.Int32:
                    {
                        if (!targetProperty.TryGetValue(out int vv))
                        {
                            return;
                        }
                        value = vv;
                        if (targetProperty.TryGetRange(out Range<int> r0) && r0.Valid())
                        {
                            range = new Range<float>(r0.MinValue, r0.MaxValue);
                        }
                    }
                    break;
                case PropertyValueType.Single:
                    {
                        if (!targetProperty.TryGetValue(out float vv))
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
                case PropertyValueType.Boolean:
                    {
                        if (!targetProperty.TryGetValue(out bool vv))
                        {
                            return;
                        }
                        value = vv ? 1 : 0;
                        {
                            range = new Range<float>(0, 1);
                        }
                    }
                    break;
                case PropertyValueType.Enum:
                    {
                        var idx = targetProperty.GetEnumValueIndex();
                        if (idx < 0)
                        {
                            return;
                        }
                        value = idx;
                        {
                            range = new Range<float>(0, targetProperty.GetEnumValueCount() - 1);
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
            if (force || (ac != active) || (vl != value) || (rg != range))
            {
                UpdateVisuals();
            }
            if (notify && (vl != value))
            {
                OnValueChanged?.Invoke(value);
            }
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
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.Int32:
                case PropertyValueType.Single:
                case PropertyValueType.Boolean:
                case PropertyValueType.Enum:
                    break;
                default:
                    if (warn)
                    {
                        LazyDebug.LogWarning($"{transform.GetFullPath()}: targetProperty is not supported type : {targetProperty.GetPropertyType().FullName}");
                    }
                    return;
            }
        }
        public bool Reverse
        {
            get
            {
                return (currentDirection == DirectionType.RightToLeft) || (currentDirection == DirectionType.TopToBottom);
            }
        }
        public int Axis
        {
            get
            {
                return ((currentDirection == DirectionType.BottomToTop) || (currentDirection == DirectionType.TopToBottom)) ? 1 : 0;
            }
        }

        public void StepUp()
        {
            var step = StepValue;
            Value += Reverse ? -step : +step;
        }
        public void StepDown()
        {
            var step = StepValue;
            Value += Reverse ? +step : -step;
        }
        private void UpdateInput()
        {
            if (!IsActive() || !IsInteractable() || (currentSelectionState != SelectionState.Selected))
            {
                return;
            }
            if (InputActions.Up.WasPressedThisFrame())
            {
                StepUp();
            }
            if (InputActions.Down.WasPressedThisFrame())
            {
                StepDown();
            }
        }
        private void UpdateDirection()
        {
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
        }
        private void UpdateVisuals()
        {
            var val = targetProperty.GetValue();
            if (view != null)
            {
                view.text = val.ToString();
            }
            if (lazyView != null)
            {
                lazyView.Text = val.ToString();
            }
            if (fillRect != null)
            {
                var anchorMin = Vector2.zero;
                var anchorMax = Vector2.one;
                var value = NormalizedValue;
                var axis = Axis;
                if (Reverse)
                {
                    anchorMin[axis] = 1 - value;
                }
                else
                {
                    anchorMax[axis] = value;
                }
                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }

            if (handleRect != null)
            {
                var anchorMin = handleRect.anchorMin;
                var anchorMax = handleRect.anchorMax;
                var value = NormalizedValue;
                var minMax = Reverse ? (1 - value) : value;
                var axis = Axis;
                anchorMin[axis] = minMax;
                anchorMax[axis] = minMax;
                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
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

        internal static float CalcDragValue(float delta, float dist, float dragValue)
        {
            var value = dragValue + delta / (1.0f + Mathf.Pow(Mathf.Abs(dist), 2.0f) * 10.0f);
            return Mathf.Clamp01(value);
        }
        private void UpdateDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(handleParentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                var axis = Axis;
                var rect = handleParentRect.rect;
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
                    var vv = CalcDragValue(d0, d1, dragValue);
#if true
                    if (dragValue != vv)
                    {
                        dragValue = vv;
                        dragPoint[axis] = localPoint[axis];
                    }
#endif
                    NormalizedValue = vv;
                }
            }
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            //LazyDebug.Log("OnPointerDown");
            base.OnPointerDown(eventData);
            dragging = false;
            if (!MayDrag(eventData))
            {
                return;
            }
            if (handleParentRect != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(handleRect, eventData.position, eventData.pressEventCamera))
                {
                    dragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(handleParentRect, eventData.position, eventData.pressEventCamera, out dragPoint);
                }
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            dragValue = NormalizedValue;
            //LazyDebug.Log("OnBeginDrag");
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            UpdateDrag(eventData);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
        private void SetupCache()
        {
            handleParentRect = (handleRect == null) ? null : (handleRect.parent as RectTransform);
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
            UpdateDirection();
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
