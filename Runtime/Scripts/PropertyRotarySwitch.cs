using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// Bool,Int32,Enum型プロパティのロータリー(多ステート)スイッチ
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyRotarySwitch : RotarySwitch
    {
        [SerializeField]
        [LazyProperty(PropertyValueType.Boolean | PropertyValueType.Int32 | PropertyValueType.Enum)]
        private LazyProperty targetProperty = new();

        private bool started = false;
        private bool active = false;
        private int value = -1;

        public override bool IsInteractable()
        {
            return active && base.IsInteractable();
        }
        private void SetValue(int value)
        {
            if (!active)
            {
                return;
            }
            if (this.value == value)
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
            this.value = -1;
            if ((value < 0) || (value >= Count))
            {
                return;
            }
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.Boolean:
                    {
                        if (value >= 2)
                        {
                            return;
                        }
                        targetProperty.SetValue(value != 0);
                    }
                    break;
                case PropertyValueType.Int32:
                    {
                        targetProperty.SetValue(value);
                    }
                    break;
                case PropertyValueType.Enum:
                    {
                        targetProperty.SetEnumValueIndex(value);
                    }
                    break;
                default:
                    break;
            }
        }
        private void UpdateValue(bool notify, bool force)
        {
            active = false;
            if (!IsActive())
            {
                return;
            }
            if (!targetProperty.IsValid())
            {
                return;
            }
            int select;
            var range = Range;
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.Boolean:
                    {
                        if (!targetProperty.TryGetValue(out bool v))
                        {
                            return;
                        }
                        select = v ? 1 : 0;
                        {
                            range = new LazyRange<int>(0, 1);
                        }
                    }
                    break;
                case PropertyValueType.Int32:
                    {
                        if (!targetProperty.TryGetValue(out int v))
                        {
                            return;
                        }
                        select = v;
                        if (targetProperty.TryGetRange(out LazyRange<int> r0) && r0.Valid())
                        {
                            range = r0;
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
                        select = idx;
                        {
                            range = new LazyRange<int>(0, targetProperty.GetEnumValueCount() - 1);
                        }
                    }
                    break;
                default:
                    return;
            }
            active = true;
            Select(select, range.MinValue, range.MaxValue, notify, force);
        }
        private void UpdateState(bool notify, bool force)
        {
            var ac = active;
            UpdateValue(notify, force);
            if (ac != active)
            {
                OnDidApplyAnimationProperties();
            }
        }
        private void SetupProperty(bool warn)
        {
            value = -1;
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
                case PropertyValueType.Boolean:
                case PropertyValueType.Int32:
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
        private void RotarySwitchOnValueChanged(int value)
        {
            SetValue(value);
        }
        private void UpdateState()
        {
            UpdateState(true, false);
        }

        protected override void Awake()
        {
            base.Awake();
            SetupProperty(true);
            OnValueChanged.AddListener(RotarySwitchOnValueChanged);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
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
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            UpdateState(false, false);
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
