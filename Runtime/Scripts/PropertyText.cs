using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// 指定プロパティを文字列にしてTextMeshProのtextへセット
    /// </summary>
    public class PropertyText : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI textMesh = null;

        [SerializeField]
        [LazyProperty(PropertyValueType.Everything, false, false)]
        private LazyProperty targetProperty = new();

        [SerializeField]
        private string format = "";

        private bool started = false;
        private object propertyValue = null;

        private void SetupProperty(bool warn)
        {
            propertyValue = null;
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
            if (textMesh == null)
            {
                return;
            }
            if (!textMesh.IsActive())
            {
                return;
            }
            if (!targetProperty.IsValid())
            {
                return;
            }
            var v = targetProperty.GetValue();
            if (v == null)
            {
                return;
            }
            if ((propertyValue != null) && propertyValue.Equals(v))
            {
                return;
            }
            propertyValue = v;
            var vt = targetProperty.GetValueType();
            textMesh.text = LazyProperty.FormatString(v, vt, format);
        }

        private void Awake()
        {
            SetupProperty(true);
        }
        private void OnEnable()
        {
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                UpdateState();
            }
        }
        private void Start()
        {
            started = true;
            {
                UpdateState();
            }
        }
        private void OnDisable()
        {
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
            UpdateState();
        }
        protected void OnValidate()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TMPro.TextMeshProUGUI>();
            }
            UnityEditor.EditorApplication.delayCall += DelayedUpdate;
        }
#endif
    }
}
