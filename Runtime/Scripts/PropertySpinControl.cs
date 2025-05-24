using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LazyUI
{
    [RequireComponent(typeof(RectTransform))]
    public class PropertySpinControl : SpinControl
    {
        [Serializable]
        public class PropertySpinControlEvent : UnityEvent<float> { }

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
        [LazyProperty(PropertyValueType.Int32 | PropertyValueType.Single | PropertyValueType.Boolean | PropertyValueType.Enum, false)]
        private LazyProperty targetProperty = new();

        [SerializeField]
        private PropertySpinControlEvent onValueChanged = new();

        private Range<float> range = default;
        private float step = 0;

        private bool active = false;

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                SetValue(value);
                UpdateState(true);
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
                UpdateState(true);
            }
        }
        public float Step
        {
            get
            {
                return step;
            }
        }

        public override bool IsInteractable()
        {
            return active && base.IsInteractable();
        }
        public PropertySpinControlEvent OnValueChanged => onValueChanged;

        private float ValidateValue(float value)
        {
            value = range.Clamp(value);
            if (wholeNumbers)
            {
                value = Mathf.Round(value);
            }
            return value;
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
        private void UpdateStep()
        {
            step = Mathf.Max(Mathf.Max(0, range.MaxValue - range.MinValue) * stepRatio, stepValue);
            if (wholeNumbers)
            {
                step = Mathf.Max(1, Mathf.Floor(step));
            }
        }
        private void UpdateValue()
        {
            {
                range = new Range<float>(minValue, maxValue);
                if (wholeNumbers)
                {
                    range = new Range<float>(Mathf.Round(range.MinValue), Mathf.Round(range.MaxValue));
                }
                UpdateStep();
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
                            UpdateStep();
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
                            if (wholeNumbers)
                            {
                                range = new Range<float>(Mathf.Ceil(range.MinValue), Mathf.Floor(range.MaxValue));
                            }
                            UpdateStep();
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
                            step = 1;
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
                            step = 1;
                        }
                    }
                    break;
                default:
                    return;
            }
            active = true;
            value = ValidateValue(value);
        }
        private void UpdateState(bool notify)
        {
            var ac = active;
            var vl = value;
            UpdateValue();
#if true
            if (ac != active)
            {
                OnDidApplyAnimationProperties();
            }
#endif
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
        protected override void OnSpinUp()
        {
            Value += Reverse ? -step : +step;
        }
        protected override void OnSpinDown()
        {
            Value += Reverse ? +step : -step;
        }
        protected override void OnBeginDrag(PointerEventData eventData)
        {
        }
        protected override void OnDrag(PointerEventData eventData)
        {
        }
        protected override void OnEndDrag(PointerEventData eventData)
        {
        }
        protected override void OnCoroutine()
        {
            UpdateState(true);
        }
        protected override void Awake()
        {
            base.Awake();
            SetupProperty(true);
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            UpdateState(false);
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
