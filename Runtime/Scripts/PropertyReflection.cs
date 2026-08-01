using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// プロパティをコピー
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyReflection : MonoBehaviour
    {
        [SerializeField]
        [LazyProperty(PropertyValueType.Everything, false)]
        private LazyProperty sourceProperty = new();

        [SerializeField]
        [LazyProperty(PropertyValueType.Everything, false)]
        private LazyProperty targetProperty = new();

        private void UpdateProperty()
        {
            if (!sourceProperty.IsValid() || !targetProperty.IsValid())
            {
                return;
            }
            var sourceValue = sourceProperty.GetValue();
            if (LazyProperty.TryConvert(sourceValue, targetProperty.GetValueType(), out object targetValue))
            {
                targetProperty.SetValue(targetValue);
            }
        }

        private void SetupProperty(bool warn)
        {
            if (sourceProperty == null)
            {
                sourceProperty = new();
            }
            if (targetProperty == null)
            {
                targetProperty = new();
            }
            sourceProperty.Invalidate();
            targetProperty.Invalidate();
            if (sourceProperty.IsEmpty() || targetProperty.IsEmpty())
            {
                return;
            }
            if (!sourceProperty.IsValid())
            {
                if (warn)
                {
                    LazyDebug.LogWarning($"{transform.GetFullPath()}: sourceProperty is not valid. : {sourceProperty}");
                }
            }
            if (!targetProperty.IsValid())
            {
                if (warn)
                {
                    LazyDebug.LogWarning($"{transform.GetFullPath()}: destinationProperty is not valid. : {targetProperty}");
                }
            }
        }

        private void Awake()
        {
            SetupProperty(true);
        }
        private void OnEnable()
        {
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateProperty);
        }
        private void OnDisable()
        {
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateProperty);
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            UpdateProperty();
        }
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
