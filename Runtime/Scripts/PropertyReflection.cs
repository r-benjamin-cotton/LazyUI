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
        [Serializable]
        private struct Reflection
        {
            [LazyProperty(PropertyValueType.Everything, false)]
            public LazyProperty sourceProperty;

            [SerializeField]
            [LazyProperty(PropertyValueType.Everything, false)]
            public LazyProperty targetProperty;
        }
        [SerializeField]
        private Reflection[] reflections = null;

        private void UpdateProperty()
        {
            if (reflections == null)
            {
                return;
            }
            foreach (var r in reflections)
            {
                if ((r.sourceProperty == null) || (r.targetProperty == null))
                {
                    continue;
                }
                if (!r.sourceProperty.IsValid() || !r.targetProperty.IsValid())
                {
                    return;
                }
                var sourceValue = r.sourceProperty.GetValue();
                if (LazyProperty.TryConvert(sourceValue, r.targetProperty.GetValueType(), out object targetValue))
                {
                    r.targetProperty.SetValue(targetValue);
                }
            }
        }

        private void SetupProperty(bool warn)
        {
            if (reflections == null)
            {
                return;
            }
            foreach (var r in reflections)
            {
                if ((r.sourceProperty == null) || (r.targetProperty == null))
                {
                    continue;
                }
                r.sourceProperty.Invalidate();
                r.targetProperty.Invalidate();
                if (r.sourceProperty.IsEmpty() || r.targetProperty.IsEmpty())
                {
                    return;
                }
                if (!r.sourceProperty.IsValid())
                {
                    if (warn)
                    {
                        LazyDebug.LogWarning($"{transform.GetFullPath()}: sourceProperty is not valid. : {r.sourceProperty} => {r.targetProperty}");
                    }
                }
                if (!r.targetProperty.IsValid())
                {
                    if (warn)
                    {
                        LazyDebug.LogWarning($"{transform.GetFullPath()}: targetProperty is not valid. : {r.sourceProperty} => {r.targetProperty}");
                    }
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
