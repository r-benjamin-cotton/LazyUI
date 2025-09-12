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
    /// 指定のプロパティを直接操作するラジオボタン
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyRadioButton : Selectable, ISubmitHandler
    {
        [Serializable]
        public class PropertyRadioButtonEvent : UnityEvent<bool> { }

        [SerializeField]
        private Image coverGraphic = null;

        [SerializeField]
        private Sprite activeImage = null;

        [SerializeField]
        private bool toggle = false;

        [SerializeField]
        [LazyProperty(PropertyValueType.Everything, true)]
        private LazyProperty targetProperty = new();

        [SerializeField]
        private PropertyRadioButtonEvent onValueChanged = new();

        private bool started = false;
        private bool active = false;
        private bool isOn = false;
        private object propertyValue = null;
        private object previousValue = null;

        public override bool IsInteractable()
        {
            return active && base.IsInteractable();
        }
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                if (!IsInteractable())
                {
                    return;
                }
                if (isOn || !value)
                {
                    return;
                }
                Submit();
                UpdateState(true, false);
            }
        }
        public void Submit()
        {
            if (!IsInteractable())
            {
                return;
            }
            if (isOn)
            {
                if (toggle && (previousValue != null))
                {
                    targetProperty.SetValue(previousValue);
                }
            }
            else
            {
                if (toggle)
                {
                    previousValue = targetProperty.GetValue();
                }
                targetProperty.SetValue(propertyValue);
            }
        }
        private void UpdateValue()
        {
            isOn = false;
            active = false;
            if (!IsActive())
            {
                return;
            }
            if (!targetProperty.IsValid())
            {
                return;
            }
            if (propertyValue == null)
            {
                return;
            }
#if true
            if (!targetProperty.TryTestValue(PropertyTestFunction.Equal, out isOn))
            {
                return;
            }
            active = true;
#else
            var v = targetProperty.GetValue();
            if (v == null)
            {
                return;
            }
            isOn = v.Equals(propertyValue);
            active = true;
#endif
        }
        private void UpdateState(bool notify, bool force)
        {
            var ac = active;
            var on = isOn;
            UpdateValue();
            if (force || (on != isOn) || (ac != active))
            {
                UpdateVisuals();
            }
            if (notify && (on != isOn))
            {
                onValueChanged?.Invoke(isOn);
            }
        }
        private void UpdateVisuals()
        {
            var target = (coverGraphic != null) ? coverGraphic : image;
            if (target != null)
            {
                target.overrideSprite = isOn ? activeImage : null;
                if (ReferenceEquals(target, coverGraphic) && !ReferenceEquals(target.gameObject, this.gameObject))
                {
                    target.gameObject.SetActive(isOn);
                }
            }
#if true
            if (this != null)
            {
                OnDidApplyAnimationProperties();
            }
#endif
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsInteractable())
            {
                return;
            }
            Submit();
        }
        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }
            IsOn = true;
        }
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
            propertyValue = targetProperty.GetSpecificValue();
            if (propertyValue == null)
            {
                if (warn)
                {
                    LazyDebug.LogWarning($"{transform.GetFullPath()}: targetProperty.value is not valid. : {targetProperty}");
                }
            }
        }
        private void UpdateState()
        {
            UpdateState(true, false);
        }
        protected override void Awake()
        {
            base.Awake();
            SetupProperty(true);
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
