using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LazyUI
{
    /// <summary>
    /// TMP_InputFieldをプロパティに接続
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyInputField : MonoBehaviour
    {
        public enum ExtraContentType
        {
            Nop,
            HexNumber,
        }
        [SerializeField]
        private TMP_InputField inputField = null;

        [SerializeField]
        private LazyInputField lazyInputField = null;

        [SerializeField]
        private int clamp = 0;

        [SerializeField]
        private ExtraContentType extraContentType = ExtraContentType.Nop;

        [SerializeField]
        private string textFormat = "";

        [SerializeField]
        [LazyProperty(PropertyValueType.Int32 | PropertyValueType.Single | PropertyValueType.String, false)]
        private LazyProperty targetProperty = new();

        private bool started = false;
        private object currentValue = null;

        private void UpdateState(bool force)
        {
            if (!targetProperty.IsValid())
            {
                return;
            }
            if (((inputField != null) && inputField.isFocused) || ((lazyInputField != null) && lazyInputField.HasFocus))
            {
                return;
            }
            var value = targetProperty.GetValue();
            if (force || (value != null) && !value.Equals(currentValue))
            {
                currentValue = value;
                SetText(value);
            }
        }
        private void SetText(object propertyValue)
        {
            switch (extraContentType)
            {
                case ExtraContentType.HexNumber:
                    {
                        var txt = LazyProperty.FormatString(propertyValue, targetProperty.GetValueType(), "X08");
                        if ((clamp > 0) && (txt.Length > clamp))
                        {
                            txt = txt.Substring(txt.Length - clamp, clamp);
                        }
                        if (inputField != null)
                        {
                            inputField.text = txt;
                        }
                        if (lazyInputField != null)
                        {
                            lazyInputField.Text = txt;
                        }
                    }
                    break;
                case ExtraContentType.Nop:
                default:
                    {
                        var txt = LazyProperty.FormatString(propertyValue, targetProperty.GetValueType(), textFormat);
                        if (inputField != null)
                        {
                            inputField.text = txt;
                        }
                        if (lazyInputField != null)
                        {
                            lazyInputField.Text = txt;
                        }
                    }
                    break;
            }
        }
        private void SetValue(object value)
        {
            if ((value != null) && !value.Equals(currentValue))
            {
                currentValue = null;
                targetProperty.SetValue(value);
            }
        }
        private void OnSubmit(string text)
        {
            if (!targetProperty.IsValid())
            {
                return;
            }
            switch (extraContentType)
            {
                case ExtraContentType.HexNumber:
                    {
                        if (uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out uint value))
                        {
                            var val = LazyProperty.Convert(value, targetProperty.GetValueType());
                            SetValue(val);
                        }
                    }
                    break;
                case ExtraContentType.Nop:
                default:
                    {
                        if (LazyProperty.TryParse(text, targetProperty.GetValueType(), out var value, targetProperty.GetPropertyType()))
                        {
                            SetValue(value);
                        }
                    }
                    break;
            }
        }
        private void OnValueChanged(string text)
        {
            if (clamp > 0)
            {
                if (text.Length > clamp)
                {
                    text = text[(text.Length - clamp)..];
                    inputField.text = text;
                }
            }
            if (lazyInputField != null)
            {
                lazyInputField.Text = text;
            }
        }
        private void OnValueChangedLazy(string text)
        {
            if (clamp > 0)
            {
                if (text.Length > clamp)
                {
                    text = text[(text.Length - clamp)..];
                    lazyInputField.Text = text;
                }
            }
            if (inputField != null)
            {
                inputField.text = text;
            }
        }
        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            var ch = char.ToUpper(addedChar);
            switch (extraContentType)
            {
                case ExtraContentType.HexNumber:
                    {
                        if ((ch >= '0') && (ch <= '9') || (ch >= 'A') && (ch <= 'F'))
                        {
                            // valid!
                            return ch;
                        }
                        return '\0';
                    }
                case ExtraContentType.Nop:
                default:
                    {
                        return addedChar;
                    }
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
                    LazyDebug.LogWarning($"{transform.GetFullPath()}: [{targetProperty}] is not valid");
                }
                return;
            }
        }
        private void UpdateState()
        {
            UpdateState(false);
        }
        private void Awake()
        {
            SetupProperty(true);
        }
        private void OnEnable()
        {
            if (inputField != null)
            {
                //inputField.onSubmit.AddListener(OnSubmit);
                inputField.onEndEdit.AddListener(OnSubmit);
                inputField.onValueChanged.AddListener(OnValueChanged);
                inputField.onValidateInput += OnValidateInput;
            }
            if (lazyInputField != null)
            {
                lazyInputField.onEndEdit.AddListener(OnSubmit);
                lazyInputField.onValueChanged.AddListener(OnValueChangedLazy);
                lazyInputField.OnValidateInput += OnValidateInput;
            }
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                UpdateState(true);
            }
        }
        private void Start()
        {
            started = true;
            {
                UpdateState(true);
            }
        }
        private void OnDisable()
        {
            currentValue = null;
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (inputField != null)
            {
                //inputField.onSubmit.RemoveListener(OnSubmit);
                inputField.onEndEdit.RemoveListener(OnSubmit);
                inputField.onValueChanged.RemoveListener(OnValueChanged);
                inputField.onValidateInput -= OnValidateInput;
            }
            if (lazyInputField != null)
            {
                lazyInputField.onEndEdit.RemoveListener(OnSubmit);
                lazyInputField.onValueChanged.RemoveListener(OnValueChangedLazy);
                lazyInputField.OnValidateInput -= OnValidateInput;
            }
        }
#if UNITY_EDITOR
        private void Reset()
        {
            inputField = GetComponent<TMP_InputField>();
            lazyInputField = GetComponent<LazyInputField>();
        }
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupProperty(false);
            UpdateState(true);
        }
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
