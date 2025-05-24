using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// BoolまたはEnum型プロパティを選択
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertySelector : Selector
    {
        [SerializeField]
        [LazyProperty(PropertyValueType.Boolean | PropertyValueType.Int32 | PropertyValueType.Enum)]
        private LazyProperty targetProperty = new();

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
        private void UpdateValue(bool notify)
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
                    }
                    break;
                case PropertyValueType.Int32:
                    {
                        if (!targetProperty.TryGetValue(out int v))
                        {
                            return;
                        }
                        select = v;
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
                    }
                    break;
                default:
                    return;
            }
            active = true;
            Select(select, notify);
        }
        private void UpdateState(bool notify)
        {
            var ac = active;
            UpdateValue(notify);
            if (ac != active)
            {
                OnDidApplyAnimationProperties();
            }
        }
        private void SetupProperty(bool warn)
        {
            active = false;
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
            Array enumValues = null;
            var vt = targetProperty.GetValueType();
            switch (vt)
            {
                case PropertyValueType.Boolean:
                    enumValues = new bool[] { false, true };
                    break;
                case PropertyValueType.Int32:
                    break;
                case PropertyValueType.Enum:
                    enumValues = Enum.GetValues(targetProperty.GetPropertyType());
                    if (enumValues == null)
                    {
                        return;
                    }
                    break;
                default:
                    if (warn)
                    {
                        LazyDebug.LogWarning($"{transform.GetFullPath()}: targetProperty is not supported type : {targetProperty.GetPropertyType().FullName}");
                    }
                    return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
            }
            else
#endif
            if ((enumValues != null) && (Count <= 0))
            {
                for (int i = 0; i < enumValues.Length; i++)
                {
                    var item = default(Item);
                    item.text = enumValues.GetValue(i).ToString();
                    AddItem(item);
                }
            }
        }
        private void SelectorOnValueChanged(int value)
        {
            SetValue(value);
        }
        protected override void OnUpdate()
        {
            UpdateState(true);
        }
        protected override void Awake()
        {
            base.Awake();
            SetupProperty(true);
            OnValueChanged.AddListener(SelectorOnValueChanged);
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
