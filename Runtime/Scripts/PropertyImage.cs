using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LazyUI
{
    /// <summary>
    /// Bool,Int32,Enum型プロパティをインデックスにimageへスプライトをセット
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyImage : MonoBehaviour
    {
        [SerializeField]
        private Image image= null;

        [SerializeField]
        private List<Sprite> sprites = new();

        [SerializeField]
        [LazyProperty(PropertyValueType.Boolean | PropertyValueType.Int32 | PropertyValueType.Enum)]
        private LazyProperty targetProperty = new();

        private bool started = false;

        private void UpdateVisuals()
        {
            if (!targetProperty.IsValid())
            {
                if ((image != null) && (sprites != null))
                {
                    image.overrideSprite = null;
                }
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
            if ((image != null) && (sprites != null))
            {
                image.overrideSprite = sprites[Mathf.Clamp(select, 0, sprites.Count)];
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
        private void Awake()
        {
            SetupProperty(true);
        }
        private void OnEnable()
        {
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateVisuals);
            if (started)
            {
                UpdateVisuals();
            }
        }
        private void Start()
        {
            started = true;
            {
                UpdateVisuals();
            }
        }
        private void OnDisable()
        {
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateVisuals);
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            UpdateVisuals();
        }
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
